-- 024_d78_lot_prices.sql
-- D-78 P3 (docs/design/D78-SPEC.md 3.8, D-82): the beekeeper's asking price per lot, and the two-admin correction of delivery facts.
--   * LotPrices: append-only price history of a lot (the current price is the latest row). Entered only by the beekeeper's
--     account; locked by the application once the lot is in a finalized batch.
--   * DeliveryCorrections: a platform administrator requests a correction of a delivery recorded with containers (date, weights,
--     container rows, seal numbers) with a reason; a different administrator confirms it. The old and the new confirmed content
--     and their hashes stay in the row. Only a pending row may change, and only its decision columns.
--   * The delivery locks of 023 let a confirmed correction through: while the application applies it, it sets the session value
--     'top4.delivery_correction' (sp_set_session_context) inside the correction's transaction. Nothing else can set it from the UI.
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Containers', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 023 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

-- ---------------------------------------------------------------- prices (3.8)

IF OBJECT_ID(N'dbo.LotPrices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LotPrices
    (
        PriceId              int            IDENTITY(1,1) NOT NULL CONSTRAINT PK_LotPrices PRIMARY KEY,
        RawHoneyProductionId int            NOT NULL CONSTRAINT FK_LotPrices_Lots REFERENCES dbo.RawHoneyProductions (RawHoneyProductionId),
        PricePerKg           decimal(10,2)  NOT NULL,
        Currency             char(3)        NOT NULL,
        Basis                varchar(20)    NOT NULL CONSTRAINT DF_LotPrices_Basis DEFAULT ('Asking'),
        Note                 nvarchar(200)  NULL,
        EnteredByUserId      nvarchar(450)  NOT NULL,
        EnteredAt            datetime2      NOT NULL,
        ClientIp             varchar(45)    NULL,
        CONSTRAINT CK_LotPrices_Price CHECK (PricePerKg > 0 AND PricePerKg < 100000),
        CONSTRAINT CK_LotPrices_Basis CHECK (Basis IN ('Asking', 'Agreed'))
    );
    CREATE INDEX IX_LotPrices_Lot ON dbo.LotPrices (RawHoneyProductionId, PriceId);
    PRINT N'024: LotPrices created.';
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_LotPrices_AppendOnly ON dbo.LotPrices AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'LotPrices is append-only: a new price is a new row.', 1;
END;
GO

-- ---------------------------------------------------------------- delivery corrections (P2 follow-up)

IF OBJECT_ID(N'dbo.DeliveryCorrections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DeliveryCorrections
    (
        CorrectionId         int            IDENTITY(1,1) NOT NULL CONSTRAINT PK_DeliveryCorrections PRIMARY KEY,
        IntakeId             int            NOT NULL CONSTRAINT FK_DeliveryCorrections_Intakes REFERENCES dbo.LotIntakes (IntakeId),
        RawHoneyProductionId int            NOT NULL CONSTRAINT FK_DeliveryCorrections_Lots REFERENCES dbo.RawHoneyProductions (RawHoneyProductionId),
        OldContent           nvarchar(max)  NOT NULL,
        OldContentSha256     char(64)       NOT NULL,
        NewContent           nvarchar(max)  NOT NULL,
        NewContentSha256     char(64)       NOT NULL,
        Reason               nvarchar(500)  NOT NULL,
        Status               varchar(20)    NOT NULL CONSTRAINT DF_DeliveryCorrections_Status DEFAULT ('Pending'),
        RequestedByUserId    nvarchar(450)  NOT NULL,
        RequestedAt          datetime2      NOT NULL,
        DecidedByUserId      nvarchar(450)  NULL,
        DecidedAt            datetime2      NULL,
        DecisionNote         nvarchar(500)  NULL,
        InfoSmsId            bigint         NULL CONSTRAINT FK_DeliveryCorrections_Sms REFERENCES dbo.SmsMessages (SmsId),
        CONSTRAINT CK_DeliveryCorrections_Status CHECK (Status IN ('Pending', 'Applied', 'Rejected')),
        CONSTRAINT CK_DeliveryCorrections_FourEyes CHECK (DecidedByUserId IS NULL OR DecidedByUserId <> RequestedByUserId)
    );
    CREATE INDEX IX_DeliveryCorrections_Intake ON dbo.DeliveryCorrections (IntakeId, CorrectionId);
    PRINT N'024: DeliveryCorrections created.';
END;
GO

-- a decided correction is a record; a pending one changes only by its decision
CREATE OR ALTER TRIGGER dbo.TR_DeliveryCorrections_Guard ON dbo.DeliveryCorrections AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'Delivery corrections cannot be deleted.', 1;
    IF EXISTS (SELECT 1 FROM deleted WHERE Status <> 'Pending')
        THROW 50001, N'A decided delivery correction cannot be changed.', 1;
    IF UPDATE(IntakeId) OR UPDATE(RawHoneyProductionId) OR UPDATE(OldContent) OR UPDATE(OldContentSha256) OR UPDATE(NewContent)
       OR UPDATE(NewContentSha256) OR UPDATE(Reason) OR UPDATE(RequestedByUserId) OR UPDATE(RequestedAt)
        THROW 50001, N'Only the decision of a delivery correction can be recorded.', 1;
END;
GO

-- ---------------------------------------------------------------- the 023 locks, with the correction passage

CREATE OR ALTER TRIGGER dbo.TR_Containers_Immutable ON dbo.Containers AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
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
    IF NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;
    -- a confirmed correction may fix a seal number; it never deletes an event
    IF NOT EXISTS (SELECT 1 FROM inserted) OR SESSION_CONTEXT(N'top4.delivery_correction') IS NULL
       OR UPDATE(ContainerId) OR UPDATE(Event) OR UPDATE(UserId) OR UPDATE(At)
        THROW 50001, N'ContainerSealEvents is append-only: add a new event instead.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_LotIntakes_DeliveryLocked ON dbo.LotIntakes AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
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

CREATE OR ALTER TRIGGER dbo.TR_RawHoneyProductions_DeliveryLocked ON dbo.RawHoneyProductions AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM deleted d JOIN dbo.LotIntakes i ON i.RawHoneyProductionId = d.RawHoneyProductionId AND i.ContainersRecorded = 1)
        RETURN;
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN inserted n ON n.RawHoneyProductionId = d.RawHoneyProductionId
        JOIN dbo.LotIntakes i ON i.RawHoneyProductionId = d.RawHoneyProductionId AND i.ContainersRecorded = 1
        WHERE ISNULL(d.RawHoneyProductionApiaryId, -1) <> ISNULL(n.RawHoneyProductionApiaryId, -1)
           OR ISNULL(d.RawHoneyProductionHoneyType, -1) <> ISNULL(n.RawHoneyProductionHoneyType, -1)
           OR ISNULL(d.RawHoneyProductionPeriodId, -1) <> ISNULL(n.RawHoneyProductionPeriodId, -1)
           OR ISNULL(d.OriginCountry, '') <> ISNULL(n.OriginCountry, '')
           -- amount and date change only through a confirmed delivery correction
           OR (SESSION_CONTEXT(N'top4.delivery_correction') IS NULL
               AND (ISNULL(d.RawHoneyProductionAmount, -1) <> ISNULL(n.RawHoneyProductionAmount, -1)
                    OR ISNULL(d.RawHoneyProductionDate, '19000101') <> ISNULL(n.RawHoneyProductionDate, '19000101'))))
        THROW 50002, N'The delivery facts of a lot recorded with containers cannot be changed (D-81).', 1;
END;
GO

PRINT N'024: lot prices, delivery corrections and the correction passage of the delivery locks are in place.';
GO

SET NOEXEC OFF;
GO
