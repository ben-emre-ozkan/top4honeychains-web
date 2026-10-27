-- 019_d78_sms_otp.sql
-- D-78 P1 (docs/design/D78-SPEC.md 3.4 and 5, D-80): the SMS log (SmsMessages) and the one-time codes (OneTimeCodes).
--   * SmsMessages: one row per message sent through the provider abstraction. The full phone number is never stored here
--     (masked number + HMAC of the number); with a real provider, codes and passwords in the stored text are masked.
--     Append-only: only the delivery status columns (Status, StatusAt, Error, ProviderMessageId) may change.
--   * OneTimeCodes: 6-digit codes, stored only as an HMAC with a per-code salt, bound to a purpose, a subject and the hash
--     of the content the person confirms. Only the usage columns (Attempts, LastAttemptAt, UsedAt, UsedByUserId, VoidedAt,
--     VoidReason) may change. LastAttemptAt is an addition to the spec table (rate limit of failed entries per officer).
-- New tables only; additive and idempotent (a second run creates nothing). SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.SmsMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SmsMessages
    (
        SmsId             bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_SmsMessages PRIMARY KEY,
        Purpose           varchar(30)    NOT NULL,
        SubjectType       varchar(30)    NULL,
        SubjectKey        nvarchar(64)   NULL,
        ToPhoneMasked     varchar(20)    NOT NULL,
        ToPhoneHash       char(64)       NOT NULL,
        Language          varchar(5)     NOT NULL,
        BodyStored        nvarchar(800)  NOT NULL,
        Segments          tinyint        NOT NULL,
        Provider          varchar(30)    NOT NULL,
        ProviderMessageId varchar(64)    NULL,
        Status            varchar(20)    NOT NULL,
        StatusAt          datetime2      NULL,
        Error             nvarchar(400)  NULL,
        SentByUserId      nvarchar(450)  NULL,
        OrgId             int            NULL,
        CreatedAt         datetime2      NOT NULL CONSTRAINT DF_SmsMessages_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
    CREATE INDEX IX_SmsMessages_Phone ON dbo.SmsMessages (ToPhoneHash, CreatedAt);
    CREATE INDEX IX_SmsMessages_Subject ON dbo.SmsMessages (Purpose, SubjectType, SubjectKey, CreatedAt);
    CREATE INDEX IX_SmsMessages_Sender ON dbo.SmsMessages (SentByUserId, CreatedAt);
    CREATE INDEX IX_SmsMessages_CreatedAt ON dbo.SmsMessages (CreatedAt);
END;
GO

IF OBJECT_ID(N'dbo.OneTimeCodes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OneTimeCodes
    (
        CodeId        bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_OneTimeCodes PRIMARY KEY,
        SmsId         bigint         NOT NULL CONSTRAINT FK_OneTimeCodes_SmsMessages REFERENCES dbo.SmsMessages (SmsId),
        Purpose       varchar(30)    NOT NULL,
        SubjectType   varchar(30)    NOT NULL,
        SubjectKey    nvarchar(64)   NOT NULL,
        ContentSha256 char(64)       NOT NULL,
        CodeHash      char(64)       NOT NULL,
        Salt          binary(16)     NOT NULL,
        ExpiresAt     datetime2      NOT NULL,
        Attempts      tinyint        NOT NULL CONSTRAINT DF_OneTimeCodes_Attempts DEFAULT (0),
        LastAttemptAt datetime2      NULL,
        UsedAt        datetime2      NULL,
        UsedByUserId  nvarchar(450)  NULL,
        VoidedAt      datetime2      NULL,
        VoidReason    varchar(30)    NULL,
        CreatedAt     datetime2      NOT NULL CONSTRAINT DF_OneTimeCodes_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
    CREATE INDEX IX_OneTimeCodes_Subject ON dbo.OneTimeCodes (Purpose, SubjectType, SubjectKey, CreatedAt);
    CREATE INDEX IX_OneTimeCodes_SmsId ON dbo.OneTimeCodes (SmsId);
END;
GO

-- append-only: rows are never deleted; only the status / usage columns change
CREATE OR ALTER TRIGGER dbo.TR_SmsMessages_AppendOnly ON dbo.SmsMessages AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'SmsMessages is append-only: rows cannot be deleted.', 1;
    IF UPDATE(Purpose) OR UPDATE(SubjectType) OR UPDATE(SubjectKey) OR UPDATE(ToPhoneMasked) OR UPDATE(ToPhoneHash) OR UPDATE(Language)
       OR UPDATE(BodyStored) OR UPDATE(Segments) OR UPDATE(Provider) OR UPDATE(SentByUserId) OR UPDATE(OrgId) OR UPDATE(CreatedAt)
        THROW 50002, N'SmsMessages: only the delivery status can change.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_OneTimeCodes_AppendOnly ON dbo.OneTimeCodes AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'OneTimeCodes is append-only: rows cannot be deleted.', 1;
    IF UPDATE(SmsId) OR UPDATE(Purpose) OR UPDATE(SubjectType) OR UPDATE(SubjectKey) OR UPDATE(ContentSha256) OR UPDATE(CodeHash)
       OR UPDATE(Salt) OR UPDATE(ExpiresAt) OR UPDATE(CreatedAt)
        THROW 50002, N'OneTimeCodes: only the usage columns can change.', 1;
END;
GO

PRINT N'019: SmsMessages and OneTimeCodes are in place.';
GO
