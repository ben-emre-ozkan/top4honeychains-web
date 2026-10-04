-- 013_role_permissions.sql
-- Editable permission matrix per organization role (D-56), next to the CCP responsibility matrix (which keeps who
-- enters and approves each CCP, 005). This table holds the other rights:
--   * Area 'Ccp' + AreaKey = CCP id, Action 'Read': which roles may open a CCP their organization takes part in
--   * Areas Intake, Samples, LabResults, Shipments, Storefront, Recall, Finalize: Enter / Approve / Read
-- The seed reproduces the rules the code applied before, so nothing changes until an administrator edits the matrix.
-- Fixed in code and not part of the matrix: no self-approval, four-eyes on lab results, append-only audit log,
-- every change audited.
--
-- Additive and idempotent; a second run inserts nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.CcpResponsibilityRoles', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 005_org_model_p2.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions
    (
        Area            varchar(30)   NOT NULL,
        AreaKey         varchar(20)   NOT NULL CONSTRAINT DF_RolePermissions_AreaKey DEFAULT (''),
        Action          varchar(10)   NOT NULL,
        OrgRole         nvarchar(40)  NOT NULL,
        UpdatedAt       datetime2(0)  NOT NULL CONSTRAINT DF_RolePermissions_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedByUserId nvarchar(450) NULL,
        CONSTRAINT PK_RolePermissions PRIMARY KEY (Area, AreaKey, Action, OrgRole),
        CONSTRAINT CK_RolePermissions_Action CHECK (Action IN ('Enter', 'Approve', 'Read'))
    );
END;
GO

BEGIN TRANSACTION;

-- every role reads the CCPs of the items its organization takes part in (rule before the matrix)
INSERT dbo.RolePermissions (Area, AreaKey, Action, OrgRole)
SELECT 'Ccp', CONVERT(varchar(20), c.Id), 'Read', r.Code
FROM dbo.CriticalControlPoints c CROSS JOIN dbo.OrgRoles r
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissions p WHERE p.Area = 'Ccp' AND p.AreaKey = CONVERT(varchar(20), c.Id) AND p.Action = 'Read' AND p.OrgRole = r.Code);

-- the other areas, as the code applied them
INSERT dbo.RolePermissions (Area, AreaKey, Action, OrgRole)
SELECT v.Area, '', v.Action, v.OrgRole
FROM (VALUES
        ('Intake', 'Enter', N'SampleAcceptanceOfficer'),
        ('Intake', 'Read', N'SampleAcceptanceOfficer'), ('Intake', 'Read', N'OrgAdmin'),
        ('Samples', 'Enter', N'SampleAcceptanceOfficer'),
        ('Samples', 'Approve', N'Analyst'), ('Samples', 'Approve', N'LabManager'),
        ('Samples', 'Read', N'SampleAcceptanceOfficer'), ('Samples', 'Read', N'OrgAdmin'), ('Samples', 'Read', N'Analyst'), ('Samples', 'Read', N'LabManager'),
        ('LabResults', 'Enter', N'Analyst'),
        ('LabResults', 'Approve', N'LabManager'),
        ('LabResults', 'Read', N'OrgAdmin'), ('LabResults', 'Read', N'Staff'), ('LabResults', 'Read', N'FieldInspector'), ('LabResults', 'Read', N'SampleAcceptanceOfficer'),
        ('LabResults', 'Read', N'QualityApprover'), ('LabResults', 'Read', N'Analyst'), ('LabResults', 'Read', N'LabManager'),
        ('Shipments', 'Enter', N'Staff'), ('Shipments', 'Enter', N'OrgAdmin'),
        ('Shipments', 'Approve', N'Staff'), ('Shipments', 'Approve', N'OrgAdmin'),
        ('Shipments', 'Read', N'Staff'), ('Shipments', 'Read', N'OrgAdmin'), ('Shipments', 'Read', N'QualityApprover'),
        ('Storefront', 'Enter', N'OrgAdmin'), ('Storefront', 'Read', N'OrgAdmin'),
        ('Recall', 'Read', N'OrgAdmin'), ('Recall', 'Read', N'Staff'), ('Recall', 'Read', N'FieldInspector'), ('Recall', 'Read', N'SampleAcceptanceOfficer'),
        ('Recall', 'Read', N'QualityApprover'), ('Recall', 'Read', N'Analyst'), ('Recall', 'Read', N'LabManager'),
        ('Finalize', 'Enter', N'Staff'), ('Finalize', 'Enter', N'OrgAdmin'),
        ('Finalize', 'Approve', N'QualityApprover')
     ) v(Area, Action, OrgRole)
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissions p WHERE p.Area = v.Area AND p.AreaKey = '' AND p.Action = v.Action AND p.OrgRole = v.OrgRole);

COMMIT TRANSACTION;
GO

SET NOEXEC OFF;
GO
