-- 004_org_model.sql
-- Role and organization model, phase P1 (docs/ROLE-MODEL.md, docs/DECISIONS.md D-22 to D-32).
-- Used by the app only when Features:OrgRoleModel = true; with the flag off nothing reads these tables.
--   * Lookups: Capabilities, OrgRoles (codes only, no names of people or companies)
--   * Organizations, OrgCapabilities, OrgDocuments, OrgVerificationEvents, Memberships
--   * Beekeepers as records linked to an association, without an account: BeekeeperAssociations,
--     BeekeeperRegistry (AKS enterprise number and its status), RegistryVerifications (requests)
--   * AuditEntries: append-only (triggers reject UPDATE and DELETE), hash-chained (unique PrevHash)
--   * CCP work: who recorded a value, who approved a CCP record (four-eyes, D-27)
--   * Data migration (insert only, never updates or deletes existing rows):
--       - one platform organization (capability PlatformAdmin, Verified)
--       - an OrgAdmin membership in it for every user in the Administrator role
--       - one organization per Wholesalers row (Pending; capabilities Processing, Packing, Wholesale, D-24)
--
-- Additive and idempotent: every step is guarded; a second run inserts nothing.
-- SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

---------------------------------------------------------------------------------------------------
-- 1. Lookups
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Capabilities', N'U') IS NULL
    CREATE TABLE dbo.Capabilities
    (
        Code        nvarchar(30)  NOT NULL CONSTRAINT PK_Capabilities PRIMARY KEY,
        Description nvarchar(300) NOT NULL,
        SortOrder   int           NOT NULL
    );
GO
INSERT INTO dbo.Capabilities (Code, Description, SortOrder)
SELECT v.Code, v.Description, v.SortOrder
FROM (VALUES
    (N'Beekeeping',         N'Beekeeping enterprise (no login; registry number required)', 1),
    (N'Association',        N'Beekeeper association; records data for its member beekeepers (CCP 0)', 2),
    (N'CentralAssociation', N'Central beekeepers'' association', 3),
    (N'Processing',         N'Reception and processing, CCP 1-6 (an association or a packer)', 4),
    (N'Packing',            N'Filling, labelling and packaging, CCP 7-9', 5),
    (N'Laboratory',         N'Laboratory (accredited or in-house)', 6),
    (N'Wholesale',          N'Wholesale; holder at CCP 10 while it has the goods', 7),
    (N'Retail',             N'Retail; holder at CCP 10 while it has the goods', 8),
    (N'PublicAuthority',    N'Public authority, read-only', 9),
    (N'PlatformAdmin',      N'Runs the platform', 10)
) v (Code, Description, SortOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Capabilities c WHERE c.Code = v.Code);
PRINT CONCAT(N'Capabilities added: ', @@ROWCOUNT);
GO

IF OBJECT_ID(N'dbo.OrgRoles', N'U') IS NULL
    CREATE TABLE dbo.OrgRoles
    (
        Code        nvarchar(30)  NOT NULL CONSTRAINT PK_OrgRoles PRIMARY KEY,
        Description nvarchar(300) NOT NULL,
        SortOrder   int           NOT NULL
    );
GO
INSERT INTO dbo.OrgRoles (Code, Description, SortOrder)
SELECT v.Code, v.Description, v.SortOrder
FROM (VALUES
    (N'OrgAdmin',                N'Manages the organization''s documents; reads its data', 1),
    (N'Staff',                   N'Enters CCP values and operational records', 2),
    (N'FieldInspector',          N'Enters the field audit at the beekeeper (CCP 0)', 3),
    (N'SampleAcceptanceOfficer', N'Lot intake and sampling (CCP 1 reception)', 4),
    (N'QualityApprover',         N'Approves CCP records (four-eyes)', 5),
    (N'Analyst',                 N'Enters lab results', 6),
    (N'LabManager',              N'Approves lab results', 7)
) v (Code, Description, SortOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.OrgRoles r WHERE r.Code = v.Code);
PRINT CONCAT(N'Org roles added: ', @@ROWCOUNT);
GO

---------------------------------------------------------------------------------------------------
-- 2. Organizations
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Organizations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Organizations
    (
        OrgId              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Organizations PRIMARY KEY,
        DisplayName        nvarchar(200) NOT NULL,
        LegalName          nvarchar(300) NULL,
        TaxNo              nvarchar(50)  NULL,
        Province           nvarchar(100) NULL,
        District           nvarchar(100) NULL,
        Status             nvarchar(20)  NOT NULL CONSTRAINT DF_Organizations_Status DEFAULT (N'Pending'),
        IsPlatform         bit           NOT NULL CONSTRAINT DF_Organizations_IsPlatform DEFAULT (0),
        FourEyesExempt     bit           NOT NULL CONSTRAINT DF_Organizations_FourEyesExempt DEFAULT (0),
        LegacyWholesalerId int           NULL CONSTRAINT FK_Organizations_Wholesalers REFERENCES dbo.Wholesalers (WholesalerId),
        VerifiedAt         datetime2(0)  NULL,
        VerifiedByUserId   nvarchar(450) NULL,
        CreatedAt          datetime2(0)  NOT NULL CONSTRAINT DF_Organizations_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedByUserId    nvarchar(450) NULL,
        CONSTRAINT CK_Organizations_Status CHECK (Status IN (N'Pending', N'Verified', N'Suspended'))
    );
    CREATE UNIQUE INDEX UX_Organizations_LegacyWholesaler ON dbo.Organizations (LegacyWholesalerId) WHERE LegacyWholesalerId IS NOT NULL;
    CREATE UNIQUE INDEX UX_Organizations_Platform ON dbo.Organizations (IsPlatform) WHERE IsPlatform = 1;
    PRINT N'Table Organizations created';
END;
GO

IF OBJECT_ID(N'dbo.OrgCapabilities', N'U') IS NULL
    CREATE TABLE dbo.OrgCapabilities
    (
        OrgId                   int           NOT NULL CONSTRAINT FK_OrgCapabilities_Org REFERENCES dbo.Organizations (OrgId),
        Capability              nvarchar(30)  NOT NULL CONSTRAINT FK_OrgCapabilities_Capability REFERENCES dbo.Capabilities (Code),
        IsAccredited            bit           NOT NULL CONSTRAINT DF_OrgCapabilities_IsAccredited DEFAULT (0),
        AccreditationNumber     nvarchar(100) NULL,
        AccreditationValidUntil date          NULL,
        AddedAt                 datetime2(0)  NOT NULL CONSTRAINT DF_OrgCapabilities_AddedAt DEFAULT (SYSUTCDATETIME()),
        AddedByUserId           nvarchar(450) NULL,
        CONSTRAINT PK_OrgCapabilities PRIMARY KEY (OrgId, Capability)
    );
GO

IF OBJECT_ID(N'dbo.OrgDocuments', N'U') IS NULL
    CREATE TABLE dbo.OrgDocuments
    (
        DocumentId       int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrgDocuments PRIMARY KEY,
        OrgId            int           NOT NULL CONSTRAINT FK_OrgDocuments_Org REFERENCES dbo.Organizations (OrgId),
        Kind             nvarchar(30)  NOT NULL,
        OriginalFileName nvarchar(255) NOT NULL,
        StoredFileName   nvarchar(260) NOT NULL,
        Sha256           char(64)      NOT NULL,
        UploadedByUserId nvarchar(450) NOT NULL,
        UploadedAt       datetime2(0)  NOT NULL CONSTRAINT DF_OrgDocuments_UploadedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_OrgDocuments_Kind CHECK (Kind IN (N'Licence', N'Accreditation', N'TaxCertificate', N'RegistryProof', N'Other'))
    );
GO

IF OBJECT_ID(N'dbo.OrgVerificationEvents', N'U') IS NULL
    CREATE TABLE dbo.OrgVerificationEvents
    (
        EventId    int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrgVerificationEvents PRIMARY KEY,
        OrgId      int           NOT NULL CONSTRAINT FK_OrgVerificationEvents_Org REFERENCES dbo.Organizations (OrgId),
        FromStatus nvarchar(20)  NOT NULL,
        ToStatus   nvarchar(20)  NOT NULL,
        Reason     nvarchar(500) NULL,
        ByUserId   nvarchar(450) NOT NULL,
        At         datetime2(0)  NOT NULL CONSTRAINT DF_OrgVerificationEvents_At DEFAULT (SYSUTCDATETIME())
    );
GO

IF OBJECT_ID(N'dbo.Memberships', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Memberships
    (
        MembershipId     int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Memberships PRIMARY KEY,
        UserId           nvarchar(450) NOT NULL CONSTRAINT FK_Memberships_User REFERENCES dbo.AspNetUsers (Id),
        OrgId            int           NOT NULL CONSTRAINT FK_Memberships_Org REFERENCES dbo.Organizations (OrgId),
        OrgRole          nvarchar(30)  NOT NULL CONSTRAINT FK_Memberships_Role REFERENCES dbo.OrgRoles (Code),
        Status           nvarchar(20)  NOT NULL CONSTRAINT DF_Memberships_Status DEFAULT (N'Active'),
        GrantedByUserId  nvarchar(450) NULL,
        GrantedAt        datetime2(0)  NOT NULL CONSTRAINT DF_Memberships_GrantedAt DEFAULT (SYSUTCDATETIME()),
        RevokedByUserId  nvarchar(450) NULL,
        RevokedAt        datetime2(0)  NULL,
        CONSTRAINT CK_Memberships_Status CHECK (Status IN (N'Active', N'Revoked'))
    );
    CREATE UNIQUE INDEX UX_Memberships_Active ON dbo.Memberships (UserId, OrgId, OrgRole) WHERE Status = N'Active';
    CREATE INDEX IX_Memberships_Org ON dbo.Memberships (OrgId);
    PRINT N'Table Memberships created';
END;
GO

---------------------------------------------------------------------------------------------------
-- 3. Beekeepers as records (no account), association link, registry
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.BeekeeperAssociations', N'U') IS NULL
    CREATE TABLE dbo.BeekeeperAssociations
    (
        Id                int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BeekeeperAssociations PRIMARY KEY,
        BeekeeperId       nvarchar(450) NOT NULL CONSTRAINT FK_BeekeeperAssociations_Beekeeper REFERENCES dbo.Beekeepers (BeekeeperId),
        AssociationOrgId  int           NOT NULL CONSTRAINT FK_BeekeeperAssociations_Org REFERENCES dbo.Organizations (OrgId),
        ValidFrom         date          NOT NULL,
        ValidTo           date          NULL,
        CreatedByUserId   nvarchar(450) NULL,
        CreatedAt         datetime2(0)  NOT NULL CONSTRAINT DF_BeekeeperAssociations_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
GO

-- Separate table instead of new Beekeepers columns: the scaffolded beekeeper Edit binds the whole row
IF OBJECT_ID(N'dbo.BeekeeperRegistry', N'U') IS NULL
    CREATE TABLE dbo.BeekeeperRegistry
    (
        BeekeeperId      nvarchar(450) NOT NULL CONSTRAINT PK_BeekeeperRegistry PRIMARY KEY
                                       CONSTRAINT FK_BeekeeperRegistry_Beekeeper REFERENCES dbo.Beekeepers (BeekeeperId),
        RegistryNumber   nvarchar(14)  NULL,
        Status           nvarchar(20)  NOT NULL CONSTRAINT DF_BeekeeperRegistry_Status DEFAULT (N'Unverified'),
        Province         nvarchar(100) NULL,
        District         nvarchar(100) NULL,
        VerifiedAt       datetime2(0)  NULL,
        VerifiedByUserId nvarchar(450) NULL,
        CONSTRAINT CK_BeekeeperRegistry_Status CHECK (Status IN (N'Unverified', N'Pending', N'Verified', N'Rejected'))
    );
GO

IF OBJECT_ID(N'dbo.RegistryVerifications', N'U') IS NULL
    CREATE TABLE dbo.RegistryVerifications
    (
        Id                int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RegistryVerifications PRIMARY KEY,
        BeekeeperId       nvarchar(450) NOT NULL CONSTRAINT FK_RegistryVerifications_Beekeeper REFERENCES dbo.Beekeepers (BeekeeperId),
        EnterpriseNumber  nvarchar(14)  NOT NULL,
        Method            nvarchar(20)  NOT NULL,
        Status            nvarchar(20)  NOT NULL,
        OriginalFileName  nvarchar(255) NULL,
        StoredFileName    nvarchar(260) NULL,
        Sha256            char(64)      NULL,
        RequestedByUserId nvarchar(450) NOT NULL,
        RequestedAt       datetime2(0)  NOT NULL CONSTRAINT DF_RegistryVerifications_RequestedAt DEFAULT (SYSUTCDATETIME()),
        DecidedByUserId   nvarchar(450) NULL,
        DecidedAt         datetime2(0)  NULL,
        Reason            nvarchar(500) NULL,
        CONSTRAINT CK_RegistryVerifications_Method CHECK (Method IN (N'Manual', N'Aks')),
        CONSTRAINT CK_RegistryVerifications_Status CHECK (Status IN (N'Pending', N'Verified', N'Rejected'))
    );
GO

---------------------------------------------------------------------------------------------------
-- 4. Audit log: append-only and hash-chained (D-28)
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.AuditEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditEntries
    (
        AuditId    bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditEntries PRIMARY KEY,
        At         datetime2(3)  NOT NULL,
        UserId     nvarchar(450) NULL,
        UserName   nvarchar(256) NULL,
        OrgId      int           NULL,
        Action     nvarchar(30)  NOT NULL,
        Entity     nvarchar(100) NOT NULL,
        EntityKey  nvarchar(200) NULL,
        OldValues  nvarchar(max) NULL,
        NewValues  nvarchar(max) NULL,
        Flags      nvarchar(200) NULL,
        ClientIp   nvarchar(64)  NULL,
        PrevHash   char(64)      NOT NULL,
        Hash       char(64)      NOT NULL
    );
    -- Each row points to exactly one predecessor: a fork in the chain is impossible
    CREATE UNIQUE INDEX UX_AuditEntries_PrevHash ON dbo.AuditEntries (PrevHash);
    CREATE INDEX IX_AuditEntries_Entity ON dbo.AuditEntries (Entity, EntityKey);
    CREATE INDEX IX_AuditEntries_At ON dbo.AuditEntries (At);
    PRINT N'Table AuditEntries created';
END;
GO
IF OBJECT_ID(N'dbo.TR_AuditEntries_AppendOnly', N'TR') IS NULL
    EXEC (N'CREATE TRIGGER dbo.TR_AuditEntries_AppendOnly ON dbo.AuditEntries
            INSTEAD OF UPDATE, DELETE
            AS
            BEGIN
                THROW 51000, N''AuditEntries is append-only: updates and deletes are not allowed.'', 1;
            END;');
GO

---------------------------------------------------------------------------------------------------
-- 5. CCP work: recorded by, approved by (D-27)
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.CriticalControlPointMonitoringLogs', N'RecordedByUserId') IS NULL
    ALTER TABLE dbo.CriticalControlPointMonitoringLogs ADD
        RecordedByUserId nvarchar(450) NULL,
        RecordedByOrgId  int           NULL CONSTRAINT FK_CCPLogs_RecordedByOrg REFERENCES dbo.Organizations (OrgId);
GO
IF COL_LENGTH(N'dbo.RawHoneyCCPAssignments', N'ApprovedByUserId') IS NULL
    ALTER TABLE dbo.RawHoneyCCPAssignments ADD
        ApprovedByUserId nvarchar(450) NULL,
        ApprovedByOrgId  int           NULL CONSTRAINT FK_RawCCPAssignments_ApprovedByOrg REFERENCES dbo.Organizations (OrgId),
        ApprovedAt       datetime      NULL,
        SelfApproved     bit           NOT NULL CONSTRAINT DF_RawCCPAssignments_SelfApproved DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.FinalHoneyCCPAssignments', N'ApprovedByUserId') IS NULL
    ALTER TABLE dbo.FinalHoneyCCPAssignments ADD
        ApprovedByUserId nvarchar(450) NULL,
        ApprovedByOrgId  int           NULL CONSTRAINT FK_FinalCCPAssignments_ApprovedByOrg REFERENCES dbo.Organizations (OrgId),
        ApprovedAt       datetime      NULL,
        SelfApproved     bit           NOT NULL CONSTRAINT DF_FinalCCPAssignments_SelfApproved DEFAULT (0);
GO

---------------------------------------------------------------------------------------------------
-- 6. Data migration (insert only)
---------------------------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Organizations WHERE IsPlatform = 1)
BEGIN
    INSERT INTO dbo.Organizations (DisplayName, Status, IsPlatform, VerifiedAt)
    VALUES (N'Platform administration', N'Verified', 1, SYSUTCDATETIME());
    PRINT N'Platform organization created';
END;
DECLARE @Platform int = (SELECT OrgId FROM dbo.Organizations WHERE IsPlatform = 1);

INSERT INTO dbo.OrgCapabilities (OrgId, Capability)
SELECT @Platform, N'PlatformAdmin'
WHERE NOT EXISTS (SELECT 1 FROM dbo.OrgCapabilities WHERE OrgId = @Platform AND Capability = N'PlatformAdmin');

INSERT INTO dbo.Memberships (UserId, OrgId, OrgRole, Status)
SELECT ur.UserId, @Platform, N'OrgAdmin', N'Active'
FROM dbo.AspNetUserRoles ur
JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId AND r.NormalizedName = N'ADMINISTRATOR'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Memberships m WHERE m.UserId = ur.UserId AND m.OrgId = @Platform AND m.OrgRole = N'OrgAdmin');
PRINT CONCAT(N'Platform memberships for administrators added: ', @@ROWCOUNT);

-- One organization per wholesaler (D-24). The name is copied from the existing row, no literal names.
INSERT INTO dbo.Organizations (DisplayName, LegalName, TaxNo, Province, Status, LegacyWholesalerId)
SELECT LEFT(w.WholesalerTitle, 200), w.WholesalerTitle, w.WholesalerTaxNo, w.WholesalerCity, N'Pending', w.WholesalerId
FROM dbo.Wholesalers w
WHERE NOT EXISTS (SELECT 1 FROM dbo.Organizations o WHERE o.LegacyWholesalerId = w.WholesalerId);
PRINT CONCAT(N'Organizations created from wholesalers: ', @@ROWCOUNT);

INSERT INTO dbo.OrgCapabilities (OrgId, Capability)
SELECT o.OrgId, c.Code
FROM dbo.Organizations o
CROSS JOIN (VALUES (N'Processing'), (N'Packing'), (N'Wholesale')) c (Code)
WHERE o.LegacyWholesalerId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.OrgCapabilities x WHERE x.OrgId = o.OrgId)
  AND NOT EXISTS (SELECT 1 FROM dbo.OrgVerificationEvents e WHERE e.OrgId = o.OrgId);
PRINT CONCAT(N'Capabilities given to wholesaler organizations: ', @@ROWCOUNT);
GO
