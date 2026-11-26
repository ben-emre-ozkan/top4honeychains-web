-- 023_d78_containers_samples.sql
-- D-78 P2 (docs/design/D78-SPEC.md 3.2, 3.3, 3.9, 3.10, D-81): sealed containers, a sample from every container, the delivery
-- confirmation and the chain of custody.
--   * Containers: one row per physical container of a delivery (code L{lot}-C{n}, kind, net weight, origin seal). Never changed
--     or deleted after insert.
--   * ContainerSealEvents: append-only seal history of a container (Checked origin seal, Applied plant seal, Broken, Replaced).
--   * Samples: nullable ContainerId, Kind (Container / Composite; NULL = legacy lot sample), the tamper-evident cap at sealing and
--     the reception checks (cap, label, seal number). Existing samples stay valid (all NULL).
--   * SampleSources: the container jars a composite sample was made from (insert-only).
--   * LotIntakes: nullable confirmation columns (method, content hash, code, photo review) and ContainersRecorded; the delivery-form
--     columns become nullable (the signed photo is now the fallback). Widening only; existing intakes unchanged (read as legacy).
--   * Triggers: a delivery recorded with containers (ContainersRecorded = 1) cannot change its facts, neither in LotIntakes nor in
--     its lot (RawHoneyProductions: apiary, honey type, period, amount, date, origin country).
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Samples', N'U') IS NULL OR OBJECT_ID(N'dbo.OneTimeCodes', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 007 and 019 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

-- ---------------------------------------------------------------- intake confirmation

IF COL_LENGTH(N'dbo.LotIntakes', N'ContainersRecorded') IS NULL
    ALTER TABLE dbo.LotIntakes ADD
        ConfirmationMethod varchar(20)   NULL,
        ContentSha256      char(64)      NULL,
        ConfirmedAt        datetime2     NULL,
        CodeId             bigint        NULL CONSTRAINT FK_LotIntakes_OneTimeCodes REFERENCES dbo.OneTimeCodes (CodeId),
        FallbackReason     nvarchar(300) NULL,
        EvidenceStatus     varchar(20)   NULL,
        ReviewedByUserId   nvarchar(450) NULL,
        ReviewedAt         datetime2     NULL,
        ReviewReason       nvarchar(500) NULL,
        ContainersRecorded bit           NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.LotIntakes') AND name = N'FormStoredFileName' AND is_nullable = 0)
    ALTER TABLE dbo.LotIntakes ALTER COLUMN FormStoredFileName nvarchar(100) NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.LotIntakes') AND name = N'FormOriginalFileName' AND is_nullable = 0)
    ALTER TABLE dbo.LotIntakes ALTER COLUMN FormOriginalFileName nvarchar(260) NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.LotIntakes') AND name = N'FormSha256' AND is_nullable = 0)
    ALTER TABLE dbo.LotIntakes ALTER COLUMN FormSha256 char(64) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_LotIntakes_Confirmation')
    ALTER TABLE dbo.LotIntakes ADD CONSTRAINT CK_LotIntakes_Confirmation CHECK
        ((ConfirmationMethod IS NULL OR ConfirmationMethod IN ('SmsCode', 'SignedPhoto'))
         AND (EvidenceStatus IS NULL OR EvidenceStatus IN ('Accepted', 'PendingReview', 'Rejected')));
GO

-- ---------------------------------------------------------------- containers and seals

IF OBJECT_ID(N'dbo.Containers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Containers
    (
        ContainerId          int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Containers PRIMARY KEY,
        IntakeId             int           NOT NULL CONSTRAINT FK_Containers_LotIntakes REFERENCES dbo.LotIntakes (IntakeId),
        RawHoneyProductionId int           NOT NULL CONSTRAINT FK_Containers_RawHoneyProductions REFERENCES dbo.RawHoneyProductions (RawHoneyProductionId),
        Seq                  int           NOT NULL,
        ContainerCode        nvarchar(30)  NOT NULL,
        Kind                 varchar(20)   NOT NULL,
        NetWeightKg          decimal(10,2) NOT NULL,
        GrossWeightKg        decimal(10,2) NULL,
        OriginSealNumber     nvarchar(50)  NULL,
        OriginSealIntact     bit           NULL,
        CreatedByUserId      nvarchar(450) NOT NULL,
        CreatedAt            datetime2     NOT NULL CONSTRAINT DF_Containers_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_Containers_Kind CHECK (Kind IN ('Tin', 'Drum', 'Bucket', 'Ibc', 'Other')),
        CONSTRAINT CK_Containers_Net CHECK (NetWeightKg > 0)
    );
    CREATE UNIQUE INDEX UX_Containers_Code ON dbo.Containers (ContainerCode);
    CREATE UNIQUE INDEX UX_Containers_LotSeq ON dbo.Containers (RawHoneyProductionId, Seq);
    CREATE INDEX IX_Containers_Intake ON dbo.Containers (IntakeId);
END;
GO

IF OBJECT_ID(N'dbo.ContainerSealEvents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContainerSealEvents
    (
        EventId             int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ContainerSealEvents PRIMARY KEY,
        ContainerId         int           NOT NULL CONSTRAINT FK_ContainerSealEvents_Containers REFERENCES dbo.Containers (ContainerId),
        Event               varchar(20)   NOT NULL,
        SealNumber          nvarchar(50)  NOT NULL,
        Intact              bit           NULL,
        UserId              nvarchar(450) NOT NULL,
        At                  datetime2     NOT NULL CONSTRAINT DF_ContainerSealEvents_At DEFAULT (SYSUTCDATETIME()),
        Note                nvarchar(300) NULL,
        PhotoStoredFileName nvarchar(100) NULL,
        PhotoSha256         char(64)      NULL,
        CONSTRAINT CK_ContainerSealEvents_Event CHECK (Event IN ('Checked', 'Applied', 'Broken', 'Replaced'))
    );
    CREATE INDEX IX_ContainerSealEvents_Container ON dbo.ContainerSealEvents (ContainerId, EventId);
END;
GO

-- ---------------------------------------------------------------- samples per container, composites

IF COL_LENGTH(N'dbo.Samples', N'ContainerId') IS NULL
    ALTER TABLE dbo.Samples ADD
        ContainerId             int         NULL CONSTRAINT FK_Samples_Containers REFERENCES dbo.Containers (ContainerId),
        Kind                    varchar(20) NULL,
        JarCapIntactAtSealing   bit         NULL,
        JarCapIntactAtReception bit         NULL,
        LabelIntactAtReception  bit         NULL,
        SealNumberMatches       bit         NULL;
GO

-- D-98: not again once 034 has replaced it by the wider CK_Samples_Kind2 (a second run of 023, or db/install followed by
-- every migration, would otherwise bring back the narrow check and refuse batch samples)
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name IN (N'CK_Samples_Kind', N'CK_Samples_Kind2'))
    ALTER TABLE dbo.Samples ADD CONSTRAINT CK_Samples_Kind CHECK (Kind IS NULL OR Kind IN ('Container', 'Composite'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Samples_Container' AND object_id = OBJECT_ID(N'dbo.Samples'))
    CREATE INDEX IX_Samples_Container ON dbo.Samples (ContainerId);
GO

IF OBJECT_ID(N'dbo.SampleSources', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SampleSources
    (
        CompositeSampleId int       NOT NULL CONSTRAINT FK_SampleSources_Composite REFERENCES dbo.Samples (SampleId),
        SourceSampleId    int       NOT NULL CONSTRAINT FK_SampleSources_Source REFERENCES dbo.Samples (SampleId),
        CreatedAt         datetime2 NOT NULL CONSTRAINT DF_SampleSources_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_SampleSources PRIMARY KEY (CompositeSampleId, SourceSampleId)
    );
END;
GO

-- ---------------------------------------------------------------- immutability (D78-SPEC 3.10)

CREATE OR ALTER TRIGGER dbo.TR_Containers_Immutable ON dbo.Containers AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'Containers of a delivery cannot be changed or deleted.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_ContainerSealEvents_AppendOnly ON dbo.ContainerSealEvents AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'ContainerSealEvents is append-only: add a new event instead.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_SampleSources_InsertOnly ON dbo.SampleSources AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'The sources of a composite sample cannot be changed.', 1;
END;
GO

-- a delivery recorded with containers keeps its facts; only the review of a signed photo changes later
CREATE OR ALTER TRIGGER dbo.TR_LotIntakes_DeliveryLocked ON dbo.LotIntakes AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM deleted WHERE ContainersRecorded = 1)
        RETURN;
    IF NOT EXISTS (SELECT 1 FROM inserted)
        THROW 50001, N'A delivery recorded with containers cannot be deleted.', 1;
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
           OR ISNULL(d.RawHoneyProductionAmount, -1) <> ISNULL(n.RawHoneyProductionAmount, -1)
           OR ISNULL(d.RawHoneyProductionDate, '19000101') <> ISNULL(n.RawHoneyProductionDate, '19000101')
           OR ISNULL(d.OriginCountry, '') <> ISNULL(n.OriginCountry, ''))
        THROW 50002, N'The delivery facts of a lot recorded with containers cannot be changed (D-81).', 1;
END;
GO

PRINT N'023: containers, seal events, samples per container, composite sources, intake confirmation and the delivery locks are in place.';
GO

SET NOEXEC OFF;
GO
