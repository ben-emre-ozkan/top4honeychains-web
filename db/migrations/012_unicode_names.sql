-- 012_unicode_names.sql
-- Turkish letters in names (D-55). These columns were varchar under the database collation SQL_Latin1_General_CP1_CI_AS,
-- whose code page has no ş, ğ, ı (and İ, Ş, Ğ): "Ayşe" was stored as "Ayse", "Arılığı" as "Ariligi". They become
-- nvarchar of the same length and nullability; nothing is narrowed or dropped. Letters already lost cannot be restored.
--   * Beekeepers.BeekeeperFirstName, Beekeepers.BeekeeperLastName  varchar(50)  -> nvarchar(50)
--   * Apiaries.ApiaryTitle                                         varchar(50)  -> nvarchar(50)
--   * ApiaryPhotos.ApiaryPhotoDesc                                 varchar(max) -> nvarchar(max)
--   * RawHoneyProductionPeriods.RawHoneyProductionPeriodTitle      varchar(50)  -> nvarchar(50)
-- Reference titles (honey types, education levels, beekeeping types and purposes) stay varchar: they carry unique
-- constraints and their base text is English; their translations are in ReferenceTranslations (D-51).
--
-- Idempotent: a column that is already nvarchar is left alone. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Beekeepers' AND COLUMN_NAME = 'BeekeeperFirstName' AND DATA_TYPE = 'varchar')
    ALTER TABLE dbo.Beekeepers ALTER COLUMN BeekeeperFirstName nvarchar(50) NOT NULL;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Beekeepers' AND COLUMN_NAME = 'BeekeeperLastName' AND DATA_TYPE = 'varchar')
    ALTER TABLE dbo.Beekeepers ALTER COLUMN BeekeeperLastName nvarchar(50) NOT NULL;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Apiaries' AND COLUMN_NAME = 'ApiaryTitle' AND DATA_TYPE = 'varchar')
    ALTER TABLE dbo.Apiaries ALTER COLUMN ApiaryTitle nvarchar(50) NOT NULL;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ApiaryPhotos' AND COLUMN_NAME = 'ApiaryPhotoDesc' AND DATA_TYPE = 'varchar')
    ALTER TABLE dbo.ApiaryPhotos ALTER COLUMN ApiaryPhotoDesc nvarchar(max) NULL;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RawHoneyProductionPeriods' AND COLUMN_NAME = 'RawHoneyProductionPeriodTitle' AND DATA_TYPE = 'varchar')
    ALTER TABLE dbo.RawHoneyProductionPeriods ALTER COLUMN RawHoneyProductionPeriodTitle nvarchar(50) NULL;
GO
