-- 036_d98_seal_emptying.sql
-- D-98: a container of a delivery is emptied into a batch only after its closing seal was scanned and confirmed intact.
--   * ContainerEmptyings: every seal check at emptying, append-only. Outcome Emptied (the scanned code is the container's
--     closing seal and it is intact: the container is emptied into the batch, its seal is opened and an association container is
--     free again), Mismatch (another code was scanned) or FoundBroken (the seal was found broken). Mismatch and FoundBroken stop
--     the step; the container stays sealed and the record stays visible on the container, the delivery and the batch.
--     A container is emptied once (filtered unique index).
-- The seal history of ContainerSealEvents keeps its four events (the "Broken" event of the emptied seal is added there).
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Containers', N'U') IS NULL OR OBJECT_ID(N'dbo.SealCodes', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 023 and 033 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

IF OBJECT_ID(N'dbo.ContainerEmptyings', N'U') IS NULL
CREATE TABLE dbo.ContainerEmptyings
(
    EmptyingId             int IDENTITY(1, 1) NOT NULL CONSTRAINT PK_ContainerEmptyings PRIMARY KEY,
    ContainerId            int           NOT NULL CONSTRAINT FK_ContainerEmptyings_Containers REFERENCES dbo.Containers (ContainerId),
    FinalHoneyProductionId int           NOT NULL CONSTRAINT FK_ContainerEmptyings_FinalHoneyProductions REFERENCES dbo.FinalHoneyProductions (FinalHoneyProductionId),
    Outcome                varchar(20)   NOT NULL CONSTRAINT CK_ContainerEmptyings_Outcome CHECK (Outcome IN ('Emptied', 'Mismatch', 'FoundBroken')),
    ExpectedSeal           varchar(50)   NOT NULL,
    ScannedSeal            varchar(50)   NULL,
    UserId                 nvarchar(450) NOT NULL,
    At                     datetime2     NOT NULL,
    Note                   nvarchar(300) NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ContainerEmptyings_Container' AND object_id = OBJECT_ID(N'dbo.ContainerEmptyings'))
    CREATE INDEX IX_ContainerEmptyings_Container ON dbo.ContainerEmptyings (ContainerId, EmptyingId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ContainerEmptyings_Batch' AND object_id = OBJECT_ID(N'dbo.ContainerEmptyings'))
    CREATE INDEX IX_ContainerEmptyings_Batch ON dbo.ContainerEmptyings (FinalHoneyProductionId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ContainerEmptyings_Emptied' AND object_id = OBJECT_ID(N'dbo.ContainerEmptyings'))
    CREATE UNIQUE INDEX UX_ContainerEmptyings_Emptied ON dbo.ContainerEmptyings (ContainerId) WHERE Outcome = 'Emptied';
GO

CREATE OR ALTER TRIGGER dbo.TR_ContainerEmptyings_AppendOnly ON dbo.ContainerEmptyings AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 50001, N'ContainerEmptyings is append-only: add a new record instead.', 1;
END;
GO

PRINT '036 done';
GO
