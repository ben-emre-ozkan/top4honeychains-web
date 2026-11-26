-- 034_d98_ccp_paths.sql
-- D-98 (partner meeting 2026-11-26): two paths after the laboratory, CCP changes, optional laboratory values, the final honey
-- result of a batch and returning a CCP record.
--   * Organizations.CcpPath: 'Association' (honey goes from the approved lab report straight to packing; CCP 2 to 6 do not apply)
--     or 'Company' (processing, CCP 2 onwards as before). NULL = the default: Association when the organization has the
--     Association capability, Company otherwise. The platform administrator can set it.
--   * RawHoneyProductions.PackingReadyAt/By: the Quality Assurance Officer's "ready for packing" on the association path.
--   * Raw/FinalHoneyCCPAssignments.ReturnReason/ReturnedBy/ReturnedAt: a record sent back to the person who entered it
--     (status 4 "Rejected" is shown as "returned").
--   * Samples.FinalHoneyProductionId and Kind 'Batch': a laboratory sample of a batch's final blend (company path). The sample
--     keeps a lot of the batch in RawHoneyProductionId (NOT NULL) only to reuse the lot-based checks; its results are never
--     written to that lot.
--   * CCP parameters: CCP 2 gets "Foam check" (yes/no, required) and "Heating time" (minutes), temperature at most 45 °C
--     (to verify), HMF/diastase/proline/moisture optional; CCP 6 is "Clarification"; CCP 7 asks about physical residue in the jar,
--     CCP 10 whether the jars are kept in the dark; six laboratory values become optional.
--   * CriticalLimit 'Yes'/'No' on a yes/no parameter is the expected answer; another answer is outside the limit.
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (level 150). Run with sqlcmd -I -f 65001.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Samples', N'U') IS NULL OR OBJECT_ID(N'dbo.SealCodes', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 033 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

-- ---------------------------------------------------------------- the path of an organization

IF COL_LENGTH(N'dbo.Organizations', N'CcpPath') IS NULL
    ALTER TABLE dbo.Organizations ADD CcpPath varchar(20) NULL
        CONSTRAINT CK_Organizations_CcpPath CHECK (CcpPath IS NULL OR CcpPath IN ('Association', 'Company'));
GO

IF COL_LENGTH(N'dbo.RawHoneyProductions', N'PackingReadyAt') IS NULL
    ALTER TABLE dbo.RawHoneyProductions ADD
        PackingReadyAt       datetime2     NULL,
        PackingReadyByUserId nvarchar(450) NULL;
GO

-- ---------------------------------------------------------------- returning a CCP record

IF COL_LENGTH(N'dbo.RawHoneyCCPAssignments', N'ReturnReason') IS NULL
    ALTER TABLE dbo.RawHoneyCCPAssignments ADD
        ReturnReason     nvarchar(500) NULL,
        ReturnedByUserId nvarchar(450) NULL,
        ReturnedAt       datetime2     NULL;
GO
IF COL_LENGTH(N'dbo.FinalHoneyCCPAssignments', N'ReturnReason') IS NULL
    ALTER TABLE dbo.FinalHoneyCCPAssignments ADD
        ReturnReason     nvarchar(500) NULL,
        ReturnedByUserId nvarchar(450) NULL,
        ReturnedAt       datetime2     NULL;
GO

-- ---------------------------------------------------------------- the final honey result of a batch

IF COL_LENGTH(N'dbo.Samples', N'FinalHoneyProductionId') IS NULL
    ALTER TABLE dbo.Samples ADD FinalHoneyProductionId int NULL
        CONSTRAINT FK_Samples_FinalHoneyProductions REFERENCES dbo.FinalHoneyProductions (FinalHoneyProductionId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Samples_FinalHoneyProductionId' AND object_id = OBJECT_ID(N'dbo.Samples'))
    CREATE INDEX IX_Samples_FinalHoneyProductionId ON dbo.Samples (FinalHoneyProductionId) WHERE FinalHoneyProductionId IS NOT NULL;
GO
-- widen the kind check: 'Batch' joins 'Container' and 'Composite' (the old constraint is replaced by a wider one)
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Samples_Kind2')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Samples_Kind')
        ALTER TABLE dbo.Samples DROP CONSTRAINT CK_Samples_Kind;
    ALTER TABLE dbo.Samples ADD CONSTRAINT CK_Samples_Kind2
        CHECK (Kind IS NULL OR Kind IN ('Container', 'Composite', 'Batch'));
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Samples_BatchKind')
    ALTER TABLE dbo.Samples ADD CONSTRAINT CK_Samples_BatchKind
        CHECK ((Kind = 'Batch' AND FinalHoneyProductionId IS NOT NULL) OR (ISNULL(Kind, '') <> 'Batch' AND FinalHoneyProductionId IS NULL));
GO

-- ---------------------------------------------------------------- CCP parameters

DECLARE @now datetime2 = SYSDATETIME();
DECLARE @ccp2 int = (SELECT Id FROM dbo.CriticalControlPoints WHERE Code = N'CCP 2');
DECLARE @ccp6 int = (SELECT Id FROM dbo.CriticalControlPoints WHERE Code = N'CCP 6');
DECLARE @ccp7 int = (SELECT Id FROM dbo.CriticalControlPoints WHERE Code = N'CCP 7');
DECLARE @ccp10 int = (SELECT Id FROM dbo.CriticalControlPoints WHERE Code = N'CCP 10');
DECLARE @lab int = (SELECT Id FROM dbo.CriticalControlPoints WHERE Code = N'CCP 1');

-- the category of a CCP is the one its parameters already use
DECLARE @cat2 int = (SELECT TOP 1 ParameterCategory FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp2 AND ParameterCategory IS NOT NULL);
DECLARE @cat6 int = (SELECT TOP 1 ParameterCategory FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp6 AND ParameterCategory IS NOT NULL);
DECLARE @cat7 int = (SELECT TOP 1 ParameterCategory FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp7 AND ParameterCategory IS NOT NULL);
DECLARE @cat10 int = (SELECT TOP 1 ParameterCategory FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp10 AND ParameterCategory IS NOT NULL);

-- CCP 2: new questions
IF @ccp2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp2 AND ParameterName = N'Foam check')
    INSERT INTO dbo.CriticalControlPointParameters (CCPId, ParameterName, ParameterCategory, ParameterUnit, CriticalLimit, DataType, IsRequired, LimitVerified)
    VALUES (@ccp2, N'Foam check', @cat2, N'None', N'Yes', N'YesNo', 1, 0);
IF @ccp2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp2 AND ParameterName = N'Heating time')
    INSERT INTO dbo.CriticalControlPointParameters (CCPId, ParameterName, ParameterCategory, ParameterUnit, DataType, IsRequired, LimitVerified)
    VALUES (@ccp2, N'Heating time', @cat2, N'Minutes', N'Number', 0, 0);

-- CCP 2: temperature at most 45 °C (to verify); the laboratory values become optional (item 7 of the lab verifies them)
UPDATE dbo.CriticalControlPointParameters SET MaxValue = 45, LimitVerified = 0
WHERE CCPId = @ccp2 AND ParameterName = N'Temperature' AND MaxValue IS NULL;
UPDATE dbo.CriticalControlPointParameters SET IsRequired = 0
WHERE CCPId = @ccp2 AND ParameterName IN (N'HMF', N'Diastase', N'Proline', N'Moisture') AND IsRequired = 1;

-- CCP 6 is about clarity, not foam
UPDATE dbo.CriticalControlPoints SET Title = N'Clarification',
    CriticalityJustification = N'Clarity of the honey: after settling, wax particles and sediment are removed so the honey is clear (foam is checked during heating, CCP 2).'
WHERE Id = @ccp6 AND Title = N'Foam and Sediment Removal';
UPDATE dbo.CriticalControlPointParameterCategories SET CategoryDesc = N'Clarification'
WHERE CategoryId = @cat6 AND CategoryDesc = N'Foam and Sediment Removal';
UPDATE dbo.CriticalControlPointParameters SET ParameterName = N'Clarity (visual inspection)'
WHERE CCPId = @ccp6 AND ParameterName = N'Visual Inspection';

-- CCP 7 and CCP 10: new questions
IF @ccp7 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp7 AND ParameterName = N'Is there any physical residue in the jar? (glass fragments etc.)')
    INSERT INTO dbo.CriticalControlPointParameters (CCPId, ParameterName, ParameterCategory, ParameterUnit, CriticalLimit, DataType, IsRequired, LimitVerified)
    VALUES (@ccp7, N'Is there any physical residue in the jar? (glass fragments etc.)', @cat7, N'None', N'No', N'YesNo', 1, 0);
IF @ccp10 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp10 AND ParameterName = N'Are the jars kept in the dark?')
    INSERT INTO dbo.CriticalControlPointParameters (CCPId, ParameterName, ParameterCategory, ParameterUnit, CriticalLimit, DataType, IsRequired, LimitVerified)
    VALUES (@ccp10, N'Are the jars kept in the dark?', @cat10, N'None', N'Yes', N'YesNo', 1, 0);

-- laboratory: six values become optional
UPDATE dbo.CriticalControlPointParameters SET IsRequired = 0
WHERE CCPId = @lab AND ParameterName IN (N'Naphthalene', N'Sensory Analysis', N'C4 Sugar', N'Pesticides', N'Antibiotics', N'Maltoz') AND IsRequired = 1;

-- ---------------------------------------------------------------- translations of the changed reference texts

DECLARE @t TABLE (Entity varchar(40), EntityKey nvarchar(100), Field varchar(40), Culture varchar(10), Value nvarchar(max));
DECLARE @p int;

SET @p = (SELECT ParameterId FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp2 AND ParameterName = N'Foam check');
INSERT INTO @t VALUES
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'tr', N'Köpük kontrolü'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'es', N'Control de espuma'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'pl', N'Kontrola piany'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'lv', N'Putu pārbaude');
SET @p = (SELECT ParameterId FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp2 AND ParameterName = N'Heating time');
INSERT INTO @t VALUES
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'tr', N'Isıtma süresi'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'es', N'Tiempo de calentamiento'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'pl', N'Czas ogrzewania'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'lv', N'Karsēšanas laiks'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Unit', 'tr', N'Dakika'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Unit', 'es', N'Minutos'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Unit', 'pl', N'Minuty'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Unit', 'lv', N'Minūtes');
SET @p = (SELECT ParameterId FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp6 AND ParameterName = N'Clarity (visual inspection)');
INSERT INTO @t VALUES
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'tr', N'Berraklık (gözle kontrol)'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'es', N'Claridad (inspección visual)'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'pl', N'Klarowność (kontrola wzrokowa)'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'lv', N'Dzidrums (vizuāla pārbaude)');
SET @p = (SELECT ParameterId FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp7 AND ParameterName = N'Is there any physical residue in the jar? (glass fragments etc.)');
INSERT INTO @t VALUES
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'tr', N'Kavanozda fiziksel kalıntı var mı? (cam parçası vb.)'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'es', N'¿Hay residuos físicos en el frasco? (fragmentos de vidrio, etc.)'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'pl', N'Czy w słoiku są pozostałości fizyczne? (odłamki szkła itp.)'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'lv', N'Vai burkā ir fiziski piemaisījumi? (stikla lauskas u. c.)');
SET @p = (SELECT ParameterId FROM dbo.CriticalControlPointParameters WHERE CCPId = @ccp10 AND ParameterName = N'Are the jars kept in the dark?');
INSERT INTO @t VALUES
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'tr', N'Kavanozlar karanlıkta muhafaza ediliyor mu?'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'es', N'¿Se conservan los frascos en la oscuridad?'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'pl', N'Czy słoiki są przechowywane w ciemności?'),
 ('Parameter', CAST(@p AS nvarchar(100)), 'Name', 'lv', N'Vai burkas glabā tumsā?');

INSERT INTO @t VALUES
 ('Ccp', CAST(@ccp6 AS nvarchar(100)), 'Title', 'tr', N'Berraklaştırma'),
 ('Ccp', CAST(@ccp6 AS nvarchar(100)), 'Title', 'es', N'Clarificación'),
 ('Ccp', CAST(@ccp6 AS nvarchar(100)), 'Title', 'pl', N'Klarowanie'),
 ('Ccp', CAST(@ccp6 AS nvarchar(100)), 'Title', 'lv', N'Dzidrināšana'),
 ('Ccp', CAST(@ccp6 AS nvarchar(100)), 'CriticalityJustification', 'tr', N'Balın berraklığı: Dinlendirmeden sonra mum parçacıkları ve tortu ayrılır, bal berrak olur (köpük ısıtma sırasında, KKN 2''de kontrol edilir).'),
 ('Ccp', CAST(@ccp6 AS nvarchar(100)), 'CriticalityJustification', 'es', N'Claridad de la miel: tras la decantación se retiran las partículas de cera y los sedimentos para que la miel quede clara (la espuma se controla durante el calentamiento, PCC 2).'),
 ('Ccp', CAST(@ccp6 AS nvarchar(100)), 'CriticalityJustification', 'pl', N'Klarowność miodu: po odstaniu usuwa się cząstki wosku i osad, aby miód był klarowny (pianę kontroluje się podczas ogrzewania, KPK 2).'),
 ('Ccp', CAST(@ccp6 AS nvarchar(100)), 'CriticalityJustification', 'lv', N'Medus dzidrums: pēc nostādināšanas noņem vaska daļiņas un nogulsnes, lai medus būtu dzidrs (putas pārbauda karsēšanas laikā, KKP 2).'),
 ('ParameterCategory', CAST(@cat6 AS nvarchar(100)), 'Description', 'tr', N'Berraklaştırma'),
 ('ParameterCategory', CAST(@cat6 AS nvarchar(100)), 'Description', 'es', N'Clarificación'),
 ('ParameterCategory', CAST(@cat6 AS nvarchar(100)), 'Description', 'pl', N'Klarowanie'),
 ('ParameterCategory', CAST(@cat6 AS nvarchar(100)), 'Description', 'lv', N'Dzidrināšana');

DELETE FROM @t WHERE EntityKey IS NULL;

-- the renamed CCP 6 texts replace the old foam translations; new keys are added; translations edited by hand stay
UPDATE rt SET rt.Value = t.Value, rt.UpdatedAt = @now
FROM dbo.ReferenceTranslations rt
JOIN @t t ON t.Entity = rt.Entity AND t.EntityKey = rt.EntityKey AND t.Field = rt.Field AND t.Culture = rt.Culture
WHERE rt.Value <> t.Value
  AND ((rt.Entity = 'Ccp' AND rt.Field = 'Title' AND rt.Value IN (N'Köpük ve Tortu Giderme', N'Eliminación de espuma y sedimentos', N'Usuwanie piany i osadu', N'Putu un nogulšņu noņemšana'))
    OR (rt.Entity = 'ParameterCategory' AND rt.Value IN (N'Köpük ve Tortu Giderme', N'Eliminación de espuma y sedimentos', N'Usuwanie piany i osadu', N'Putu un nogulšņu noņemšana'))
    OR (rt.Entity = 'Ccp' AND rt.Field = 'CriticalityJustification' AND rt.Value IN (N'Ürünün berraklığını ve görünümünü iyileştirir', N'Mejora la limpidez y el aspecto del producto', N'Poprawia klarowność i wygląd produktu', N'Uzlabo produkta dzidrumu un izskatu'))
    OR (rt.Entity = 'Parameter' AND rt.Field = 'Name' AND rt.Value IN (N'Gözle Kontrol', N'Görsel Kontrol', N'Görsel Muayene', N'Inspección visual', N'Kontrola wizualna', N'Vizuāla pārbaude', N'Vizuālā pārbaude')));

INSERT INTO dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value, UpdatedAt)
SELECT t.Entity, t.EntityKey, t.Field, t.Culture, t.Value, @now
FROM @t t
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations rt
                  WHERE rt.Entity = t.Entity AND rt.EntityKey = t.EntityKey AND rt.Field = t.Field AND rt.Culture = t.Culture);
GO

PRINT '034 done';
GO
