-- 009_reference_translations.sql
-- Full i18n (D-51): translations of reference data shown in dropdowns and tables (honey types, production
-- periods, education levels, beekeeping types and purposes, CCP status names, capabilities, organization
-- roles, CCP titles, parameter categories, parameters and units, limit notes, parameter contents, countries).
--   * ReferenceTranslations: one row per entity row, field and language. The base column of the reference
--     table stays the fallback text; the app shows the translation of the selected language when present.
-- The translations themselves are seeded by 010_reference_translations_seed.sql.
--
-- Additive and idempotent; a second run creates nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ReferenceTranslations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReferenceTranslations
    (
        Entity          varchar(40)    NOT NULL,
        EntityKey       nvarchar(100)  NOT NULL,
        Field           varchar(40)    NOT NULL,
        Culture         varchar(10)    NOT NULL,
        Value           nvarchar(max)  NOT NULL,
        UpdatedAt       datetime2(0)   NOT NULL CONSTRAINT DF_ReferenceTranslations_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedByUserId nvarchar(450)  NULL,
        CONSTRAINT PK_ReferenceTranslations PRIMARY KEY (Entity, EntityKey, Field, Culture),
        CONSTRAINT CK_ReferenceTranslations_Culture CHECK (Culture IN ('en', 'tr', 'es', 'pl', 'lv'))
    );
END;
GO
