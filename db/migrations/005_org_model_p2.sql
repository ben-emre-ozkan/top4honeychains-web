-- 005_org_model_p2.sql
-- Role and organization model, phase P2 (docs/ROLE-MODEL.md, D-25, D-27, D-35, D-36, D-40).
-- Used by the app only when Features:OrgRoleModel = true.
--   * CCP responsibility matrix as configuration: who (capability) owns a CCP, which org roles enter
--     and which approve; seeded from D-25 for CCP 0-10 (by code)
--   * AssignmentStatuses row 6 "Submitted" (values complete, waiting for the four-eyes approval)
--   * Owning organization on lots and batches (D-36); existing rows are not changed (NULL = the
--     organization created from the lot's / batch's wholesaler, as in P1)
--   * Finalize request / approval columns on batches
--   * CcpCorrections: corrections of CCP values with a second approver (D-35, D-27)
--
-- Additive and idempotent; a second run inserts nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Organizations', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 004_org_model.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

---------------------------------------------------------------------------------------------------
-- 1. CCP responsibility matrix (D-25)
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.CcpResponsibilities', N'U') IS NULL
    CREATE TABLE dbo.CcpResponsibilities
    (
        CcpId            int           NOT NULL CONSTRAINT PK_CcpResponsibilities PRIMARY KEY
                                       CONSTRAINT FK_CcpResponsibilities_Ccp REFERENCES dbo.CriticalControlPoints (Id),
        OwnerRule        nvarchar(30)  NOT NULL,
        RequiresApproval bit           NOT NULL CONSTRAINT DF_CcpResponsibilities_RequiresApproval DEFAULT (1),
        UpdatedAt        datetime2(0)  NOT NULL CONSTRAINT DF_CcpResponsibilities_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedByUserId  nvarchar(450) NULL,
        CONSTRAINT CK_CcpResponsibilities_OwnerRule CHECK (OwnerRule IN (N'Owner', N'BeekeeperAssociation'))
    );
GO
IF OBJECT_ID(N'dbo.CcpResponsibilityCapabilities', N'U') IS NULL
    CREATE TABLE dbo.CcpResponsibilityCapabilities
    (
        CcpId      int          NOT NULL CONSTRAINT FK_CcpRespCaps_Resp REFERENCES dbo.CcpResponsibilities (CcpId),
        Capability nvarchar(30) NOT NULL CONSTRAINT FK_CcpRespCaps_Capability REFERENCES dbo.Capabilities (Code),
        CONSTRAINT PK_CcpResponsibilityCapabilities PRIMARY KEY (CcpId, Capability)
    );
GO
IF OBJECT_ID(N'dbo.CcpResponsibilityRoles', N'U') IS NULL
    CREATE TABLE dbo.CcpResponsibilityRoles
    (
        CcpId   int          NOT NULL CONSTRAINT FK_CcpRespRoles_Resp REFERENCES dbo.CcpResponsibilities (CcpId),
        Action  nvarchar(10) NOT NULL,
        OrgRole nvarchar(30) NOT NULL CONSTRAINT FK_CcpRespRoles_Role REFERENCES dbo.OrgRoles (Code),
        CONSTRAINT PK_CcpResponsibilityRoles PRIMARY KEY (CcpId, Action, OrgRole),
        CONSTRAINT CK_CcpResponsibilityRoles_Action CHECK (Action IN (N'Enter', N'Approve'))
    );
GO

-- Seeds (D-25). Only CCPs without a responsibility row are seeded, so edits in the app are kept.
DECLARE @seed TABLE (Ccp nvarchar(10), OwnerRule nvarchar(30), Caps nvarchar(200), EnterRoles nvarchar(200), ApproveRoles nvarchar(200));
INSERT INTO @seed VALUES
 (N'CCP0',  N'BeekeeperAssociation', N'Association',             N'FieldInspector',                N'QualityApprover'),
 (N'CCP1',  N'Owner',                N'Processing',              N'SampleAcceptanceOfficer,Staff', N'QualityApprover'),
 (N'CCP2',  N'Owner',                N'Processing',              N'Staff',                         N'QualityApprover'),
 (N'CCP3',  N'Owner',                N'Processing',              N'Staff',                         N'QualityApprover'),
 (N'CCP4',  N'Owner',                N'Processing',              N'Staff',                         N'QualityApprover'),
 (N'CCP5',  N'Owner',                N'Processing',              N'Staff',                         N'QualityApprover'),
 (N'CCP6',  N'Owner',                N'Processing',              N'Staff',                         N'QualityApprover'),
 (N'CCP7',  N'Owner',                N'Packing',                 N'Staff',                         N'QualityApprover'),
 (N'CCP8',  N'Owner',                N'Packing',                 N'Staff',                         N'QualityApprover'),
 (N'CCP9',  N'Owner',                N'Packing',                 N'Staff',                         N'QualityApprover'),
 (N'CCP10', N'Owner',                N'Wholesale,Retail,Packing', N'Staff',                        N'QualityApprover');

DECLARE @new TABLE (CcpId int, Ccp nvarchar(10));
INSERT INTO dbo.CcpResponsibilities (CcpId, OwnerRule)
OUTPUT inserted.CcpId, NULL INTO @new
SELECT c.Id, s.OwnerRule
FROM @seed s
JOIN dbo.CriticalControlPoints c ON REPLACE(c.Code, N' ', N'') = s.Ccp
WHERE NOT EXISTS (SELECT 1 FROM dbo.CcpResponsibilities r WHERE r.CcpId = c.Id);
PRINT CONCAT(N'CCP responsibilities seeded: ', @@ROWCOUNT);
UPDATE n SET Ccp = REPLACE(c.Code, N' ', N'') FROM @new n JOIN dbo.CriticalControlPoints c ON c.Id = n.CcpId;

INSERT INTO dbo.CcpResponsibilityCapabilities (CcpId, Capability)
SELECT n.CcpId, LTRIM(v.value)
FROM @new n JOIN @seed s ON s.Ccp = n.Ccp CROSS APPLY STRING_SPLIT(s.Caps, N',') v;
INSERT INTO dbo.CcpResponsibilityRoles (CcpId, Action, OrgRole)
SELECT n.CcpId, N'Enter', LTRIM(v.value)
FROM @new n JOIN @seed s ON s.Ccp = n.Ccp CROSS APPLY STRING_SPLIT(s.EnterRoles, N',') v;
INSERT INTO dbo.CcpResponsibilityRoles (CcpId, Action, OrgRole)
SELECT n.CcpId, N'Approve', LTRIM(v.value)
FROM @new n JOIN @seed s ON s.Ccp = n.Ccp CROSS APPLY STRING_SPLIT(s.ApproveRoles, N',') v;
GO

---------------------------------------------------------------------------------------------------
-- 2. Status "Submitted" (the app uses the fixed id 6)
---------------------------------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.AssignmentStatuses WHERE AssignmentStatusId = 6 AND AssignmentStatusName <> N'Submitted')
   OR EXISTS (SELECT 1 FROM dbo.AssignmentStatuses WHERE AssignmentStatusName = N'Submitted' AND AssignmentStatusId <> 6)
BEGIN
    RAISERROR(N'AssignmentStatuses: id 6 is used by another status (or "Submitted" has another id); aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.AssignmentStatuses WHERE AssignmentStatusId = 6)
BEGIN
    SET IDENTITY_INSERT dbo.AssignmentStatuses ON;
    INSERT INTO dbo.AssignmentStatuses (AssignmentStatusId, AssignmentStatusName, AssignmentStatusDescription)
    VALUES (6, N'Submitted', N'All required values are recorded; waiting for approval by another person');
    SET IDENTITY_INSERT dbo.AssignmentStatuses OFF;
    PRINT N'AssignmentStatuses: "Submitted" (6) added';
END;
GO

---------------------------------------------------------------------------------------------------
-- 3. Owning organization on lots and batches (D-36), finalize request / approval
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.RawHoneyProductions', N'OwnerOrgId') IS NULL
    ALTER TABLE dbo.RawHoneyProductions ADD OwnerOrgId int NULL
        CONSTRAINT FK_RawHoneyProductions_OwnerOrg REFERENCES dbo.Organizations (OrgId);
IF COL_LENGTH(N'dbo.FinalHoneyProductions', N'OwnerOrgId') IS NULL
    ALTER TABLE dbo.FinalHoneyProductions ADD OwnerOrgId int NULL
        CONSTRAINT FK_FinalHoneyProductions_OwnerOrg REFERENCES dbo.Organizations (OrgId);
IF COL_LENGTH(N'dbo.FinalHoneyProductions', N'FinalizeRequestedByUserId') IS NULL
    ALTER TABLE dbo.FinalHoneyProductions ADD
        FinalizeRequestedByUserId nvarchar(450) NULL,
        FinalizeRequestedAt       datetime      NULL,
        FinalizedByUserId         nvarchar(450) NULL,
        SelfFinalized             bit           NOT NULL CONSTRAINT DF_FinalHoneyProductions_SelfFinalized DEFAULT (0);
GO

---------------------------------------------------------------------------------------------------
-- 4. Corrections with a second approver (D-35)
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.CcpCorrections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CcpCorrections
    (
        CorrectionId      int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CcpCorrections PRIMARY KEY,
        OwnerKind         nvarchar(10)  NOT NULL,
        OwnerId           int           NOT NULL,
        CcpId             int           NOT NULL CONSTRAINT FK_CcpCorrections_Ccp REFERENCES dbo.CriticalControlPoints (Id),
        ParameterId       int           NOT NULL CONSTRAINT FK_CcpCorrections_Parameter REFERENCES dbo.CriticalControlPointParameters (ParameterId),
        OldNumericValue   decimal(10,4) NULL,
        OldTextValue      nvarchar(100) NULL,
        NewNumericValue   decimal(10,4) NULL,
        NewTextValue      nvarchar(100) NULL,
        Reason            nvarchar(500) NOT NULL,
        Status            nvarchar(20)  NOT NULL CONSTRAINT DF_CcpCorrections_Status DEFAULT (N'Pending'),
        RequestedByUserId nvarchar(450) NOT NULL,
        RequestedAt       datetime2(0)  NOT NULL CONSTRAINT DF_CcpCorrections_RequestedAt DEFAULT (SYSUTCDATETIME()),
        DecidedByUserId   nvarchar(450) NULL,
        DecidedAt         datetime2(0)  NULL,
        DecisionNote      nvarchar(500) NULL,
        CONSTRAINT CK_CcpCorrections_OwnerKind CHECK (OwnerKind IN (N'raw', N'batch')),
        CONSTRAINT CK_CcpCorrections_Status CHECK (Status IN (N'Pending', N'Applied', N'Rejected')),
        CONSTRAINT CK_CcpCorrections_SecondPerson CHECK (DecidedByUserId IS NULL OR DecidedByUserId <> RequestedByUserId)
    );
    CREATE INDEX IX_CcpCorrections_Owner ON dbo.CcpCorrections (OwnerKind, OwnerId, CcpId);
    PRINT N'Table CcpCorrections created';
END;
GO

SET NOEXEC OFF;
GO
