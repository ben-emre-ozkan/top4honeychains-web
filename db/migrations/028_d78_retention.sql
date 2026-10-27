-- 028_d78_retention.sql
-- D-78 P5 (D78-SPEC 6.3, D-84): the retention job may remove what 6.3 says, and nothing else. The append-only triggers of
-- OneTimeCodes, SmsMessages and LotPrices let a change through only when the session carries 'top4.retention'
-- (sp_set_session_context, set by the job inside its transaction; nothing in the UI sets it) AND every row is past its period:
--   OneTimeCodes  delete, created more than 30 days ago;
--   SmsMessages   number hash, masked number and body cleared (counts kept), sent more than 1 year ago;
--   LotPrices     delete, entered more than 5 years ago.
-- The checks repeat the job's own rule (defence in depth). All other behaviour of the triggers is unchanged. The job is off by
-- default (Retention:ApplyEnabled = false); its first run is the owner's decision. No data is changed by this script.
-- Idempotent (CREATE OR ALTER). SQL Server 2019 (level 150). Run with sqlcmd -I (or in SSMS).

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.OneTimeCodes', N'U') IS NULL OR OBJECT_ID(N'dbo.SmsMessages', N'U') IS NULL OR OBJECT_ID(N'dbo.LotPrices', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 019_d78_sms_otp.sql and 024_d78_lot_prices.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_OneTimeCodes_AppendOnly ON dbo.OneTimeCodes AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
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

CREATE OR ALTER TRIGGER dbo.TR_SmsMessages_AppendOnly ON dbo.SmsMessages AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
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

CREATE OR ALTER TRIGGER dbo.TR_LotPrices_AppendOnly ON dbo.LotPrices AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 028: the retention job deletes prices entered more than 5 years ago (commercial records)
    IF NOT EXISTS (SELECT 1 FROM inserted) AND SESSION_CONTEXT(N'top4.retention') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM deleted WHERE EnteredAt >= DATEADD(year, -5, SYSUTCDATETIME()))
        RETURN;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'LotPrices is append-only: a new price is a new row.', 1;
END;
GO

PRINT N'028: the retention passage of the OneTimeCodes, SmsMessages and LotPrices triggers is in place (the job itself is off).';
GO

SET NOEXEC OFF;
GO
