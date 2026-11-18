-- 030_d88_plant_batches.sql
-- D-88 (docs/DECISIONS.md): the processing or packing organization creates its own batches ("New batch" in My organization);
-- the platform administrator no longer takes part in the honey's journey.
--   * RolePermissions: area Batch, action Enter (create a batch), for Staff and OrgAdmin. The Roles and permissions screen
--     can change it like every other right.
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (the live database runs at level 140). sqlcmd -I -f 65001.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 013 first (RolePermissions); aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

INSERT dbo.RolePermissions (Area, AreaKey, Action, OrgRole)
SELECT v.Area, '', v.Action, v.OrgRole
FROM (VALUES ('Batch', 'Enter', N'Staff'), ('Batch', 'Enter', N'OrgAdmin')) AS v (Area, Action, OrgRole)
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissions p WHERE p.Area = v.Area AND p.AreaKey = '' AND p.Action = v.Action AND p.OrgRole = v.OrgRole);
PRINT N'030: ' + CONVERT(nvarchar(10), @@ROWCOUNT) + N' role permission(s) added.';
GO

PRINT N'030: the Batch right (Enter) is in place.';
GO

SET NOEXEC OFF;
GO
