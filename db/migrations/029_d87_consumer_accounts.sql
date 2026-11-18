-- 029_d87_consumer_accounts.sql
-- D-87 (docs/DECISIONS.md): consumer accounts on the web platform.
--   * ConsumerAccounts: a consumer sign-in account (no role, no membership): pseudonym code, language, age confirmation, the
--     accepted consumer notice, created and last sign-in times, status. Deleting an account removes this row and the saved list;
--     the Identity user is anonymised by the application.
--   * ConsumerSavedBatches: the honeys a consumer saved explicitly (opened stories are never recorded).
--   * ConsumerFeedback: feedback to the organization that owns the batch, with its answer; it keeps the pseudonym only, so it
--     stays with the organization after the account is deleted (ConsumerUserId becomes NULL).
--   * Notices: audience 'Consumer' (the old check is disabled and replaced by a wider one, as 025 did for Consents).
--   * RolePermissions: area ConsumerFeedback, Read and Enter (answer), for OrgAdmin, QualityApprover and ComplianceOfficer.
--   * Seeds the DRAFT consumer privacy notice (version consumer-2026-11-draft1) in five languages (O-14). Insert-only.
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (the live database runs at level 140). sqlcmd -I -f 65001.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Notices', N'U') IS NULL OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL OR OBJECT_ID(N'dbo.FinalHoneyProductions', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 013 and 020 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

IF OBJECT_ID(N'dbo.ConsumerAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerAccounts
    (
        UserId          nvarchar(450) NOT NULL CONSTRAINT PK_ConsumerAccounts PRIMARY KEY,
        Pseudonym       varchar(12)   NOT NULL,
        Language        varchar(5)    NOT NULL,
        AgeConfirmedAt  datetime2     NOT NULL,
        NoticeId        int           NULL CONSTRAINT FK_ConsumerAccounts_Notices REFERENCES dbo.Notices (NoticeId),
        NoticeVersion   varchar(40)   NULL,
        CreatedAt       datetime2     NOT NULL CONSTRAINT DF_ConsumerAccounts_CreatedAt DEFAULT (SYSUTCDATETIME()),
        LastSignInAt    datetime2     NULL,
        Status          varchar(20)   NOT NULL CONSTRAINT DF_ConsumerAccounts_Status DEFAULT ('Active'),
        CONSTRAINT CK_ConsumerAccounts_Status CHECK (Status IN ('Active', 'Locked'))
    );
    CREATE UNIQUE INDEX UX_ConsumerAccounts_Pseudonym ON dbo.ConsumerAccounts (Pseudonym);
END;
GO

IF OBJECT_ID(N'dbo.ConsumerSavedBatches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerSavedBatches
    (
        ConsumerUserId          nvarchar(450) NOT NULL CONSTRAINT FK_ConsumerSavedBatches_ConsumerAccounts REFERENCES dbo.ConsumerAccounts (UserId),
        FinalHoneyProductionId  int           NOT NULL CONSTRAINT FK_ConsumerSavedBatches_FinalHoneyProductions REFERENCES dbo.FinalHoneyProductions (FinalHoneyProductionId),
        SavedAt                 datetime2     NOT NULL CONSTRAINT DF_ConsumerSavedBatches_SavedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_ConsumerSavedBatches PRIMARY KEY (ConsumerUserId, FinalHoneyProductionId)
    );
END;
GO

IF OBJECT_ID(N'dbo.ConsumerFeedback', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerFeedback
    (
        FeedbackId              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerFeedback PRIMARY KEY,
        ConsumerUserId          nvarchar(450)  NULL CONSTRAINT FK_ConsumerFeedback_ConsumerAccounts REFERENCES dbo.ConsumerAccounts (UserId),
        Pseudonym               varchar(12)    NOT NULL,
        FinalHoneyProductionId  int            NOT NULL CONSTRAINT FK_ConsumerFeedback_FinalHoneyProductions REFERENCES dbo.FinalHoneyProductions (FinalHoneyProductionId),
        OrgId                   int            NOT NULL CONSTRAINT FK_ConsumerFeedback_Organizations REFERENCES dbo.Organizations (OrgId),
        Category                varchar(30)    NOT NULL,
        Message                 nvarchar(1000) NOT NULL,
        Language                varchar(5)     NULL,
        CreatedAt               datetime2      NOT NULL CONSTRAINT DF_ConsumerFeedback_CreatedAt DEFAULT (SYSUTCDATETIME()),
        Status                  varchar(20)    NOT NULL CONSTRAINT DF_ConsumerFeedback_Status DEFAULT ('New'),
        ReadAt                  datetime2      NULL,
        ReadByUserId            nvarchar(450)  NULL,
        Answer                  nvarchar(1000) NULL,
        AnsweredAt              datetime2      NULL,
        AnsweredByUserId        nvarchar(450)  NULL,
        ClosedAt                datetime2      NULL,
        ClosedByUserId          nvarchar(450)  NULL,
        CONSTRAINT CK_ConsumerFeedback_Category CHECK (Category IN ('TasteSmell', 'TextureCrystallization', 'PackagingLabel', 'SuspectedFake', 'Other')),
        CONSTRAINT CK_ConsumerFeedback_Status CHECK (Status IN ('New', 'Read', 'Answered', 'Closed'))
    );
    CREATE INDEX IX_ConsumerFeedback_Org ON dbo.ConsumerFeedback (OrgId, Status);
    CREATE INDEX IX_ConsumerFeedback_Consumer ON dbo.ConsumerFeedback (ConsumerUserId, CreatedAt);
END;
GO

-- Notices for consumers: widen the audience check (the old one stays, disabled)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Notices_Audience' AND is_disabled = 0)
    ALTER TABLE dbo.Notices NOCHECK CONSTRAINT CK_Notices_Audience;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Notices_Audience2')
    ALTER TABLE dbo.Notices ADD CONSTRAINT CK_Notices_Audience2 CHECK (Audience IN ('Beekeeper', 'Member', 'All', 'Consumer'));
GO

INSERT dbo.RolePermissions (Area, AreaKey, Action, OrgRole)
SELECT v.Area, '', v.Action, v.OrgRole
FROM (VALUES ('ConsumerFeedback', 'Read', N'OrgAdmin'), ('ConsumerFeedback', 'Read', N'QualityApprover'), ('ConsumerFeedback', 'Read', N'ComplianceOfficer'),
             ('ConsumerFeedback', 'Enter', N'OrgAdmin'), ('ConsumerFeedback', 'Enter', N'QualityApprover'), ('ConsumerFeedback', 'Enter', N'ComplianceOfficer')) AS v (Area, Action, OrgRole)
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissions p WHERE p.Area = v.Area AND p.AreaKey = '' AND p.Action = v.Action AND p.OrgRole = v.OrgRole);
PRINT N'029: ' + CONVERT(nvarchar(10), @@ROWCOUNT) + N' role permission(s) added.';
GO

-- @@NOTICES@@

PRINT N'029: ConsumerAccounts, ConsumerSavedBatches, ConsumerFeedback, the consumer notice audience and permissions are in place.';
