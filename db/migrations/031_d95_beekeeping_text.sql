-- 031_d95_beekeeping_text.sql
-- D-95: the description of the capability "Beekeeping enterprise" said that beekeepers have no accounts; since the beekeeper portal
-- (D-80, D-82) a beekeeper registered with a mobile phone can have one. New text in five languages.
-- Base text (English) in Capabilities.Description; translations in ReferenceTranslations. Reference texts only; no business data.
-- Idempotent: a row is written only when its text differs, a missing row is added. SQL Server 2019 (level 150).
-- Run with sqlcmd -I -f 65001 (or in SSMS).

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

DECLARE @texts TABLE (Culture varchar(5) NOT NULL, Value nvarchar(400) NOT NULL);
INSERT @texts (Culture, Value) VALUES
    ('en', N'Able to produce honey. A beekeeper is represented by a Beekeeper record; a beekeeper registered with a mobile phone can also have an account for the beekeeper area.'),
    ('tr', N'Bal üretebilir. Arıcı bir Arıcı kaydı ile temsil edilir; cep telefonuyla kaydedilen arıcının arıcı alanı için bir hesabı da olabilir.'),
    ('es', N'Capaz de producir miel. El apicultor está representado por un registro de apicultor; un apicultor registrado con un teléfono móvil también puede tener una cuenta para el área de apicultor.'),
    ('pl', N'Zdolność do wytwarzania miodu. Pszczelarza reprezentuje rekord pszczelarza; pszczelarz zarejestrowany z telefonem komórkowym może też mieć konto w obszarze pszczelarza.'),
    ('lv', N'Spēj iegūt medu. Biškopi pārstāv biškopja ieraksts; biškopim, kas reģistrēts ar mobilo tālruni, var būt arī konts biškopja sadaļai.');

-- base text (English description)
UPDATE c SET c.Description = t.Value
FROM dbo.Capabilities c JOIN @texts t ON t.Culture = 'en'
WHERE c.Code = 'Beekeeping' AND ISNULL(c.Description, N'') <> t.Value;

-- translations
UPDATE r SET r.Value = t.Value, r.UpdatedAt = SYSUTCDATETIME(), r.UpdatedByUserId = NULL
FROM dbo.ReferenceTranslations r JOIN @texts t ON r.Culture = t.Culture
WHERE r.Entity = 'Capability' AND r.EntityKey = 'Beekeeping' AND r.Field = 'Description' AND t.Culture <> 'en' AND r.Value <> t.Value;

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value, UpdatedAt)
SELECT 'Capability', 'Beekeeping', 'Description', t.Culture, t.Value, SYSUTCDATETIME()
FROM @texts t
WHERE t.Culture <> 'en'
  AND EXISTS (SELECT 1 FROM dbo.Capabilities c WHERE c.Code = 'Beekeeping')
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations r
                  WHERE r.Entity = 'Capability' AND r.EntityKey = 'Beekeeping' AND r.Field = 'Description' AND r.Culture = t.Culture);

COMMIT TRANSACTION;
PRINT N'031: description of Beekeeping enterprise written.';
GO

SET NOEXEC OFF;
GO
