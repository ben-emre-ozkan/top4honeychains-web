-- 007_lab_p4.sql
-- Role and organization model, phase P4 (docs/ROLE-MODEL.md section 7, D-42).
-- Used by the app only when Features:OrgRoleModel = true.
--   * Samples: taken at intake with a seal number and a retained counter-sample, sent to a verified
--     laboratory; SampleCustodyEvents is the chain of custody (sent, received with seal check, rejected)
--   * LabReports / LabResults: results per CCP parameter, report PDF with its SHA-256, a public report
--     code, approval by a LabManager (four-eyes without exemption, D-27)
--   * CriticalControlPointMonitoringLogs.LabReportId: CCP values that come from an approved lab report
--     (immutable; changed only through correction records)
--
-- Additive and idempotent; a second run creates nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.LotIntakes', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 006_custody_p3.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

---------------------------------------------------------------------------------------------------
-- 1. Samples and their chain of custody
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Samples', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Samples
    (
        SampleId              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Samples PRIMARY KEY,
        SampleCode            nvarchar(30)  NOT NULL,
        RawHoneyProductionId  int           NOT NULL CONSTRAINT FK_Samples_Lot REFERENCES dbo.RawHoneyProductions (RawHoneyProductionId),
        TakingOrgId           int           NOT NULL CONSTRAINT FK_Samples_TakingOrg REFERENCES dbo.Organizations (OrgId),
        LabOrgId              int           NOT NULL CONSTRAINT FK_Samples_LabOrg REFERENCES dbo.Organizations (OrgId),
        SealNumber            nvarchar(50)  NOT NULL,
        CounterSealNumber     nvarchar(50)  NOT NULL,
        CounterLocation       nvarchar(200) NOT NULL,
        RetainUntil           date          NOT NULL,
        Status                nvarchar(20)  NOT NULL CONSTRAINT DF_Samples_Status DEFAULT (N'Sent'),
        TakenByUserId         nvarchar(450) NOT NULL,
        TakenAt               datetime2(0)  NOT NULL CONSTRAINT DF_Samples_TakenAt DEFAULT (SYSUTCDATETIME()),
        ReceivedByUserId      nvarchar(450) NULL,
        ReceivedAt            datetime2(0)  NULL,
        SealIntact            bit           NULL,
        RejectReason          nvarchar(500) NULL,
        CONSTRAINT CK_Samples_Status CHECK (Status IN (N'Sent', N'Received', N'Rejected')),
        CONSTRAINT CK_Samples_Seals CHECK (SealNumber <> CounterSealNumber)
    );
    CREATE UNIQUE INDEX UX_Samples_Code ON dbo.Samples (SampleCode);
    CREATE INDEX IX_Samples_Lot ON dbo.Samples (RawHoneyProductionId);
    CREATE INDEX IX_Samples_Lab ON dbo.Samples (LabOrgId, Status);
    PRINT N'Samples created';
END;
GO
IF OBJECT_ID(N'dbo.SampleCustodyEvents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SampleCustodyEvents
    (
        EventId     int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SampleCustodyEvents PRIMARY KEY,
        SampleId    int           NOT NULL CONSTRAINT FK_SampleCustodyEvents_Sample REFERENCES dbo.Samples (SampleId),
        Event       nvarchar(20)  NOT NULL,
        FromOrgId   int           NULL CONSTRAINT FK_SampleCustodyEvents_From REFERENCES dbo.Organizations (OrgId),
        ToOrgId     int           NULL CONSTRAINT FK_SampleCustodyEvents_To REFERENCES dbo.Organizations (OrgId),
        UserId      nvarchar(450) NOT NULL,
        At          datetime2(0)  NOT NULL CONSTRAINT DF_SampleCustodyEvents_At DEFAULT (SYSUTCDATETIME()),
        SealIntact  bit           NULL,
        Note        nvarchar(500) NULL,
        CONSTRAINT CK_SampleCustodyEvents_Event CHECK (Event IN (N'Sent', N'Received', N'Rejected'))
    );
    CREATE INDEX IX_SampleCustodyEvents_Sample ON dbo.SampleCustodyEvents (SampleId);
    PRINT N'SampleCustodyEvents created';
END;
GO

---------------------------------------------------------------------------------------------------
-- 2. Lab reports and results
---------------------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.LabReports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LabReports
    (
        ReportId              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LabReports PRIMARY KEY,
        SampleId              int           NOT NULL CONSTRAINT FK_LabReports_Sample REFERENCES dbo.Samples (SampleId),
        LabOrgId              int           NOT NULL CONSTRAINT FK_LabReports_Lab REFERENCES dbo.Organizations (OrgId),
        ReportCode            nvarchar(20)  NOT NULL,
        ReportNumber          nvarchar(50)  NULL,
        IssuedOn              date          NULL,
        Status                nvarchar(20)  NOT NULL CONSTRAINT DF_LabReports_Status DEFAULT (N'Draft'),
        StoredFileName        nvarchar(100) NULL,
        OriginalFileName      nvarchar(260) NULL,
        Sha256                char(64)      NULL,
        EnteredByUserId       nvarchar(450) NOT NULL,
        SubmittedAt           datetime2(0)  NULL,
        ApprovedByUserId      nvarchar(450) NULL,
        ApprovedAt            datetime2(0)  NULL,
        ReturnReason          nvarchar(500) NULL,
        IsAccredited          bit           NOT NULL CONSTRAINT DF_LabReports_IsAccredited DEFAULT (0),
        AccreditationNumber   nvarchar(50)  NULL,
        InHouse               bit           NOT NULL CONSTRAINT DF_LabReports_InHouse DEFAULT (0),
        CreatedAt             datetime2(0)  NOT NULL CONSTRAINT DF_LabReports_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_LabReports_Status CHECK (Status IN (N'Draft', N'Submitted', N'Approved')),
        CONSTRAINT CK_LabReports_FourEyes CHECK (ApprovedByUserId IS NULL OR ApprovedByUserId <> EnteredByUserId)
    );
    CREATE UNIQUE INDEX UX_LabReports_Code ON dbo.LabReports (ReportCode);
    CREATE UNIQUE INDEX UX_LabReports_Sample ON dbo.LabReports (SampleId);
    PRINT N'LabReports created';
END;
GO
IF OBJECT_ID(N'dbo.LabResults', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LabResults
    (
        ResultId              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LabResults PRIMARY KEY,
        ReportId              int           NOT NULL CONSTRAINT FK_LabResults_Report REFERENCES dbo.LabReports (ReportId),
        ParameterId           int           NOT NULL CONSTRAINT FK_LabResults_Parameter REFERENCES dbo.CriticalControlPointParameters (ParameterId),
        MeasuredNumericValue  decimal(10,4) NULL,
        MeasuredTextValue     nvarchar(100) NULL,
        IsInRange             bit           NULL,
        AppliedLimit          nvarchar(200) NULL,
        CONSTRAINT CK_LabResults_Type CHECK ((MeasuredNumericValue IS NOT NULL AND MeasuredTextValue IS NULL) OR (MeasuredNumericValue IS NULL AND MeasuredTextValue IS NOT NULL))
    );
    CREATE UNIQUE INDEX UX_LabResults ON dbo.LabResults (ReportId, ParameterId);
    PRINT N'LabResults created';
END;
GO

---------------------------------------------------------------------------------------------------
-- 3. CCP values that come from an approved lab report
---------------------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.CriticalControlPointMonitoringLogs', N'LabReportId') IS NULL
BEGIN
    ALTER TABLE dbo.CriticalControlPointMonitoringLogs ADD LabReportId int NULL
        CONSTRAINT FK_CCPLogs_LabReport REFERENCES dbo.LabReports (ReportId);
    PRINT N'CriticalControlPointMonitoringLogs.LabReportId added';
END;
GO

SET NOEXEC OFF;
GO
