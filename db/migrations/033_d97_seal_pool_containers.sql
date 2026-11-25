-- 033_d97_seal_pool_containers.sql
-- D-97: the delivery step "Containers and samples" with a seal pool, how each container is closed and the jar cap type.
--   * SealBatches / SealCodes: the seal pool of an organization. A batch of seal labels issued by the platform (codes TS-nnnnnn,
--     printed in advance on tamper-evident VOID label stock) or a range of purchased numbered plastic seals. Each code is used
--     once: Issued -> Used (on which container or jar) -> Broken (opened). Codes are unique on the whole platform.
--   * OrgContainers: an organization's own permanent containers (codes TK-nnnn), registered once and used for many deliveries.
--   * Containers: Closure (SealLabel / PlasticSeal / OrgContainer; NULL = recorded before D-97) and OrgContainerId.
--   * LotIntakes: JarCapType of the delivery's sample jars (SafetyButton / TamperRing; NULL = before D-97).
--   * Samples: CapType of the jar (copied from the delivery; NULL = before D-97).
-- Additive and idempotent; a second run changes nothing. Existing deliveries stay valid and keep their confirmed content.
-- SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Containers', N'U') IS NULL OR OBJECT_ID(N'dbo.Organizations', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 023 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

-- ---------------------------------------------------------------- permanent containers of an organization

IF OBJECT_ID(N'dbo.OrgContainers', N'U') IS NULL
CREATE TABLE dbo.OrgContainers
(
    OrgContainerId  int IDENTITY(1, 1) NOT NULL CONSTRAINT PK_OrgContainers PRIMARY KEY,
    OrgId           int            NOT NULL CONSTRAINT FK_OrgContainers_Organizations REFERENCES dbo.Organizations (OrgId),
    Code            varchar(30)    NOT NULL,
    Kind            nvarchar(20)   NOT NULL,
    Name            nvarchar(100)  NULL,
    CapacityKg      decimal(10, 2) NULL,
    TareKg          decimal(10, 2) NULL,
    IsActive        bit            NOT NULL CONSTRAINT DF_OrgContainers_IsActive DEFAULT (1),
    CreatedByUserId nvarchar(450)  NOT NULL,
    CreatedAt       datetime2      NOT NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_OrgContainers_Code' AND object_id = OBJECT_ID(N'dbo.OrgContainers'))
    CREATE UNIQUE INDEX UX_OrgContainers_Code ON dbo.OrgContainers (Code);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrgContainers_OrgId' AND object_id = OBJECT_ID(N'dbo.OrgContainers'))
    CREATE INDEX IX_OrgContainers_OrgId ON dbo.OrgContainers (OrgId);
GO

-- ---------------------------------------------------------------- seal pool

IF OBJECT_ID(N'dbo.SealBatches', N'U') IS NULL
CREATE TABLE dbo.SealBatches
(
    BatchId        int IDENTITY(1, 1) NOT NULL CONSTRAINT PK_SealBatches PRIMARY KEY,
    OrgId          int           NOT NULL CONSTRAINT FK_SealBatches_Organizations REFERENCES dbo.Organizations (OrgId),
    Kind           varchar(20)   NOT NULL CONSTRAINT CK_SealBatches_Kind CHECK (Kind IN ('Label', 'Plastic')),
    FirstCode      varchar(50)   NOT NULL,
    LastCode       varchar(50)   NOT NULL,
    CodeCount      int           NOT NULL,
    Note           nvarchar(200) NULL,
    IssuedByUserId nvarchar(450) NOT NULL,
    IssuedAt       datetime2     NOT NULL
);
GO

IF OBJECT_ID(N'dbo.SealCodes', N'U') IS NULL
CREATE TABLE dbo.SealCodes
(
    SealCodeId   int IDENTITY(1, 1) NOT NULL CONSTRAINT PK_SealCodes PRIMARY KEY,
    BatchId      int           NOT NULL CONSTRAINT FK_SealCodes_SealBatches REFERENCES dbo.SealBatches (BatchId),
    OrgId        int           NOT NULL CONSTRAINT FK_SealCodes_Organizations REFERENCES dbo.Organizations (OrgId),
    Code         varchar(50)   NOT NULL,
    Kind         varchar(20)   NOT NULL CONSTRAINT CK_SealCodes_Kind CHECK (Kind IN ('Label', 'Plastic')),
    Status       varchar(20)   NOT NULL CONSTRAINT DF_SealCodes_Status DEFAULT ('Issued')
                               CONSTRAINT CK_SealCodes_Status CHECK (Status IN ('Issued', 'Used', 'Broken')),
    UsedAs       varchar(20)   NULL CONSTRAINT CK_SealCodes_UsedAs CHECK (UsedAs IS NULL OR UsedAs IN ('Origin', 'Closing', 'Jar', 'CounterJar')),
    ContainerId  int           NULL CONSTRAINT FK_SealCodes_Containers REFERENCES dbo.Containers (ContainerId),
    SampleId     int           NULL CONSTRAINT FK_SealCodes_Samples REFERENCES dbo.Samples (SampleId),
    UsedByUserId nvarchar(450) NULL,
    UsedAt       datetime2     NULL,
    BrokenAt     datetime2     NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_SealCodes_Code' AND object_id = OBJECT_ID(N'dbo.SealCodes'))
    CREATE UNIQUE INDEX UX_SealCodes_Code ON dbo.SealCodes (Code);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SealCodes_Org_Status' AND object_id = OBJECT_ID(N'dbo.SealCodes'))
    CREATE INDEX IX_SealCodes_Org_Status ON dbo.SealCodes (OrgId, Status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SealCodes_BatchId' AND object_id = OBJECT_ID(N'dbo.SealCodes'))
    CREATE INDEX IX_SealCodes_BatchId ON dbo.SealCodes (BatchId);
GO

-- a used code never goes back to the pool
CREATE OR ALTER TRIGGER dbo.TR_SealCodes_OneWay ON dbo.SealCodes AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.SealCodeId = d.SealCodeId
               WHERE i.SealCodeId IS NULL AND d.Status <> 'Issued')
        THROW 50001, N'A used seal code cannot be deleted.', 1;
    IF EXISTS (SELECT 1 FROM deleted d JOIN inserted i ON i.SealCodeId = d.SealCodeId
               WHERE i.Code <> d.Code OR i.OrgId <> d.OrgId
                  OR (d.Status = 'Used' AND i.Status = 'Issued') OR (d.Status = 'Broken' AND i.Status <> 'Broken'))
        THROW 50001, N'A seal code cannot be renamed or returned to the pool.', 1;
END;
GO

-- ---------------------------------------------------------------- the delivery: closure, permanent container, jar cap type

IF COL_LENGTH(N'dbo.Containers', N'Closure') IS NULL
    ALTER TABLE dbo.Containers ADD
        Closure        varchar(20) NULL CONSTRAINT CK_Containers_Closure CHECK (Closure IS NULL OR Closure IN ('SealLabel', 'PlasticSeal', 'OrgContainer')),
        OrgContainerId int         NULL CONSTRAINT FK_Containers_OrgContainers REFERENCES dbo.OrgContainers (OrgContainerId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Containers_OrgContainerId' AND object_id = OBJECT_ID(N'dbo.Containers'))
    CREATE INDEX IX_Containers_OrgContainerId ON dbo.Containers (OrgContainerId) WHERE OrgContainerId IS NOT NULL;
GO

IF COL_LENGTH(N'dbo.LotIntakes', N'JarCapType') IS NULL
    ALTER TABLE dbo.LotIntakes ADD JarCapType varchar(20) NULL
        CONSTRAINT CK_LotIntakes_JarCapType CHECK (JarCapType IS NULL OR JarCapType IN ('SafetyButton', 'TamperRing'));
GO

IF COL_LENGTH(N'dbo.Samples', N'CapType') IS NULL
    ALTER TABLE dbo.Samples ADD CapType varchar(20) NULL
        CONSTRAINT CK_Samples_CapType CHECK (CapType IS NULL OR CapType IN ('SafetyButton', 'TamperRing'));
GO

PRINT '033 done';
GO
