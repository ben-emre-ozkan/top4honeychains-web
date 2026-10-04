-- 008_storefront_origin_p5.sql
-- Role and organization model, phase P5 (docs/ROLE-MODEL.md section 11 and 12, D-30, D-31, D-43).
--   * Consents: storefront consent of a beekeeper, recorded by the association (date, consent document
--     with SHA-256, consent text version and hash); withdrawal unpublishes the storefront at once
--   * BeekeeperStorefronts: a stable, random public code per beekeeper (never derived from the name)
--   * RawHoneyProductions.OriginCountry: ISO 3166-1 alpha-2; existing rows stay NULL (read as TR),
--     new rows get TR by default
--   * BatchOriginShares: country percentages of a batch by weight, stored at finalize (EU 2024/1438)
-- Read by the app only when Features:OrgRoleModel = true, except that finalize stores the origin shares.
--
-- Additive and idempotent; a second run creates nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.LabReports', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 007_lab_p4.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

---------------------------------------------------------------------------------------------------
-- 1. Storefront consent and public code
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Consents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Consents
    (
        ConsentId                 int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Consents PRIMARY KEY,
        BeekeeperId               nvarchar(450) NOT NULL CONSTRAINT FK_Consents_Beekeeper REFERENCES dbo.Beekeepers (BeekeeperId),
        AssociationOrgId          int           NOT NULL CONSTRAINT FK_Consents_Association REFERENCES dbo.Organizations (OrgId),
        Purpose                   nvarchar(30)  NOT NULL,
        NameConsented             bit           NOT NULL CONSTRAINT DF_Consents_NameConsented DEFAULT (0),
        TextVersion               nvarchar(30)  NOT NULL,
        TextSha256                char(64)      NOT NULL,
        GivenOn                   date          NOT NULL,
        DocumentStoredFileName    nvarchar(100) NOT NULL,
        DocumentOriginalFileName  nvarchar(260) NOT NULL,
        DocumentSha256            char(64)      NOT NULL,
        RecordedByUserId          nvarchar(450) NOT NULL,
        RecordedAt                datetime2(0)  NOT NULL CONSTRAINT DF_Consents_RecordedAt DEFAULT (SYSUTCDATETIME()),
        WithdrawnAt               datetime2(0)  NULL,
        WithdrawnByUserId         nvarchar(450) NULL,
        WithdrawReason            nvarchar(500) NULL,
        CONSTRAINT CK_Consents_Purpose CHECK (Purpose IN (N'Storefront'))
    );
    -- at most one active consent per beekeeper and purpose
    CREATE UNIQUE INDEX UX_Consents_Active ON dbo.Consents (BeekeeperId, Purpose) WHERE WithdrawnAt IS NULL;
    PRINT N'Consents created';
END;
GO
IF OBJECT_ID(N'dbo.BeekeeperStorefronts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BeekeeperStorefronts
    (
        BeekeeperId  nvarchar(450) NOT NULL CONSTRAINT PK_BeekeeperStorefronts PRIMARY KEY
                                   CONSTRAINT FK_BeekeeperStorefronts_Beekeeper REFERENCES dbo.Beekeepers (BeekeeperId),
        Code         nvarchar(20)  NOT NULL,
        CreatedAt    datetime2(0)  NOT NULL CONSTRAINT DF_BeekeeperStorefronts_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
    CREATE UNIQUE INDEX UX_BeekeeperStorefronts_Code ON dbo.BeekeeperStorefronts (Code);
    PRINT N'BeekeeperStorefronts created';
END;
GO

---------------------------------------------------------------------------------------------------
-- 2. Origin country of lots and origin shares of batches (EU 2024/1438)
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.RawHoneyProductions', N'OriginCountry') IS NULL
BEGIN
    -- nullable: existing lots keep NULL (read as TR); the default applies to new rows only
    ALTER TABLE dbo.RawHoneyProductions ADD OriginCountry char(2) NULL
        CONSTRAINT DF_RawHoneyProductions_OriginCountry DEFAULT ('TR');
    PRINT N'RawHoneyProductions.OriginCountry added';
END;
GO
IF OBJECT_ID(N'dbo.BatchOriginShares', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BatchOriginShares
    (
        FinalHoneyProductionId  int           NOT NULL CONSTRAINT FK_BatchOriginShares_Batch REFERENCES dbo.FinalHoneyProductions (FinalHoneyProductionId),
        CountryCode             char(2)       NOT NULL,
        Amount                  decimal(18,2) NOT NULL,
        Percentage              decimal(5,2)  NOT NULL,
        ComputedAt              datetime2(0)  NOT NULL CONSTRAINT DF_BatchOriginShares_ComputedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_BatchOriginShares PRIMARY KEY (FinalHoneyProductionId, CountryCode),
        CONSTRAINT CK_BatchOriginShares_Percentage CHECK (Percentage >= 0 AND Percentage <= 100)
    );
    PRINT N'BatchOriginShares created';
END;
GO

SET NOEXEC OFF;
GO
