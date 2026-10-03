-- 003_ccp_followup.sql
-- CCP follow-up (docs/DECISIONS.md D-18, D-20, D-21):
--   * CriticalControlPoints.PublicTextValues: whether free-text values of a CCP may appear on the public
--     Honey Story page. New column defaults to 0 (hidden); when the column is first added, CCP 1-10 are set
--     to 1 (they showed text before), CCP 0 stays 0 (field audit at the beekeeper, D-12).
--   * CriticalControlPointParameterLimits.HoneyTypeId: a limit for one honey type, overriding the honey
--     class (seed: chestnut honey follows the honeydew rule for electrical conductivity, min 0.8 mS/cm).
--   * CriticalControlPointParameterLimits.RatioWithParameterId: an extra check on value / other value
--     (seed: fructose / glucose 0.9-1.4 for floral honey).
--   * Notes: the pine 23 % moisture seed is doubtful (EU 2001/110: the 23 % exception is for heather).
--   All seeds stay "to verify" (LimitVerified is not touched).
--
-- Additive and idempotent: no drops, renames or type changes; every step is guarded; values edited in
-- the app are not overwritten. Requires 002. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.CriticalControlPointParameterLimits', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 002_ccp_integrity.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

---------------------------------------------------------------------------------------------------
-- 1. Public free-text values per CCP (D-20)
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.CriticalControlPoints', N'PublicTextValues') IS NULL
BEGIN
    ALTER TABLE dbo.CriticalControlPoints ADD PublicTextValues bit NOT NULL
        CONSTRAINT DF_CriticalControlPoints_PublicTextValues DEFAULT (0);
    -- Only when the column is new: keep today's behaviour for CCP 1-10
    EXEC (N'UPDATE dbo.CriticalControlPoints SET PublicTextValues = 1
            WHERE REPLACE(Code, N'' '', N'''') IN (N''CCP1'', N''CCP2'', N''CCP3'', N''CCP4'', N''CCP5'',
                                                  N''CCP6'', N''CCP7'', N''CCP8'', N''CCP9'', N''CCP10'');
           PRINT CONCAT(N''CCPs with public text values: '', @@ROWCOUNT);');
END;
GO

---------------------------------------------------------------------------------------------------
-- 2. Honey-type and ratio limits (D-21)
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.CriticalControlPointParameterLimits', N'HoneyTypeId') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameterLimits ADD HoneyTypeId int NULL
        CONSTRAINT FK_ParameterLimits_HoneyType FOREIGN KEY REFERENCES dbo.HoneyTypes (HoneyTypeId);
IF COL_LENGTH(N'dbo.CriticalControlPointParameterLimits', N'RatioWithParameterId') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameterLimits ADD RatioWithParameterId int NULL
        CONSTRAINT FK_ParameterLimits_RatioWithParameter FOREIGN KEY REFERENCES dbo.CriticalControlPointParameters (ParameterId);
GO
IF OBJECT_ID(N'dbo.CK_ParameterLimits_SumOrRatio', N'C') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameterLimits ADD CONSTRAINT CK_ParameterLimits_SumOrRatio
        CHECK (SumWithParameterId IS NULL OR RatioWithParameterId IS NULL);
GO

DECLARE @Ccp1 int = (SELECT TOP 1 Id FROM dbo.CriticalControlPoints WHERE REPLACE(Code, N' ', N'') = N'CCP1');
DECLARE @Ec int = (SELECT TOP 1 ParameterId FROM dbo.CriticalControlPointParameters WHERE CCPId = @Ccp1 AND ParameterName = N'Electrical Conductivity');
DECLARE @Fru int = (SELECT TOP 1 ParameterId FROM dbo.CriticalControlPointParameters WHERE CCPId = @Ccp1 AND ParameterName = N'Fruktoz');
DECLARE @Glu int = (SELECT TOP 1 ParameterId FROM dbo.CriticalControlPointParameters WHERE CCPId = @Ccp1 AND ParameterName = N'Glikoz');
DECLARE @Chestnut int = (SELECT TOP 1 HoneyTypeId FROM dbo.HoneyTypes WHERE HoneyTypeTitle = 'Chestnut Honey');

-- Chestnut honey: honeydew rule for electrical conductivity
IF @Ec IS NOT NULL AND @Chestnut IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.CriticalControlPointParameterLimits WHERE ParameterId = @Ec AND HoneyTypeId = @Chestnut)
BEGIN
    INSERT INTO dbo.CriticalControlPointParameterLimits (ParameterId, HoneyTypeId, MinValue, Note)
    VALUES (@Ec, @Chestnut, 0.8, N'Chestnut honey follows the honeydew rule (Codex / EU 2001/110); to verify');
    PRINT N'Chestnut conductivity limit seeded';
END;

-- Fructose / glucose ratio for floral honey
IF @Fru IS NOT NULL AND @Glu IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.CriticalControlPointParameterLimits WHERE ParameterId = @Fru AND RatioWithParameterId = @Glu)
BEGIN
    INSERT INTO dbo.CriticalControlPointParameterLimits (ParameterId, HoneyClass, RatioWithParameterId, MinValue, MaxValue, Note)
    VALUES (@Fru, N'Floral', @Glu, 0.9, 1.4, N'Fructose / glucose ratio, floral honey; to verify');
    PRINT N'Fructose/glucose ratio limit seeded';
END;

-- Notes (only where they still hold the 002 seed text)
UPDATE dbo.CriticalControlPointParameters
SET LimitNote = N'Pine honey: max 23 (doubtful: in EU 2001/110 the 23 % exception is for heather / Calluna honey; to verify)'
WHERE LimitNote = N'Pine honey: max 23';
PRINT CONCAT(N'Moisture notes updated: ', @@ROWCOUNT);

UPDATE dbo.CriticalControlPointParameterLimits
SET Note = N'Pine honey (doubtful: the EU 23 % exception is for heather / Calluna honey; to verify)'
WHERE Note = N'Pine honey' AND HoneyClass = N'Pine' AND MaxValue = 23;
PRINT CONCAT(N'Pine moisture limit notes updated: ', @@ROWCOUNT);

UPDATE dbo.CriticalControlPointParameters
SET LimitNote = N'Fructose + glucose: floral min 60, honeydew/pine min 45 g/100 g; fructose/glucose ratio 0.9-1.4 for floral honey'
WHERE ParameterId = @Fru AND LimitNote = N'Fructose + glucose: floral min 60, honeydew/pine min 45 g/100 g';

UPDATE dbo.CriticalControlPointParameters
SET LimitNote = N'Floral max 0.8, honeydew/pine and chestnut min 0.8 mS/cm'
WHERE ParameterId = @Ec AND LimitNote = N'Floral max 0.8, honeydew/pine min 0.8 mS/cm';
GO

SET NOEXEC OFF;
GO
