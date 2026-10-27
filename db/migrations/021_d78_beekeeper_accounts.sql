-- 021_d78_beekeeper_accounts.sql
-- D-78 P1 (docs/design/D78-SPEC.md 3.7, 4.1, 4.3, D-80): accounts, attested registrations and issued credentials.
--   * BeekeeperAccounts: the explicit link beekeeper <-> sign-in account (one each). Not backfilled: the legacy implicit link
--     (Beekeepers.BeekeeperId = AspNetUsers.Id, locked logins of D-09/D-23) becomes a row only when a registration officer
--     re-verifies the beekeeper.
--   * BeekeeperRegistrations: append-only attestations (new registration, re-verification, identity change) with the registry
--     check, the method (SMS code or signed photo), the evidence and its review by a second person. Only the review columns
--     change after insert.
--   * UserCredentialIssues: sign-in details issued to an account (by SMS, or set by an administrator); the user must change the
--     password before anything else while the latest issue has no PasswordChangedAt; the temporary password expires after 72 h.
--   * UserSettings: per-account settings kept outside the Identity tables (verified phone date, language, animations switch).
--   * Beekeepers.PreferredLanguage: language of the SMS texts (null = Turkish).
-- Identity tables are not changed. Additive and idempotent; a second run creates nothing. SQL Server 2019 (level 150). sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Notices', N'U') IS NULL OR OBJECT_ID(N'dbo.OneTimeCodes', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 019 and 020 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

IF OBJECT_ID(N'dbo.BeekeeperAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BeekeeperAccounts
    (
        BeekeeperId    nvarchar(450) NOT NULL CONSTRAINT PK_BeekeeperAccounts PRIMARY KEY
                       CONSTRAINT FK_BeekeeperAccounts_Beekeepers REFERENCES dbo.Beekeepers (BeekeeperId),
        UserId         nvarchar(450) NOT NULL,
        LinkedAt       datetime2     NOT NULL CONSTRAINT DF_BeekeeperAccounts_LinkedAt DEFAULT (SYSUTCDATETIME()),
        LinkedByUserId nvarchar(450) NULL,
        Status         varchar(20)   NOT NULL CONSTRAINT DF_BeekeeperAccounts_Status DEFAULT ('Active'),
        CONSTRAINT CK_BeekeeperAccounts_Status CHECK (Status IN ('Active', 'Suspended'))
    );
    CREATE UNIQUE INDEX UX_BeekeeperAccounts_User ON dbo.BeekeeperAccounts (UserId);
END;
GO

IF OBJECT_ID(N'dbo.BeekeeperRegistrations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BeekeeperRegistrations
    (
        RegistrationId          int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BeekeeperRegistrations PRIMARY KEY,
        BeekeeperId             nvarchar(450) NOT NULL CONSTRAINT FK_BeekeeperRegistrations_Beekeepers REFERENCES dbo.Beekeepers (BeekeeperId),
        RegisteringOrgId        int           NOT NULL CONSTRAINT FK_BeekeeperRegistrations_Organizations REFERENCES dbo.Organizations (OrgId),
        OfficerUserId           nvarchar(450) NOT NULL,
        At                      datetime2     NOT NULL CONSTRAINT DF_BeekeeperRegistrations_At DEFAULT (SYSUTCDATETIME()),
        Kind                    varchar(20)   NOT NULL,
        RegistryNumber          nvarchar(20)  NULL,
        RegistryFormatOk        bit           NOT NULL,
        RegistryProvinceMatches bit           NULL,
        ProvinceMismatchReason  nvarchar(300) NULL,
        PhoneE164Hash           char(64)      NULL,
        Method                  varchar(20)   NOT NULL,
        CodeId                  bigint        NULL CONSTRAINT FK_BeekeeperRegistrations_OneTimeCodes REFERENCES dbo.OneTimeCodes (CodeId),
        FallbackReason          nvarchar(300) NULL,
        EvidenceStoredFileName  nvarchar(100) NULL,
        EvidenceOriginalFileName nvarchar(260) NULL,
        EvidenceSha256          char(64)      NULL,
        OfficerAttestation      bit           NOT NULL,
        AttestationNoticeId     int           NULL CONSTRAINT FK_BeekeeperRegistrations_Notices REFERENCES dbo.Notices (NoticeId),
        EvidenceStatus          varchar(20)   NOT NULL,
        ReviewedByUserId        nvarchar(450) NULL,
        ReviewedAt              datetime2     NULL,
        ReviewReason            nvarchar(500) NULL,
        CONSTRAINT CK_BeekeeperRegistrations_Kind CHECK (Kind IN ('New', 'Reverification', 'IdentityChange')),
        CONSTRAINT CK_BeekeeperRegistrations_Method CHECK (Method IN ('SmsCode', 'SignedPhoto')),
        CONSTRAINT CK_BeekeeperRegistrations_Evidence CHECK (EvidenceStatus IN ('Accepted', 'PendingReview', 'Rejected')),
        CONSTRAINT CK_BeekeeperRegistrations_Attested CHECK (OfficerAttestation = 1)
    );
    CREATE INDEX IX_BeekeeperRegistrations_Beekeeper ON dbo.BeekeeperRegistrations (BeekeeperId, At);
    CREATE INDEX IX_BeekeeperRegistrations_Org ON dbo.BeekeeperRegistrations (RegisteringOrgId, EvidenceStatus);
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_BeekeeperRegistrations_AppendOnly ON dbo.BeekeeperRegistrations AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'BeekeeperRegistrations is append-only: rows cannot be deleted.', 1;
    IF UPDATE(BeekeeperId) OR UPDATE(RegisteringOrgId) OR UPDATE(OfficerUserId) OR UPDATE(At) OR UPDATE(Kind) OR UPDATE(RegistryNumber)
       OR UPDATE(RegistryFormatOk) OR UPDATE(RegistryProvinceMatches) OR UPDATE(ProvinceMismatchReason) OR UPDATE(PhoneE164Hash)
       OR UPDATE(Method) OR UPDATE(CodeId) OR UPDATE(FallbackReason) OR UPDATE(EvidenceStoredFileName) OR UPDATE(EvidenceOriginalFileName)
       OR UPDATE(EvidenceSha256) OR UPDATE(OfficerAttestation) OR UPDATE(AttestationNoticeId)
        THROW 50002, N'BeekeeperRegistrations: only the review can change.', 1;
END;
GO

IF OBJECT_ID(N'dbo.UserCredentialIssues', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserCredentialIssues
    (
        IssueId           int IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserCredentialIssues PRIMARY KEY,
        UserId            nvarchar(450) NOT NULL,
        IssuedByUserId    nvarchar(450) NULL,
        IssuedAt          datetime2     NOT NULL CONSTRAINT DF_UserCredentialIssues_IssuedAt DEFAULT (SYSUTCDATETIME()),
        Channel           varchar(20)   NOT NULL,
        SmsId             bigint        NULL CONSTRAINT FK_UserCredentialIssues_SmsMessages REFERENCES dbo.SmsMessages (SmsId),
        ExpiresAt         datetime2     NOT NULL,
        PasswordChangedAt datetime2     NULL,
        CONSTRAINT CK_UserCredentialIssues_Channel CHECK (Channel IN ('Sms', 'Manual'))
    );
    CREATE INDEX IX_UserCredentialIssues_User ON dbo.UserCredentialIssues (UserId, IssueId);
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_UserCredentialIssues_AppendOnly ON dbo.UserCredentialIssues AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'UserCredentialIssues is append-only: rows cannot be deleted.', 1;
    IF UPDATE(UserId) OR UPDATE(IssuedByUserId) OR UPDATE(IssuedAt) OR UPDATE(Channel) OR UPDATE(SmsId) OR UPDATE(ExpiresAt)
        THROW 50002, N'UserCredentialIssues: only PasswordChangedAt can be set.', 1;
END;
GO

IF OBJECT_ID(N'dbo.UserSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserSettings
    (
        UserId          nvarchar(450) NOT NULL CONSTRAINT PK_UserSettings PRIMARY KEY,
        Language        varchar(5)    NULL,
        PhoneVerifiedAt datetime2     NULL,
        PhoneCodeId     bigint        NULL CONSTRAINT FK_UserSettings_OneTimeCodes REFERENCES dbo.OneTimeCodes (CodeId),
        ShowAnimations  bit           NOT NULL CONSTRAINT DF_UserSettings_ShowAnimations DEFAULT (1)
    );
END;
GO

IF COL_LENGTH(N'dbo.Beekeepers', N'PreferredLanguage') IS NULL
    ALTER TABLE dbo.Beekeepers ADD PreferredLanguage varchar(5) NULL;
GO

PRINT N'021: BeekeeperAccounts, BeekeeperRegistrations, UserCredentialIssues, UserSettings and Beekeepers.PreferredLanguage are in place.';
GO

SET NOEXEC OFF;
GO
