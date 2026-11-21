-- 032_d96_platform_admin_name.sql
-- D-96: the capability "Platform administration" was named like the person ("Platform Yöneticisi", "Platformas administrators") in
-- Turkish and Latvian. New names (Turkish approved by a native speaker): tr "Platform yönetimi", lv "Platformas administrēšana".
-- The role "Platform administrator" keeps its names. Reference texts only; no business data.
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
    ('tr', N'Platform yönetimi'),
    ('lv', N'Platformas administrēšana');

UPDATE r SET r.Value = t.Value, r.UpdatedAt = SYSUTCDATETIME(), r.UpdatedByUserId = NULL
FROM dbo.ReferenceTranslations r JOIN @texts t ON r.Culture = t.Culture
WHERE r.Entity = 'Capability' AND r.EntityKey = 'PlatformAdmin' AND r.Field = 'Name' AND r.Value <> t.Value;

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value, UpdatedAt)
SELECT 'Capability', 'PlatformAdmin', 'Name', t.Culture, t.Value, SYSUTCDATETIME()
FROM @texts t
WHERE EXISTS (SELECT 1 FROM dbo.Capabilities c WHERE c.Code = 'PlatformAdmin')
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations r
                  WHERE r.Entity = 'Capability' AND r.EntityKey = 'PlatformAdmin' AND r.Field = 'Name' AND r.Culture = t.Culture);

COMMIT TRANSACTION;
PRINT N'032: names of the capability Platform administration written.';
GO

SET NOEXEC OFF;
GO
