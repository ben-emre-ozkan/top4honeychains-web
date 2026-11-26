-- 037_d112_test_set.sql
-- D-112: a test set for the live site (create-test-set / remove-test-set, docs/KURULUM-TR.md B9), a deliberate exception to D-88.
--   * Organizations.IsTestData and RawHoneyProductionPeriods.IsTestData: the flag of test data (0 = real, the default for every
--     existing and new row). The Honey Story and the public pages of a test batch show a TEST banner.
--   * TestSetAccounts: the accounts of the test set (AspNetUsers belongs to ASP.NET Identity, so the flag of an account is its row
--     here), with the set number and the kind of account.
--   * The append-only and immutable triggers let remove-test-set DELETE rows (never change them) while the session context
--     'top4.test_set_removal' is set, the way 028 lets the retention job through. remove-test-set decides which rows are test data
--     and refuses when a real record refers to one of them. AuditEntries stays append-only: the audit entries of the test set stay.
-- Additive and idempotent; a second run changes nothing (the triggers are rewritten with the same text). Safe on a new installation
-- (db/install) and on an updated database. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Organizations', N'U') IS NULL OR OBJECT_ID(N'dbo.ContainerEmptyings', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 004 to 036 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

-- ---------------------------------------------------------------- the flag of test data

IF COL_LENGTH(N'dbo.Organizations', N'IsTestData') IS NULL
    ALTER TABLE dbo.Organizations ADD IsTestData bit NOT NULL CONSTRAINT DF_Organizations_IsTestData DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.RawHoneyProductionPeriods', N'IsTestData') IS NULL
    ALTER TABLE dbo.RawHoneyProductionPeriods ADD IsTestData bit NOT NULL CONSTRAINT DF_RawHoneyProductionPeriods_IsTestData DEFAULT (0);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Organizations_IsTestData' AND object_id = OBJECT_ID(N'dbo.Organizations'))
    CREATE INDEX IX_Organizations_IsTestData ON dbo.Organizations (IsTestData) WHERE IsTestData = 1;
GO

IF OBJECT_ID(N'dbo.TestSetAccounts', N'U') IS NULL
CREATE TABLE dbo.TestSetAccounts
(
    UserId    nvarchar(450) NOT NULL CONSTRAINT PK_TestSetAccounts PRIMARY KEY,
    SetNo     int           NULL,
    Kind      varchar(20)   NOT NULL CONSTRAINT CK_TestSetAccounts_Kind CHECK (Kind IN ('Operator', 'Member', 'Beekeeper', 'Consumer')),
    CreatedAt datetime2     NOT NULL
);
GO

-- ---------------------------------------------------------------- the triggers: remove-test-set may delete
CREATE OR ALTER TRIGGER dbo.TR_BeekeeperRegistrations_AppendOnly ON dbo.BeekeeperRegistrations AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
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

CREATE OR ALTER TRIGGER dbo.TR_ComplianceReviews_AppendOnly ON dbo.ComplianceReviews AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'ComplianceReviews is append-only: write a new review instead.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_ContainerEmptyings_AppendOnly ON dbo.ContainerEmptyings AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'ContainerEmptyings is append-only: add a new record instead.', 1;
END;
GO

-- ---------------------------------------------------------------- the 023 locks, with the correction passage

CREATE OR ALTER TRIGGER dbo.TR_Containers_Immutable ON dbo.Containers AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted) OR SESSION_CONTEXT(N'top4.delivery_correction') IS NULL
        THROW 50001, N'Containers of a delivery cannot be changed or deleted.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_ContainerSealEvents_AppendOnly ON dbo.ContainerSealEvents AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    -- a confirmed correction may fix a seal number; it never deletes an event
    IF NOT EXISTS (SELECT 1 FROM inserted) OR SESSION_CONTEXT(N'top4.delivery_correction') IS NULL
       OR UPDATE(ContainerId) OR UPDATE(Event) OR UPDATE(UserId) OR UPDATE(At)
        THROW 50001, N'ContainerSealEvents is append-only: add a new event instead.', 1;
END;
GO

-- a decided correction is a record; a pending one changes only by its decision
CREATE OR ALTER TRIGGER dbo.TR_DeliveryCorrections_Guard ON dbo.DeliveryCorrections AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'Delivery corrections cannot be deleted.', 1;
    -- a decided correction only gets the id of the information SMS sent to the beekeeper, once
    IF EXISTS (SELECT 1 FROM deleted WHERE Status <> 'Pending')
       AND (UPDATE(Status) OR UPDATE(DecidedByUserId) OR UPDATE(DecidedAt) OR UPDATE(DecisionNote) OR EXISTS (SELECT 1 FROM deleted WHERE InfoSmsId IS NOT NULL))
        THROW 50001, N'A decided delivery correction cannot be changed.', 1;
    IF UPDATE(IntakeId) OR UPDATE(RawHoneyProductionId) OR UPDATE(OldContent) OR UPDATE(OldContentSha256) OR UPDATE(NewContent)
       OR UPDATE(NewContentSha256) OR UPDATE(Reason) OR UPDATE(RequestedByUserId) OR UPDATE(RequestedAt)
        THROW 50001, N'Only the decision of a delivery correction can be recorded.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_LotIntakes_DeliveryLocked ON dbo.LotIntakes AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM deleted WHERE ContainersRecorded = 1)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'A delivery recorded with containers cannot be deleted.', 1;
    -- a confirmed correction changes the date, the quantity and the confirmed content hash only
    IF SESSION_CONTEXT(N'top4.delivery_correction') IS NOT NULL
    BEGIN
        IF UPDATE(RawHoneyProductionId) OR UPDATE(ReceivingOrgId) OR UPDATE(ReceivedByUserId) OR UPDATE(ReceivedAt)
           OR UPDATE(Province) OR UPDATE(District) OR UPDATE(FormStoredFileName) OR UPDATE(FormOriginalFileName) OR UPDATE(FormSha256)
           OR UPDATE(ConfirmationMethod) OR UPDATE(ConfirmedAt) OR UPDATE(CodeId) OR UPDATE(FallbackReason) OR UPDATE(ContainersRecorded)
            THROW 50002, N'A delivery correction changes only the date, the quantity and the content hash.', 1;
        RETURN;
    END;
    IF UPDATE(RawHoneyProductionId) OR UPDATE(ReceivingOrgId) OR UPDATE(ReceivedByUserId) OR UPDATE(ReceivedAt) OR UPDATE(DeliveryDate)
       OR UPDATE(Quantity) OR UPDATE(Province) OR UPDATE(District) OR UPDATE(FormStoredFileName) OR UPDATE(FormOriginalFileName)
       OR UPDATE(FormSha256) OR UPDATE(ConfirmationMethod) OR UPDATE(ContentSha256) OR UPDATE(ConfirmedAt) OR UPDATE(CodeId)
       OR UPDATE(FallbackReason) OR UPDATE(ContainersRecorded)
        THROW 50002, N'A delivery recorded with containers cannot be changed (D-81).', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_LotPrices_AppendOnly ON dbo.LotPrices AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    -- 028: the retention job deletes prices entered more than 5 years ago (commercial records)
    IF NOT EXISTS (SELECT 1 FROM inserted) AND SESSION_CONTEXT(N'top4.retention') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM deleted WHERE EnteredAt >= DATEADD(year, -5, SYSUTCDATETIME()))
        RETURN;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'LotPrices is append-only: a new price is a new row.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_NoticeAcceptances_AppendOnly ON dbo.NoticeAcceptances AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'NoticeAcceptances is append-only.', 1;
END;
GO

-- a published text never changes; only RetiredAt is set once
CREATE OR ALTER TRIGGER dbo.TR_Notices_Immutable ON dbo.Notices AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'Notices cannot be deleted; publish a new version instead.', 1;
    IF UPDATE(Kind) OR UPDATE(Audience) OR UPDATE(OrgId) OR UPDATE(Purpose) OR UPDATE(Version) OR UPDATE(Language)
       OR UPDATE(Body) OR UPDATE(BodySha256) OR UPDATE(IsDraft) OR UPDATE(PublishedAt) OR UPDATE(PublishedByUserId)
        THROW 50002, N'A published notice cannot be changed; publish a new version instead.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_OneTimeCodes_AppendOnly ON dbo.OneTimeCodes AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
    BEGIN
        -- 028: the retention job deletes codes older than 30 days (only hashes were stored)
        IF SESSION_CONTEXT(N'top4.retention') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM deleted WHERE CreatedAt >= DATEADD(day, -30, SYSUTCDATETIME()))
            RETURN;
        THROW 50001, N'OneTimeCodes is append-only: rows cannot be deleted.', 1;
    END;
    IF UPDATE(SmsId) OR UPDATE(Purpose) OR UPDATE(SubjectType) OR UPDATE(SubjectKey) OR UPDATE(ContentSha256) OR UPDATE(CodeHash)
       OR UPDATE(Salt) OR UPDATE(ExpiresAt) OR UPDATE(CreatedAt)
        THROW 50002, N'OneTimeCodes: only the usage columns can change.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_SampleSources_InsertOnly ON dbo.SampleSources AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'The sources of a composite sample cannot be changed.', 1;
END;
GO

-- a used code never goes back to the pool
CREATE OR ALTER TRIGGER dbo.TR_SealCodes_OneWay ON dbo.SealCodes AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF EXISTS (SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.SealCodeId = d.SealCodeId
               WHERE i.SealCodeId IS NULL AND d.Status <> 'Issued')
        THROW 50001, N'A used seal code cannot be deleted.', 1;
    IF EXISTS (SELECT 1 FROM deleted d JOIN inserted i ON i.SealCodeId = d.SealCodeId
               WHERE i.Code <> d.Code OR i.OrgId <> d.OrgId
                  OR (d.Status = 'Used' AND i.Status = 'Issued') OR (d.Status = 'Broken' AND i.Status <> 'Broken'))
        THROW 50001, N'A seal code cannot be renamed or returned to the pool.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_SmsMessages_AppendOnly ON dbo.SmsMessages AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'SmsMessages is append-only: rows cannot be deleted.', 1;
    -- 028: the retention job clears the number and the body of messages older than 1 year; purpose, segments, status and date stay
    IF SESSION_CONTEXT(N'top4.retention') IS NOT NULL
       AND NOT (UPDATE(Purpose) OR UPDATE(SubjectType) OR UPDATE(SubjectKey) OR UPDATE(Language) OR UPDATE(Segments) OR UPDATE(Provider)
                OR UPDATE(SentByUserId) OR UPDATE(OrgId) OR UPDATE(CreatedAt))
       AND NOT EXISTS (SELECT 1 FROM inserted
                       WHERE CreatedAt >= DATEADD(year, -1, SYSUTCDATETIME())
                          OR BodyStored <> N'' OR ToPhoneMasked <> '' OR ToPhoneHash <> REPLICATE('0', 64))
        RETURN;
    IF UPDATE(Purpose) OR UPDATE(SubjectType) OR UPDATE(SubjectKey) OR UPDATE(ToPhoneMasked) OR UPDATE(ToPhoneHash) OR UPDATE(Language)
       OR UPDATE(BodyStored) OR UPDATE(Segments) OR UPDATE(Provider) OR UPDATE(SentByUserId) OR UPDATE(OrgId) OR UPDATE(CreatedAt)
        THROW 50002, N'SmsMessages: only the delivery status can change.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_UserCredentialIssues_AppendOnly ON dbo.UserCredentialIssues AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 037 (D-112): remove-test-set deletes the rows of the test set (a delete, never a change)
    IF SESSION_CONTEXT(N'top4.test_set_removal') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM inserted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'UserCredentialIssues is append-only: rows cannot be deleted.', 1;
    IF UPDATE(UserId) OR UPDATE(IssuedByUserId) OR UPDATE(IssuedAt) OR UPDATE(Channel) OR UPDATE(SmsId) OR UPDATE(ExpiresAt)
        THROW 50002, N'UserCredentialIssues: only PasswordChangedAt can be set.', 1;
END;
GO

PRINT N'037: the test-set flag and the removal passage are in place.';
GO
