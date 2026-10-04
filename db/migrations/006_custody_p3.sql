-- 006_custody_p3.sql
-- Role and organization model, phase P3 (docs/ROLE-MODEL.md section 6, D-41).
-- Used by the app only when Features:OrgRoleModel = true.
--   * LotIntakes: delivery of honey from a beekeeper record to a Processing organization, with the
--     photo of the signed delivery form (stored under App_Data with its SHA-256) and an optional SMS
--   * Shipments: a quantity of a finalized batch moves from the holding organization to another
--     verified organization; custody moves only when the receiver confirms
--   * HolderStorageRecords / HolderStorageValues: CCP 10 (storage) recorded by each later holder for
--     the quantity it holds
--
-- Additive and idempotent; a second run creates nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.FinalHoneyProductions', N'OwnerOrgId') IS NULL
BEGIN
    RAISERROR(N'Run 005_org_model_p2.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

---------------------------------------------------------------------------------------------------
-- 1. Lot intake
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.LotIntakes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LotIntakes
    (
        IntakeId                 int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LotIntakes PRIMARY KEY,
        RawHoneyProductionId     int           NOT NULL CONSTRAINT FK_LotIntakes_Lot REFERENCES dbo.RawHoneyProductions (RawHoneyProductionId),
        ReceivingOrgId           int           NOT NULL CONSTRAINT FK_LotIntakes_Org REFERENCES dbo.Organizations (OrgId),
        ReceivedByUserId         nvarchar(450) NOT NULL,
        ReceivedAt               datetime2(0)  NOT NULL CONSTRAINT DF_LotIntakes_ReceivedAt DEFAULT (SYSUTCDATETIME()),
        DeliveryDate             date          NOT NULL,
        Quantity                 decimal(18,2) NOT NULL CONSTRAINT CK_LotIntakes_Quantity CHECK (Quantity > 0),
        Province                 nvarchar(100) NOT NULL,
        District                 nvarchar(100) NOT NULL,
        FormStoredFileName       nvarchar(100) NOT NULL,
        FormOriginalFileName     nvarchar(260) NOT NULL,
        FormSha256               char(64)      NOT NULL,
        SmsStatus                nvarchar(30)  NULL,
        SmsAt                    datetime2(0)  NULL
    );
    CREATE UNIQUE INDEX UX_LotIntakes_Lot ON dbo.LotIntakes (RawHoneyProductionId);
    CREATE INDEX IX_LotIntakes_Org ON dbo.LotIntakes (ReceivingOrgId);
    PRINT N'LotIntakes created';
END;
GO

---------------------------------------------------------------------------------------------------
-- 2. Shipments of finalized batches
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Shipments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shipments
    (
        ShipmentId              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Shipments PRIMARY KEY,
        FinalHoneyProductionId  int           NOT NULL CONSTRAINT FK_Shipments_Batch REFERENCES dbo.FinalHoneyProductions (FinalHoneyProductionId),
        FromOrgId               int           NOT NULL CONSTRAINT FK_Shipments_From REFERENCES dbo.Organizations (OrgId),
        ToOrgId                 int           NOT NULL CONSTRAINT FK_Shipments_To REFERENCES dbo.Organizations (OrgId),
        Quantity                decimal(18,2) NOT NULL CONSTRAINT CK_Shipments_Quantity CHECK (Quantity > 0),
        Status                  nvarchar(20)  NOT NULL CONSTRAINT DF_Shipments_Status DEFAULT (N'Pending'),
        Note                    nvarchar(500) NULL,
        ShippedByUserId         nvarchar(450) NOT NULL,
        ShippedAt               datetime2(0)  NOT NULL CONSTRAINT DF_Shipments_ShippedAt DEFAULT (SYSUTCDATETIME()),
        DecidedByUserId         nvarchar(450) NULL,
        DecidedAt               datetime2(0)  NULL,
        RejectReason            nvarchar(500) NULL,
        CONSTRAINT CK_Shipments_Status CHECK (Status IN (N'Pending', N'Received', N'Rejected')),
        CONSTRAINT CK_Shipments_Orgs CHECK (FromOrgId <> ToOrgId)
    );
    CREATE INDEX IX_Shipments_Batch ON dbo.Shipments (FinalHoneyProductionId);
    CREATE INDEX IX_Shipments_To ON dbo.Shipments (ToOrgId, Status);
    PRINT N'Shipments created';
END;
GO

---------------------------------------------------------------------------------------------------
-- 3. CCP 10 (storage) per later holder
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.HolderStorageRecords', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HolderStorageRecords
    (
        RecordId                int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HolderStorageRecords PRIMARY KEY,
        FinalHoneyProductionId  int           NOT NULL CONSTRAINT FK_HolderStorage_Batch REFERENCES dbo.FinalHoneyProductions (FinalHoneyProductionId),
        HolderOrgId             int           NOT NULL CONSTRAINT FK_HolderStorage_Org REFERENCES dbo.Organizations (OrgId),
        CcpId                   int           NOT NULL CONSTRAINT FK_HolderStorage_Ccp REFERENCES dbo.CriticalControlPoints (Id),
        Status                  int           NOT NULL CONSTRAINT FK_HolderStorage_Status REFERENCES dbo.AssignmentStatuses (AssignmentStatusId),
        HeldQuantity            decimal(18,2) NOT NULL,
        RecordedByUserId        nvarchar(450) NULL,
        RecordedAt              datetime2(0)  NULL,
        ApprovedByUserId        nvarchar(450) NULL,
        ApprovedAt              datetime2(0)  NULL,
        SelfApproved            bit           NOT NULL CONSTRAINT DF_HolderStorage_SelfApproved DEFAULT (0)
    );
    CREATE UNIQUE INDEX UX_HolderStorage ON dbo.HolderStorageRecords (FinalHoneyProductionId, HolderOrgId, CcpId);
    PRINT N'HolderStorageRecords created';
END;
GO
IF OBJECT_ID(N'dbo.HolderStorageValues', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HolderStorageValues
    (
        ValueId               int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HolderStorageValues PRIMARY KEY,
        RecordId              int           NOT NULL CONSTRAINT FK_HolderStorageValues_Record REFERENCES dbo.HolderStorageRecords (RecordId),
        ParameterId           int           NOT NULL CONSTRAINT FK_HolderStorageValues_Parameter REFERENCES dbo.CriticalControlPointParameters (ParameterId),
        MeasuredNumericValue  decimal(10,4) NULL,
        MeasuredTextValue     nvarchar(100) NULL,
        IsInRange             bit           NULL,
        AppliedLimit          nvarchar(200) NULL,
        MeasuredAt            datetime2(0)  NOT NULL CONSTRAINT DF_HolderStorageValues_MeasuredAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_HolderStorageValues_Type CHECK ((MeasuredNumericValue IS NOT NULL AND MeasuredTextValue IS NULL) OR (MeasuredNumericValue IS NULL AND MeasuredTextValue IS NOT NULL))
    );
    CREATE UNIQUE INDEX UX_HolderStorageValues ON dbo.HolderStorageValues (RecordId, ParameterId);
    PRINT N'HolderStorageValues created';
END;
GO

SET NOEXEC OFF;
GO
