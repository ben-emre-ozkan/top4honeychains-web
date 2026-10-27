-- 022_d78_apiary_title.sql
-- D-78 P1 (docs/design/D78-SPEC.md 3.6, 4.2, F5, D-80): apiary titles are generated from province - district - locality and no
-- longer typed. Adds the nullable parts to Apiaries:
--   * Province    char(2)      plate code (table Provinces)
--   * DistrictId  int          table Districts
--   * Locality    nvarchar(60) village, plateau or field name, typed
--   * TitleSource varchar(20)  'Generated'; NULL = typed (every existing apiary)
-- The existing titles (ApiaryTitle) are not touched: they stay as typed until an apiary is edited with its three parts.
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Districts', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 011_provinces_districts.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

IF COL_LENGTH(N'dbo.Apiaries', N'Province') IS NULL
    ALTER TABLE dbo.Apiaries ADD
        Province    char(2)      NULL CONSTRAINT FK_Apiaries_Provinces REFERENCES dbo.Provinces (Code),
        DistrictId  int          NULL CONSTRAINT FK_Apiaries_Districts REFERENCES dbo.Districts (DistrictId),
        Locality    nvarchar(60) NULL,
        TitleSource varchar(20)  NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Apiaries_TitleSource')
    ALTER TABLE dbo.Apiaries ADD CONSTRAINT CK_Apiaries_TitleSource CHECK (TitleSource IS NULL OR TitleSource IN ('Generated', 'Typed'));
GO

PRINT N'022: Apiaries.Province, DistrictId, Locality and TitleSource are in place; existing titles unchanged.';
GO

SET NOEXEC OFF;
GO
