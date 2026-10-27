-- 027_d78_tours_seen.sql
-- D-78 P5 (D-84; follow-up from P4, D-83): the first-use tour's "seen" marks are kept per user on the server, so the tour does
-- not reappear on a new device. UserSettings.ToursSeen lists the roles whose tour the user has seen ("Staff,Administrator").
-- Additive: one nullable column; no data is changed. Idempotent. SQL Server 2019 (level 150). Run with sqlcmd -I (or in SSMS).

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.UserSettings', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 021_d78_beekeeper_accounts.sql first (UserSettings); aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

IF COL_LENGTH(N'dbo.UserSettings', N'ToursSeen') IS NULL
    ALTER TABLE dbo.UserSettings ADD ToursSeen nvarchar(400) NULL;
GO

PRINT N'027: UserSettings.ToursSeen is in place.';
GO

SET NOEXEC OFF;
GO
