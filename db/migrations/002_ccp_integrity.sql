-- 002_ccp_integrity.sql
-- CCP integrity (docs/DECISIONS.md D-14 to D-19, docs/CCP-CURRENT.md):
--   * CriticalControlPoints.AppliesTo (RawLot / Batch), seeded CCP 0-3 = RawLot, CCP 4-10 = Batch (D-17)
--   * AssignmentStatuses row 5 "Not applicable" plus reason / user / timestamp columns on both
--     assignment tables, enforced by a CHECK constraint (D-15)
--   * Parameter metadata: data type, min, max, required, limit note/source/verified flag (D-18)
--   * HoneyTypes.HoneyClass (Floral / Honeydew / Pine) and CriticalControlPointParameterLimits for
--     honey-class specific, conditional (e.g. diastase when HMF <= 15) and sum (fructose + glucose) limits
--   * Monitoring logs: in-range flag, applied-limit snapshot, stored file name (D-18, D-19)
--   * Seeds: data types and limits from the Turkish Food Codex honey communique, all marked
--     LimitVerified = 0 ("to verify"). Seeds only fill empty values, so re-running changes nothing
--     and values edited in the app are never overwritten.
--
-- Additive and idempotent: no drops, renames or type changes; every step is guarded.
-- Existing CCP assignments, logs and the finalized batches are not changed.
-- SQL Server 2019 (compatibility level 150). Run with sqlcmd -I (QUOTED_IDENTIFIER ON).

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

---------------------------------------------------------------------------------------------------
-- 1. CCP applicability
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.CriticalControlPoints', N'AppliesTo') IS NULL
    ALTER TABLE dbo.CriticalControlPoints ADD AppliesTo nvarchar(10) NULL;
GO
IF OBJECT_ID(N'dbo.CK_CriticalControlPoints_AppliesTo', N'C') IS NULL
    ALTER TABLE dbo.CriticalControlPoints ADD CONSTRAINT CK_CriticalControlPoints_AppliesTo
        CHECK (AppliesTo IS NULL OR AppliesTo IN (N'RawLot', N'Batch'));
GO
UPDATE dbo.CriticalControlPoints SET AppliesTo = N'RawLot'
WHERE AppliesTo IS NULL AND REPLACE(Code, N' ', N'') IN (N'CCP0', N'CCP1', N'CCP2', N'CCP3');
PRINT CONCAT(N'CCPs set to RawLot: ', @@ROWCOUNT);
UPDATE dbo.CriticalControlPoints SET AppliesTo = N'Batch'
WHERE AppliesTo IS NULL AND REPLACE(Code, N' ', N'') IN (N'CCP4', N'CCP5', N'CCP6', N'CCP7', N'CCP8', N'CCP9', N'CCP10');
PRINT CONCAT(N'CCPs set to Batch: ', @@ROWCOUNT);
GO

---------------------------------------------------------------------------------------------------
-- 2. "Not applicable" status (the app uses the fixed id 5, like ids 1-4)
---------------------------------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.AssignmentStatuses WHERE AssignmentStatusId = 5 AND AssignmentStatusName <> N'Not applicable')
   OR EXISTS (SELECT 1 FROM dbo.AssignmentStatuses WHERE AssignmentStatusName = N'Not applicable' AND AssignmentStatusId <> 5)
BEGIN
    RAISERROR(N'AssignmentStatuses: id 5 is used by another status (or "Not applicable" has another id); aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.AssignmentStatuses WHERE AssignmentStatusId = 5)
BEGIN
    SET IDENTITY_INSERT dbo.AssignmentStatuses ON;
    INSERT INTO dbo.AssignmentStatuses (AssignmentStatusId, AssignmentStatusName, AssignmentStatusDescription)
    VALUES (5, N'Not applicable', N'The CCP does not apply to this lot or batch; a reason is required');
    SET IDENTITY_INSERT dbo.AssignmentStatuses OFF;
    PRINT N'AssignmentStatuses: "Not applicable" (5) added';
END;
GO

IF COL_LENGTH(N'dbo.RawHoneyCCPAssignments', N'NotApplicableReason') IS NULL
    ALTER TABLE dbo.RawHoneyCCPAssignments ADD
        NotApplicableReason nvarchar(500) NULL,
        NotApplicableBy     nvarchar(256) NULL,
        NotApplicableAt     datetime      NULL;
GO
IF OBJECT_ID(N'dbo.CK_RawHoneyCCPAssignments_NotApplicable', N'C') IS NULL
    ALTER TABLE dbo.RawHoneyCCPAssignments ADD CONSTRAINT CK_RawHoneyCCPAssignments_NotApplicable
        CHECK (RawHoneyCCPAssignmentStatus <> 5
               OR (NotApplicableReason IS NOT NULL AND LEN(LTRIM(NotApplicableReason)) > 0
                   AND NotApplicableBy IS NOT NULL AND NotApplicableAt IS NOT NULL));
GO
IF COL_LENGTH(N'dbo.FinalHoneyCCPAssignments', N'NotApplicableReason') IS NULL
    ALTER TABLE dbo.FinalHoneyCCPAssignments ADD
        NotApplicableReason nvarchar(500) NULL,
        NotApplicableBy     nvarchar(256) NULL,
        NotApplicableAt     datetime      NULL;
GO
IF OBJECT_ID(N'dbo.CK_FinalHoneyCCPAssignments_NotApplicable', N'C') IS NULL
    ALTER TABLE dbo.FinalHoneyCCPAssignments ADD CONSTRAINT CK_FinalHoneyCCPAssignments_NotApplicable
        CHECK (FinalHoneyCCPAssignmentStatus <> 5
               OR (NotApplicableReason IS NOT NULL AND LEN(LTRIM(NotApplicableReason)) > 0
                   AND NotApplicableBy IS NOT NULL AND NotApplicableAt IS NOT NULL));
GO

---------------------------------------------------------------------------------------------------
-- 3. Parameter metadata
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.CriticalControlPointParameters', N'DataType') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD DataType nvarchar(10) NULL;
IF COL_LENGTH(N'dbo.CriticalControlPointParameters', N'MinValue') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD MinValue decimal(10,4) NULL;
IF COL_LENGTH(N'dbo.CriticalControlPointParameters', N'MaxValue') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD MaxValue decimal(10,4) NULL;
IF COL_LENGTH(N'dbo.CriticalControlPointParameters', N'IsRequired') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD IsRequired bit NOT NULL
        CONSTRAINT DF_CriticalControlPointParameters_IsRequired DEFAULT (1);
IF COL_LENGTH(N'dbo.CriticalControlPointParameters', N'LimitNote') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD LimitNote nvarchar(300) NULL;
IF COL_LENGTH(N'dbo.CriticalControlPointParameters', N'LimitSource') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD LimitSource nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.CriticalControlPointParameters', N'LimitVerified') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD LimitVerified bit NOT NULL
        CONSTRAINT DF_CriticalControlPointParameters_LimitVerified DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.CK_CriticalControlPointParameters_DataType', N'C') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD CONSTRAINT CK_CriticalControlPointParameters_DataType
        CHECK (DataType IS NULL OR DataType IN (N'Number', N'YesNo', N'Text', N'File'));
IF OBJECT_ID(N'dbo.CK_CriticalControlPointParameters_MinMax', N'C') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD CONSTRAINT CK_CriticalControlPointParameters_MinMax
        CHECK (MinValue IS NULL OR MaxValue IS NULL OR MinValue <= MaxValue);
GO

---------------------------------------------------------------------------------------------------
-- 4. Honey class and honey-class specific / conditional limits
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.HoneyTypes', N'HoneyClass') IS NULL
    ALTER TABLE dbo.HoneyTypes ADD HoneyClass nvarchar(20) NULL;
GO
IF OBJECT_ID(N'dbo.CK_HoneyTypes_HoneyClass', N'C') IS NULL
    ALTER TABLE dbo.HoneyTypes ADD CONSTRAINT CK_HoneyTypes_HoneyClass
        CHECK (HoneyClass IS NULL OR HoneyClass IN (N'Floral', N'Honeydew', N'Pine'));
GO
UPDATE dbo.HoneyTypes SET HoneyClass = N'Pine'
WHERE HoneyClass IS NULL AND HoneyTypeTitle = 'Pine Honey';
UPDATE dbo.HoneyTypes SET HoneyClass = N'Floral'
WHERE HoneyClass IS NULL
  AND HoneyTypeTitle IN ('Flower Honey', 'Thyme Honey', 'Sunflower Honey', 'Citrus Honey', 'Chestnut Honey', 'Highland Honey');
GO

IF OBJECT_ID(N'dbo.CriticalControlPointParameterLimits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CriticalControlPointParameterLimits
    (
        LimitId              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CriticalControlPointParameterLimits PRIMARY KEY,
        ParameterId          int           NOT NULL,
        HoneyClass           nvarchar(20)  NULL,  -- NULL = every honey class
        ConditionParameterId int           NULL,  -- applies only if that parameter's value (same lot/batch) ...
        ConditionMaxValue    decimal(10,4) NULL,  -- ... is <= this value
        SumWithParameterId   int           NULL,  -- the limit applies to value + that parameter's value
        MinValue             decimal(10,4) NULL,
        MaxValue             decimal(10,4) NULL,
        Note                 nvarchar(300) NULL,
        CONSTRAINT FK_ParameterLimits_Parameter FOREIGN KEY (ParameterId)
            REFERENCES dbo.CriticalControlPointParameters (ParameterId),
        CONSTRAINT FK_ParameterLimits_ConditionParameter FOREIGN KEY (ConditionParameterId)
            REFERENCES dbo.CriticalControlPointParameters (ParameterId),
        CONSTRAINT FK_ParameterLimits_SumWithParameter FOREIGN KEY (SumWithParameterId)
            REFERENCES dbo.CriticalControlPointParameters (ParameterId),
        CONSTRAINT CK_ParameterLimits_HoneyClass CHECK (HoneyClass IS NULL OR HoneyClass IN (N'Floral', N'Honeydew', N'Pine')),
        CONSTRAINT CK_ParameterLimits_Condition CHECK ((ConditionParameterId IS NULL AND ConditionMaxValue IS NULL)
                                                       OR (ConditionParameterId IS NOT NULL AND ConditionMaxValue IS NOT NULL)),
        CONSTRAINT CK_ParameterLimits_MinMax CHECK ((MinValue IS NOT NULL OR MaxValue IS NOT NULL)
                                                    AND (MinValue IS NULL OR MaxValue IS NULL OR MinValue <= MaxValue))
    );
    CREATE INDEX IX_ParameterLimits_Parameter ON dbo.CriticalControlPointParameterLimits (ParameterId);
    PRINT N'Table CriticalControlPointParameterLimits created';
END;
GO

---------------------------------------------------------------------------------------------------
-- 5. Monitoring logs: in-range flag, applied limit, stored file
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.CriticalControlPointMonitoringLogs', N'IsInRange') IS NULL
    ALTER TABLE dbo.CriticalControlPointMonitoringLogs ADD
        IsInRange      bit           NULL,
        AppliedLimit   nvarchar(200) NULL,
        StoredFileName nvarchar(260) NULL;
GO

---------------------------------------------------------------------------------------------------
-- 6. Seeds (only empty values are filled; all limits are marked "to verify")
---------------------------------------------------------------------------------------------------
DECLARE @Source nvarchar(200) = N'Turkish Food Codex, honey communique (seed, to verify)';

-- Parameters are identified by CCP code and parameter name, not by id.
DECLARE @p TABLE (Ccp nvarchar(10), Name nvarchar(500), DataType nvarchar(10),
                  MinValue decimal(10,4) NULL, MaxValue decimal(10,4) NULL, Note nvarchar(300) NULL, Unit nvarchar(50) NULL);

INSERT INTO @p (Ccp, Name, DataType, MinValue, MaxValue, Note, Unit) VALUES
 (N'CCP1', N'Moisture',                N'Number', NULL,  20,   N'Pine honey: max 23', NULL),
 (N'CCP1', N'HMF',                     N'Number', NULL,  40,   NULL, NULL),
 (N'CCP1', N'Diastase',                N'Number', 8,     NULL, N'Min 3 if HMF <= 15 mg/kg', NULL),
 (N'CCP1', N'C4 Sugar',                N'Number', NULL,  7,    NULL, N'%'),
 (N'CCP1', N'Sucrose',                 N'Number', NULL,  5,    N'g/100 g; max 10 for specific honeys (e.g. Robinia, citrus)', NULL),
 (N'CCP1', N'Glikoz',                  N'Number', NULL,  NULL, N'Checked as fructose + glucose (see Fruktoz)', NULL),
 (N'CCP1', N'Fruktoz',                 N'Number', NULL,  NULL, N'Fructose + glucose: floral min 60, honeydew/pine min 45 g/100 g', NULL),
 (N'CCP1', N'Maltoz',                  N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP1', N'Pesticides',              N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP1', N'Antibiotics',             N'Number', NULL,  0,    N'Veterinary drug residues: zero tolerance', NULL),
 (N'CCP1', N'Electrical Conductivity', N'Number', NULL,  NULL, N'Floral max 0.8, honeydew/pine min 0.8 mS/cm', N'mS/cm'),
 (N'CCP1', N'Proline',                 N'Number', 300,   NULL, NULL, NULL),
 (N'CCP1', N'pH',                      N'Number', 3.2,   4.5,  NULL, NULL),
 (N'CCP1', N'Free Acidity',            N'Number', NULL,  50,   NULL, NULL),
 (N'CCP1', N'Insoluble Matter',        N'Number', NULL,  NULL, NULL, N'%'),
 (N'CCP1', N'Sensory Analysis',        N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP1', N'Naphthalene',             N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP2', N'Temperature',             N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP2', N'HMF',                     N'Number', NULL,  40,   NULL, NULL),
 (N'CCP2', N'Diastase',                N'Number', 8,     NULL, N'Min 3 if HMF <= 15 mg/kg', NULL),
 (N'CCP2', N'Moisture',                N'Number', NULL,  20,   N'Pine honey: max 23', NULL),
 (N'CCP2', N'Proline',                 N'Number', 300,   NULL, NULL, NULL),
 (N'CCP3', N'Filter integrity',        N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP3', N'Sediment presence',       N'YesNo',  NULL,  NULL, NULL, NULL),
 (N'CCP3', N'Pore size',               N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP4', N'Visual appearance',       N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP4', N'Viscosity',               N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP4', N'Color consistency',       N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP4', N'Moisture',                N'Number', NULL,  20,   N'Pine honey: max 23', NULL),
 (N'CCP5', N'Temperature',             N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP5', N'Holding Time',            N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP5', N'HMF',                     N'Number', NULL,  40,   NULL, NULL),
 (N'CCP5', N'Diastase',                N'Number', 8,     NULL, N'Min 3 if HMF <= 15 mg/kg', NULL),
 (N'CCP5', N'Moisture',                N'Number', NULL,  20,   N'Pine honey: max 23', NULL),
 (N'CCP5', N'Proline',                 N'Number', 300,   NULL, NULL, NULL),
 (N'CCP6', N'Visual Inspection',       N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP6', N'Holding Time',            N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP6', N'Temperature',             N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP7', N'Net weight',              N'Number', NULL,  NULL, NULL, NULL),
 (N'CCP7', N'Visual Inspection',       N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP8', N'Origin',                  N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP8', N'Botanical source',        N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP8', N'Analytical consistency',  N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP9', N'Material type',           N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP9', N'Cleanliness',             N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP9', N'Physical durability',     N'Text',   NULL,  NULL, NULL, NULL),
 (N'CCP10', N'Temperature',            N'Number', NULL,  25,   N'Storage temperature', NULL),
 (N'CCP10', N'Relative Humidity',      N'Number', NULL,  NULL, NULL, NULL);

-- CCP 0 audit questions: yes/no, except the three that ask "how" / "which" / "to what extent"
UPDATE p SET DataType = CASE WHEN p.ParameterName LIKE N'If yes, which methods%'
                               OR p.ParameterName LIKE N'Are environmental factors%'
                               OR p.ParameterName LIKE N'To what extent%' THEN N'Text' ELSE N'YesNo' END
FROM dbo.CriticalControlPointParameters p
JOIN dbo.CriticalControlPoints c ON c.Id = p.CCPId
WHERE REPLACE(c.Code, N' ', N'') = N'CCP0' AND p.DataType IS NULL;
PRINT CONCAT(N'CCP 0 data types set: ', @@ROWCOUNT);

UPDATE p SET DataType = s.DataType
FROM dbo.CriticalControlPointParameters p
JOIN dbo.CriticalControlPoints c ON c.Id = p.CCPId
JOIN @p s ON s.Ccp = REPLACE(c.Code, N' ', N'') AND s.Name = p.ParameterName
WHERE p.DataType IS NULL;
PRINT CONCAT(N'Data types set: ', @@ROWCOUNT);

-- Limits and notes: only on parameters that have no limit data yet
UPDATE p SET MinValue = s.MinValue, MaxValue = s.MaxValue, LimitNote = s.Note, LimitSource = @Source, LimitVerified = 0
FROM dbo.CriticalControlPointParameters p
JOIN dbo.CriticalControlPoints c ON c.Id = p.CCPId
JOIN @p s ON s.Ccp = REPLACE(c.Code, N' ', N'') AND s.Name = p.ParameterName
WHERE (s.MinValue IS NOT NULL OR s.MaxValue IS NOT NULL OR s.Note IS NOT NULL)
  AND p.MinValue IS NULL AND p.MaxValue IS NULL AND p.LimitNote IS NULL AND p.LimitSource IS NULL;
PRINT CONCAT(N'Default limits seeded: ', @@ROWCOUNT);

-- Units written inconsistently ("mscm", "pct"); only the exact old values are replaced
UPDATE p SET ParameterUnit = s.Unit
FROM dbo.CriticalControlPointParameters p
JOIN dbo.CriticalControlPoints c ON c.Id = p.CCPId
JOIN @p s ON s.Ccp = REPLACE(c.Code, N' ', N'') AND s.Name = p.ParameterName
WHERE s.Unit IS NOT NULL AND p.ParameterUnit IN (N'mscm', N'pct');
PRINT CONCAT(N'Units normalised: ', @@ROWCOUNT);

-- Honey-class, conditional and sum limits
DECLARE @l TABLE (Ccp nvarchar(10), Name nvarchar(500), HoneyClass nvarchar(20) NULL,
                  ConditionName nvarchar(500) NULL, ConditionMax decimal(10,4) NULL, SumWithName nvarchar(500) NULL,
                  MinValue decimal(10,4) NULL, MaxValue decimal(10,4) NULL, Note nvarchar(300));
INSERT INTO @l VALUES
 (N'CCP1', N'Moisture', N'Pine', NULL, NULL, NULL, NULL, 23, N'Pine honey'),
 (N'CCP2', N'Moisture', N'Pine', NULL, NULL, NULL, NULL, 23, N'Pine honey'),
 (N'CCP4', N'Moisture', N'Pine', NULL, NULL, NULL, NULL, 23, N'Pine honey'),
 (N'CCP5', N'Moisture', N'Pine', NULL, NULL, NULL, NULL, 23, N'Pine honey'),
 (N'CCP1', N'Diastase', NULL, N'HMF', 15, NULL, 3, NULL, N'Honey with low natural enzyme content (HMF <= 15 mg/kg)'),
 (N'CCP2', N'Diastase', NULL, N'HMF', 15, NULL, 3, NULL, N'Honey with low natural enzyme content (HMF <= 15 mg/kg)'),
 (N'CCP5', N'Diastase', NULL, N'HMF', 15, NULL, 3, NULL, N'Honey with low natural enzyme content (HMF <= 15 mg/kg)'),
 (N'CCP1', N'Electrical Conductivity', N'Floral',   NULL, NULL, NULL, NULL, 0.8, N'Floral (blossom) honey'),
 (N'CCP1', N'Electrical Conductivity', N'Honeydew', NULL, NULL, NULL, 0.8, NULL, N'Honeydew honey'),
 (N'CCP1', N'Electrical Conductivity', N'Pine',     NULL, NULL, NULL, 0.8, NULL, N'Pine (honeydew) honey'),
 (N'CCP1', N'Fruktoz', N'Floral',   NULL, NULL, N'Glikoz', 60, NULL, N'Fructose + glucose, floral honey (g/100 g)'),
 (N'CCP1', N'Fruktoz', N'Honeydew', NULL, NULL, N'Glikoz', 45, NULL, N'Fructose + glucose, honeydew honey (g/100 g)'),
 (N'CCP1', N'Fruktoz', N'Pine',     NULL, NULL, N'Glikoz', 45, NULL, N'Fructose + glucose, pine (honeydew) honey (g/100 g)');

INSERT INTO dbo.CriticalControlPointParameterLimits
    (ParameterId, HoneyClass, ConditionParameterId, ConditionMaxValue, SumWithParameterId, MinValue, MaxValue, Note)
SELECT p.ParameterId, s.HoneyClass, cp.ParameterId, s.ConditionMax, sp.ParameterId, s.MinValue, s.MaxValue, s.Note
FROM @l s
JOIN dbo.CriticalControlPoints c ON REPLACE(c.Code, N' ', N'') = s.Ccp
JOIN dbo.CriticalControlPointParameters p ON p.CCPId = c.Id AND p.ParameterName = s.Name
LEFT JOIN dbo.CriticalControlPointParameters cp ON cp.CCPId = c.Id AND cp.ParameterName = s.ConditionName
LEFT JOIN dbo.CriticalControlPointParameters sp ON sp.CCPId = c.Id AND sp.ParameterName = s.SumWithName
WHERE (s.ConditionName IS NULL OR cp.ParameterId IS NOT NULL)
  AND (s.SumWithName IS NULL OR sp.ParameterId IS NOT NULL)
  AND NOT EXISTS (SELECT 1 FROM dbo.CriticalControlPointParameterLimits x
                  WHERE x.ParameterId = p.ParameterId
                    AND ISNULL(x.HoneyClass, N'') = ISNULL(s.HoneyClass, N'')
                    AND ISNULL(x.ConditionParameterId, 0) = ISNULL(cp.ParameterId, 0)
                    AND ISNULL(x.SumWithParameterId, 0) = ISNULL(sp.ParameterId, 0));
PRINT CONCAT(N'Honey-class / conditional limits seeded: ', @@ROWCOUNT);

-- Parameters that only have limit rows also get the source/verified marker
UPDATE p SET LimitSource = @Source
FROM dbo.CriticalControlPointParameters p
WHERE p.LimitSource IS NULL
  AND EXISTS (SELECT 1 FROM dbo.CriticalControlPointParameterLimits x WHERE x.ParameterId = p.ParameterId);
GO

SET NOEXEC OFF;
GO
