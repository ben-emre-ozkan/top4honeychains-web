-- 017_capability_texts.sql
-- D-75: the descriptions of two capabilities still described the old CCP ownership: Association "records data for its member
-- beekeepers (CCP 0)" (CCP 0 moved to the plant with 015) and Processing "CCP 1-6" (lot CCP 0-3 and batch CCP 4-6 today).
-- Rewrites the base text (English, table Capabilities) and the translations (ReferenceTranslations, Field = Description) in
-- Turkish, Spanish, Polish and Latvian. Reference texts only; no business data. Idempotent: a row is written only when its
-- text differs, a missing translation is added. SQL Server 2019 (level 150). Run with sqlcmd -I (or in SSMS).

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Capabilities', N'U') IS NULL OR OBJECT_ID(N'dbo.ReferenceTranslations', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 004_org_model.sql and 009 (reference translations) first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

BEGIN TRANSACTION;

DECLARE @texts TABLE (Code varchar(40) NOT NULL, Culture varchar(5) NOT NULL, Value nvarchar(400) NOT NULL);
INSERT @texts (Code, Culture, Value) VALUES
    ('Association', 'en', N'Beekeepers'' association; links its member beekeepers and records their consent for a public beekeeper page'),
    ('Association', 'tr', N'Arıcı birliği; üye arıcılarını bağlar ve herkese açık arıcı sayfası için rızalarını kaydeder'),
    ('Association', 'es', N'Asociación de apicultores; vincula a sus apicultores miembros y registra su consentimiento para una página pública de apicultor'),
    ('Association', 'pl', N'Stowarzyszenie pszczelarzy; łączy pszczelarzy członkowskich i rejestruje ich zgodę na publiczną stronę pszczelarza'),
    ('Association', 'lv', N'Biškopju biedrība; sasaista savus biedrus biškopjus un reģistrē viņu piekrišanu publiskai biškopja lapai'),
    ('Processing',  'en', N'Reception and processing: deliveries, samples, lot CCP 0–3 and batch CCP 4–6'),
    ('Processing',  'tr', N'Kabul ve işleme: teslim almalar, numuneler, lot KKN 0–3 ve parti KKN 4–6'),
    ('Processing',  'es', N'Recepción y procesamiento: entregas, muestras, PCC 0–3 del lote y PCC 4–6 de la partida'),
    ('Processing',  'pl', N'Przyjęcie i przetwarzanie: dostawy, próbki, KPK 0–3 partii i KPK 4–6 partii końcowej'),
    ('Processing',  'lv', N'Pieņemšana un pārstrāde: piegādes, paraugi, partijas KKP 0–3 un gala partijas KKP 4–6');

-- base text (English)
UPDATE c SET c.Description = t.Value
FROM dbo.Capabilities c JOIN @texts t ON t.Code = c.Code AND t.Culture = 'en'
WHERE ISNULL(c.Description, N'') <> t.Value;

-- translations
UPDATE r SET r.Value = t.Value, r.UpdatedAt = SYSUTCDATETIME(), r.UpdatedByUserId = NULL
FROM dbo.ReferenceTranslations r
JOIN @texts t ON r.Entity = 'Capability' AND r.EntityKey = t.Code AND r.Field = 'Description' AND r.Culture = t.Culture
WHERE t.Culture <> 'en' AND r.Value <> t.Value;

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value, UpdatedAt)
SELECT 'Capability', t.Code, 'Description', t.Culture, t.Value, SYSUTCDATETIME()
FROM @texts t
WHERE t.Culture <> 'en'
  AND EXISTS (SELECT 1 FROM dbo.Capabilities c WHERE c.Code = t.Code)
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations r
                  WHERE r.Entity = 'Capability' AND r.EntityKey = t.Code AND r.Field = 'Description' AND r.Culture = t.Culture);

COMMIT TRANSACTION;
GO

SET NOEXEC OFF;
GO
