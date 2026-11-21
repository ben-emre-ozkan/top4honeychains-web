-- The user manuals' fictional database only (setup-db.ps1, step 5b): the sample production period of the cast gets its name in
-- tr, es, pl and lv, as an administrator would enter it under Set-up > Translations of reference data. Idempotent.
SET NOCOUNT ON;
DECLARE @id varchar(20) = (SELECT TOP 1 CAST(RawHoneyProductionPeriodId AS varchar(20)) FROM dbo.RawHoneyProductionPeriods
                           WHERE RawHoneyProductionPeriodTitle = N'2026 Chestnut Honey Harvest Season');
IF @id IS NULL
BEGIN
    RAISERROR(N'The sample production period is missing.', 16, 1);
    RETURN;
END;
DECLARE @t TABLE (Culture varchar(5), Value nvarchar(200));
INSERT @t VALUES
    ('tr', N'2026 Kestane Balı Hasat Sezonu'),
    ('es', N'Temporada de cosecha de miel de castaño 2026'),
    ('pl', N'Sezon zbioru miodu kasztanowego 2026'),
    ('lv', N'2026. gada kastaņu medus ievākšanas sezona');
UPDATE r SET r.Value = t.Value, r.UpdatedAt = SYSUTCDATETIME()
FROM dbo.ReferenceTranslations r JOIN @t t ON r.Culture = t.Culture
WHERE r.Entity = 'ProductionPeriod' AND r.EntityKey = @id AND r.Field = 'Title' AND r.Value <> t.Value;
INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value, UpdatedAt)
SELECT 'ProductionPeriod', @id, 'Title', t.Culture, t.Value, SYSUTCDATETIME() FROM @t t
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations r WHERE r.Entity = 'ProductionPeriod' AND r.EntityKey = @id AND r.Field = 'Title' AND r.Culture = t.Culture);
PRINT N'sample production period named in tr, es, pl, lv';
