-- 025_d78_compliance.sql
-- D-78 P3 (docs/design/D78-SPEC.md 2.5, 2.6, 4.8, D-82): the Compliance Officer's records.
--   * DataSubjectRequests: requests of data subjects (access, correction, deletion, objection, consent withdrawal received outside
--     the portal) with the 30-day KVKK deadline; closed with an outcome.
--   * ComplianceReviews: the Compliance Officer's append-only reviews of sampling quality (photo-confirmed intakes, rejected
--     samples, seals found broken, reception mismatches) with finding and action.
--   * OrgDocuments.AgreementKind: the new document kinds DataProcessingAgreement and SupplyAgreement, stored with Kind 'Other'
--     (the existing CHECK on Kind stays as it is: no constraint is dropped).
--   * RolePermissions: Price Read (OrgAdmin, ComplianceOfficer) and OrgAudit Read (ComplianceOfficer).
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.LotPrices', N'U') IS NULL OR OBJECT_ID(N'dbo.OrgDocuments', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 024 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

IF OBJECT_ID(N'dbo.DataSubjectRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DataSubjectRequests
    (
        RequestId         int            IDENTITY(1,1) NOT NULL CONSTRAINT PK_DataSubjectRequests PRIMARY KEY,
        OrgId             int            NOT NULL CONSTRAINT FK_DataSubjectRequests_Orgs REFERENCES dbo.Organizations (OrgId),
        BeekeeperId       nvarchar(450)  NULL CONSTRAINT FK_DataSubjectRequests_Beekeepers REFERENCES dbo.Beekeepers (BeekeeperId),
        SubjectName       nvarchar(200)  NOT NULL,
        Kind              varchar(30)    NOT NULL,
        Channel           nvarchar(50)   NOT NULL,
        ReceivedOn        date           NOT NULL,
        DueOn             date           NOT NULL,
        Details           nvarchar(1000) NOT NULL,
        Status            varchar(20)    NOT NULL CONSTRAINT DF_DataSubjectRequests_Status DEFAULT ('Open'),
        Outcome           nvarchar(1000) NULL,
        RecordedByUserId  nvarchar(450)  NOT NULL,
        RecordedAt        datetime2      NOT NULL,
        ClosedByUserId    nvarchar(450)  NULL,
        ClosedAt          datetime2      NULL,
        CONSTRAINT CK_DataSubjectRequests_Kind CHECK (Kind IN ('Access', 'Correction', 'Deletion', 'Objection', 'ConsentWithdrawal')),
        CONSTRAINT CK_DataSubjectRequests_Status CHECK (Status IN ('Open', 'Answered', 'Refused')),
        CONSTRAINT CK_DataSubjectRequests_Due CHECK (DueOn >= ReceivedOn)
    );
    CREATE INDEX IX_DataSubjectRequests_Org ON dbo.DataSubjectRequests (OrgId, Status, DueOn);
    PRINT N'025: DataSubjectRequests created.';
END;
GO

IF OBJECT_ID(N'dbo.ComplianceReviews', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ComplianceReviews
    (
        ReviewId          int            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ComplianceReviews PRIMARY KEY,
        OrgId             int            NOT NULL CONSTRAINT FK_ComplianceReviews_Orgs REFERENCES dbo.Organizations (OrgId),
        SubjectType       varchar(20)    NOT NULL,
        SubjectId         int            NOT NULL,
        Outcome           varchar(20)    NOT NULL,
        Finding           nvarchar(1000) NOT NULL,
        Action            nvarchar(1000) NULL,
        ReviewedByUserId  nvarchar(450)  NOT NULL,
        ReviewedAt        datetime2      NOT NULL,
        CONSTRAINT CK_ComplianceReviews_Subject CHECK (SubjectType IN ('Intake', 'Sample', 'SealEvent')),
        CONSTRAINT CK_ComplianceReviews_Outcome CHECK (Outcome IN ('InOrder', 'Finding'))
    );
    CREATE INDEX IX_ComplianceReviews_Subject ON dbo.ComplianceReviews (OrgId, SubjectType, SubjectId);
    PRINT N'025: ComplianceReviews created.';
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_ComplianceReviews_AppendOnly ON dbo.ComplianceReviews AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'ComplianceReviews is append-only: write a new review instead.', 1;
END;
GO

IF COL_LENGTH(N'dbo.OrgDocuments', N'AgreementKind') IS NULL
    ALTER TABLE dbo.OrgDocuments ADD AgreementKind varchar(30) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_OrgDocuments_AgreementKind')
    ALTER TABLE dbo.OrgDocuments ADD CONSTRAINT CK_OrgDocuments_AgreementKind
        CHECK (AgreementKind IS NULL OR (Kind = N'Other' AND AgreementKind IN ('DataProcessingAgreement', 'SupplyAgreement')));
GO

INSERT dbo.RolePermissions (Area, AreaKey, Action, OrgRole)
SELECT v.Area, '', v.Action, v.OrgRole
FROM (VALUES ('Price', 'Read', N'OrgAdmin'), ('Price', 'Read', N'ComplianceOfficer'), ('OrgAudit', 'Read', N'ComplianceOfficer')) AS v (Area, Action, OrgRole)
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissions p WHERE p.Area = v.Area AND p.AreaKey = '' AND p.Action = v.Action AND p.OrgRole = v.OrgRole);
PRINT N'025: ' + CONVERT(nvarchar(10), @@ROWCOUNT) + N' role permission(s) added.';
GO

SET NOEXEC OFF;
GO
