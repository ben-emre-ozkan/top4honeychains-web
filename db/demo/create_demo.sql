-- db/demo/create_demo.sql
-- One complete FICTIONAL chain for presentations (D-48): a processing organization with its laboratory, a
-- beekeepers' association, a retailer, two beekeepers, two raw lots with intake records, a sample of each
-- lot with an approved, accredited lab report, a finalized batch with every CCP approved, a shipment received by the retailer with
-- its CCP 10 record, origin shares and a published beekeeper storefront.
-- Every name starts with "[DEMO]"; remove everything with cleanup_demo.sql. Names keep the Turkish letters
-- (ç, ğ, ı, ö, ş, ü): the script checks them before it commits (D-55; needs 012 and sqlcmd -f 65001).
-- Logins are not created here (no passwords in the repository): add them in the app as platform administrator
-- under Organizations -> organization -> Add member, with user names "demo_*" and "@example.invalid" e-mails,
-- so that cleanup_demo.sql removes them too.
--
-- NEVER run this against production. The script aborts unless it runs on a LocalDB instance and in the local
-- development database TOP4HoneyChainsDb (or the test database TOP4HoneyChainsE2E).
-- Run: sqlcmd -S "(localdb)\MSSQLLocalDB" -d TOP4HoneyChainsDb -I -b -f 65001 -i db\demo\create_demo.sql
-- Files (delivery forms, report PDF, consent document) are not created: their download links answer 404.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF CONVERT(nvarchar(128), SERVERPROPERTY('InstanceName')) NOT LIKE N'LOCALDB#%'
   OR DB_NAME() NOT IN (N'TOP4HoneyChainsDb', N'TOP4HoneyChainsE2E')
BEGIN
    RAISERROR(N'Demo data is only for the local LocalDB development database; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO
IF EXISTS (SELECT 1 FROM dbo.Organizations WHERE DisplayName LIKE N'[[]DEMO]%')
BEGIN
    RAISERROR(N'Demo data already exists; run cleanup_demo.sql first.', 16, 1);
    SET NOEXEC ON;
END;
GO

BEGIN TRANSACTION;

DECLARE @now datetime2(0) = SYSUTCDATETIME(), @today date = CAST(GETDATE() AS date), @seed nvarchar(450) = N'demo-seed';
DECLARE @hash char(64) = REPLICATE('d', 64);

-- master data -------------------------------------------------------------------------------------------
INSERT dbo.Wholesalers (WholesalerTitle, WholesalerEmail, WholesalerCity, WholesalerCountry, WholesalerIsActive, CreatedAt)
VALUES (N'[DEMO] Kovan Bal İşleme', N'demo.processor@example.invalid', N'Demo', N'Türkiye', 1, GETDATE());
DECLARE @ws int = SCOPE_IDENTITY();

INSERT dbo.Organizations (DisplayName, LegalName, Province, District, Status, IsPlatform, FourEyesExempt, LegacyWholesalerId, VerifiedAt, CreatedAt)
VALUES (N'[DEMO] Kovan Bal İşleme', N'[DEMO] Kovan Bal İşleme (örnek kurum)', N'Ankara', N'Demo', N'Verified', 0, 0, @ws, @now, @now);
DECLARE @processor int = SCOPE_IDENTITY();
INSERT dbo.Organizations (DisplayName, Province, District, Status, IsPlatform, FourEyesExempt, VerifiedAt, CreatedAt)
VALUES (N'[DEMO] Analiz Laboratuvarı', N'Ankara', N'Demo', N'Verified', 0, 0, @now, @now);
DECLARE @lab int = SCOPE_IDENTITY();
INSERT dbo.Organizations (DisplayName, Province, District, Status, IsPlatform, FourEyesExempt, VerifiedAt, CreatedAt)
VALUES (N'[DEMO] Arıcılar Birliği', N'Düzce', N'Demo', N'Verified', 0, 0, @now, @now);
DECLARE @association int = SCOPE_IDENTITY();
INSERT dbo.Organizations (DisplayName, Province, District, Status, IsPlatform, FourEyesExempt, VerifiedAt, CreatedAt)
VALUES (N'[DEMO] Şehir Marketi', N'İstanbul', N'Demo', N'Verified', 0, 0, @now, @now);
DECLARE @retail int = SCOPE_IDENTITY();

INSERT dbo.OrgCapabilities (OrgId, Capability, IsAccredited, AccreditationNumber, AccreditationValidUntil, AddedAt) VALUES
    (@processor, N'Processing', 0, NULL, NULL, @now), (@processor, N'Packing', 0, NULL, NULL, @now), (@processor, N'Wholesale', 0, NULL, NULL, @now),
    (@lab, N'Laboratory', 1, N'DEMO-AB-0001', DATEADD(year, 2, @today), @now),
    (@association, N'Association', 0, NULL, NULL, @now),
    (@retail, N'Retail', 0, NULL, NULL, @now);

DECLARE @bk1 nvarchar(450) = CONVERT(nvarchar(450), NEWID()), @bk2 nvarchar(450) = CONVERT(nvarchar(450), NEWID());
INSERT dbo.Beekeepers (BeekeeperId, BeekeeperFirstName, BeekeeperLastName, BeekeeperBirthDate, BeekeeperBusinessNumber, IsActive) VALUES
    (@bk1, N'[DEMO] Ayşe', N'Petek', '1975-05-20', N'DEMO-BEE-0001', 1),
    (@bk2, N'[DEMO] Mehmet', N'Öztürk', '1968-09-02', N'DEMO-BEE-0002', 1);
INSERT dbo.Apiaries (ApiaryBeekeeperId, ApiaryQRCode, ApiaryTitle, ApiaryBeehivesCount, ApiaryLocationLatitude, ApiaryLocationLongitude, ApiaryIsActive) VALUES
    (@bk1, NEWID(), N'[DEMO] Yayla Arılığı', 60, 40.8, 31.2, 1);
DECLARE @apiary1 int = SCOPE_IDENTITY();
INSERT dbo.Apiaries (ApiaryBeekeeperId, ApiaryQRCode, ApiaryTitle, ApiaryBeehivesCount, ApiaryLocationLatitude, ApiaryLocationLongitude, ApiaryIsActive) VALUES
    (@bk2, NEWID(), N'[DEMO] Çam Ormanı Arılığı', 40, 40.9, 31.1, 1);
DECLARE @apiary2 int = SCOPE_IDENTITY();
INSERT dbo.BeekeeperRegistry (BeekeeperId, RegistryNumber, Status, VerifiedAt, VerifiedByUserId) VALUES
    (@bk1, N'TR819999990001', N'Verified', @now, @seed), (@bk2, N'TR819999990002', N'Verified', @now, @seed);
INSERT dbo.BeekeeperAssociations (BeekeeperId, AssociationOrgId, ValidFrom, CreatedByUserId, CreatedAt) VALUES
    (@bk1, @association, @today, @seed, @now), (@bk2, @association, @today, @seed, @now);

-- lots with intake records ----------------------------------------------------------------------------------
DECLARE @period int = (SELECT TOP 1 RawHoneyProductionPeriodId FROM dbo.RawHoneyProductionPeriods ORDER BY RawHoneyProductionPeriodStartDate DESC);
DECLARE @flower int = (SELECT TOP 1 HoneyTypeId FROM dbo.HoneyTypes WHERE HoneyTypeTitle LIKE N'Flower%' ORDER BY HoneyTypeId);
DECLARE @pine int = (SELECT TOP 1 HoneyTypeId FROM dbo.HoneyTypes WHERE HoneyTypeTitle LIKE N'Pine%' ORDER BY HoneyTypeId);
INSERT dbo.RawHoneyProductions (RawHoneyProductionApiaryId, RawHoneyProductionPeriodId, RawHoneyProductionHoneyType, RawHoneyProductionAmount, RawHoneyProductionDate, RawHoneyProductionWholesalerId, OwnerOrgId, OriginCountry)
VALUES (@apiary1, @period, ISNULL(@flower, (SELECT MIN(HoneyTypeId) FROM dbo.HoneyTypes)), 120, DATEADD(day, -20, @today), @ws, @processor, 'TR');
DECLARE @lot1 int = SCOPE_IDENTITY();
INSERT dbo.RawHoneyProductions (RawHoneyProductionApiaryId, RawHoneyProductionPeriodId, RawHoneyProductionHoneyType, RawHoneyProductionAmount, RawHoneyProductionDate, RawHoneyProductionWholesalerId, OwnerOrgId, OriginCountry)
VALUES (@apiary2, @period, ISNULL(@pine, (SELECT MAX(HoneyTypeId) FROM dbo.HoneyTypes)), 80, DATEADD(day, -18, @today), @ws, @processor, 'TR');
DECLARE @lot2 int = SCOPE_IDENTITY();
INSERT dbo.LotIntakes (RawHoneyProductionId, ReceivingOrgId, ReceivedByUserId, ReceivedAt, DeliveryDate, Quantity, Province, District, FormStoredFileName, FormOriginalFileName, FormSha256, SmsStatus, SmsAt) VALUES
    (@lot1, @processor, @seed, @now, DATEADD(day, -20, @today), 120, N'Düzce', N'Demo', N'demo-missing.pdf', N'demo-teslim-1.pdf', @hash, N'Logged', @now),
    (@lot2, @processor, @seed, @now, DATEADD(day, -18, @today), 80, N'Düzce', N'Demo', N'demo-missing.pdf', N'demo-teslim-2.pdf', @hash, NULL, NULL);

-- CCP values of the lots: every raw-lot CCP completed and approved --------------------------------------------
-- a value inside the parameter's limits (middle of min..max), "Yes" for yes/no, "Uygun" for text
DECLARE @lots TABLE (LotId int);
INSERT @lots VALUES (@lot1), (@lot2);
INSERT dbo.RawHoneyCCPAssignments (RawHoneyCCPAssignmentCCP, RawHoneyProductionId, RawHoneyCCPAssignmentStartDate, RawHoneyCCPAssignmentEndDate, RawHoneyCCPAssignmentStatus, ApprovedByUserId, ApprovedByOrgId, ApprovedAt, SelfApproved)
SELECT c.Id, l.LotId, DATEADD(day, -15, GETDATE()), DATEADD(day, -10, GETDATE()), 3, @seed,
       CASE WHEN c.Code = N'CCP 0' THEN @association WHEN c.Code = N'CCP 1' THEN @lab ELSE @processor END, DATEADD(day, -10, GETDATE()), 0
FROM dbo.CriticalControlPoints c CROSS JOIN @lots l WHERE c.AppliesTo = N'RawLot';
INSERT dbo.CriticalControlPointMonitoringLogs (CCPId, ParameterId, RawHoneyProductionId, MeasuredNumericValue, MeasuredTextValue, MeasurementDate, RecordedByUserId, RecordedByOrgId)
SELECT p.CCPId, p.ParameterId, l.LotId,
       CASE WHEN p.DataType IN (N'YesNo', N'Text', N'File') THEN NULL
            WHEN p.MinValue IS NOT NULL AND p.MaxValue IS NOT NULL THEN (p.MinValue + p.MaxValue) / 2
            WHEN p.MinValue IS NOT NULL THEN p.MinValue + 1 WHEN p.MaxValue IS NOT NULL THEN p.MaxValue / 2 ELSE 1 END,
       CASE WHEN p.DataType = N'YesNo' THEN N'Yes' WHEN p.DataType = N'Text' THEN N'Uygun' ELSE NULL END,
       DATEADD(day, -12, GETDATE()), @seed,
       CASE WHEN c.Code = N'CCP 0' THEN @association WHEN c.Code = N'CCP 1' THEN @lab ELSE @processor END
FROM dbo.CriticalControlPointParameters p JOIN dbo.CriticalControlPoints c ON c.Id = p.CCPId CROSS JOIN @lots l
WHERE c.AppliesTo = N'RawLot' AND ISNULL(p.DataType, N'') <> N'File';

-- a sample of each lot and its approved, accredited lab report (CCP 1 values come from it) -----------------------
DECLARE @lot int, @n int = 0, @sample int, @report int;
DECLARE lots CURSOR LOCAL FAST_FORWARD FOR SELECT LotId FROM @lots ORDER BY LotId;
OPEN lots;
FETCH NEXT FROM lots INTO @lot;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @n += 1;
    INSERT dbo.Samples (SampleCode, RawHoneyProductionId, TakingOrgId, LabOrgId, SealNumber, CounterSealNumber, CounterLocation, RetainUntil, Status, TakenByUserId, TakenAt, ReceivedByUserId, ReceivedAt, SealIntact)
    VALUES (CONCAT(N'S-', @lot, N'-1'), @lot, @processor, @lab, CONCAT(N'DEMO-M-', @n, N'A'), CONCAT(N'DEMO-M-', @n, N'B'), N'[DEMO] Numune dolabı 3', DATEADD(year, 1, @today), N'Received', @seed, @now, @seed, @now, 1);
    SET @sample = SCOPE_IDENTITY();
    INSERT dbo.SampleCustodyEvents (SampleId, Event, FromOrgId, ToOrgId, UserId, At, SealIntact) VALUES
        (@sample, N'Sent', @processor, @lab, @seed, @now, NULL), (@sample, N'Received', @processor, @lab, @seed, @now, 1);
    INSERT dbo.LabReports (SampleId, LabOrgId, ReportCode, ReportNumber, IssuedOn, Status, StoredFileName, OriginalFileName, Sha256, EnteredByUserId, SubmittedAt, ApprovedByUserId, ApprovedAt, IsAccredited, AccreditationNumber, InHouse, CreatedAt)
    VALUES (@sample, @lab, CONCAT(N'DEMOREPORT', 25 + @n), CONCAT(N'DEMO-2026-00', @n), DATEADD(day, -11, @today), N'Approved', N'demo-missing.pdf', N'demo-rapor.pdf', @hash, N'demo-analyst', @now, N'demo-labmanager', @now, 1, N'DEMO-AB-0001', 0, @now);
    SET @report = SCOPE_IDENTITY();
    INSERT dbo.LabResults (ReportId, ParameterId, MeasuredNumericValue, MeasuredTextValue)
    SELECT @report, l.ParameterId, l.MeasuredNumericValue, l.MeasuredTextValue
    FROM dbo.CriticalControlPointMonitoringLogs l JOIN dbo.CriticalControlPoints c ON c.Id = l.CCPId
    WHERE l.RawHoneyProductionId = @lot AND c.Code = N'CCP 1';
    UPDATE l SET LabReportId = @report, Notes = CONCAT(N'Lab report DEMOREPORT', 25 + @n)
    FROM dbo.CriticalControlPointMonitoringLogs l JOIN dbo.CriticalControlPoints c ON c.Id = l.CCPId
    WHERE l.RawHoneyProductionId = @lot AND c.Code = N'CCP 1';
    FETCH NEXT FROM lots INTO @lot;
END;
CLOSE lots;
DEALLOCATE lots;

-- finalized batch (100 kg + 50 kg) with every batch CCP approved ------------------------------------------------
INSERT dbo.FinalHoneyProductions (FinalHoneyProductionTitle, FinalHoneyProductionWholesalerId, FinalHoneyProductionQR, FinalHoneyProductionDate, FinalHoneyProductionTotalAmount,
       FinalHoneyProductionIsFinalized, FinalHoneyProductionFinalizedDate, OwnerOrgId, FinalizeRequestedByUserId, FinalizeRequestedAt, FinalizedByUserId, SelfFinalized)
VALUES (N'[DEMO] Yayla Balı 2026', @ws, NEWID(), DATEADD(day, -8, @today), 150, 1, DATEADD(day, -5, GETDATE()), @processor, N'demo-staff', DATEADD(day, -6, GETDATE()), N'demo-quality', 0);
DECLARE @batch int = SCOPE_IDENTITY();
INSERT dbo.FinalHoneyProductionDetails (FinalHoneyProductionId, RawHoneyProductionId, UsedAmount, CreatedAt) VALUES
    (@batch, @lot1, 100, GETDATE()), (@batch, @lot2, 50, GETDATE());
INSERT dbo.FinalHoneyCCPAssignments (FinalHoneyCCPAssignmentCCP, FinalHoneyProductionId, FinalHoneyCCPAssignmentStartDate, FinalHoneyCCPAssignmentEndDate, FinalHoneyCCPAssignmentStatus, ApprovedByUserId, ApprovedByOrgId, ApprovedAt, SelfApproved)
SELECT c.Id, @batch, DATEADD(day, -8, GETDATE()), DATEADD(day, -6, GETDATE()), 3, @seed, @processor, DATEADD(day, -6, GETDATE()), 0
FROM dbo.CriticalControlPoints c WHERE c.AppliesTo = N'Batch';
INSERT dbo.CriticalControlPointMonitoringLogs (CCPId, ParameterId, FinalHoneyProductionId, MeasuredNumericValue, MeasuredTextValue, MeasurementDate, RecordedByUserId, RecordedByOrgId)
SELECT p.CCPId, p.ParameterId, @batch,
       CASE WHEN p.DataType IN (N'YesNo', N'Text', N'File') THEN NULL
            WHEN p.MinValue IS NOT NULL AND p.MaxValue IS NOT NULL THEN (p.MinValue + p.MaxValue) / 2
            WHEN p.MinValue IS NOT NULL THEN p.MinValue + 1 WHEN p.MaxValue IS NOT NULL THEN p.MaxValue / 2 ELSE 1 END,
       CASE WHEN p.DataType = N'YesNo' THEN N'Yes' WHEN p.DataType = N'Text' THEN N'Uygun' ELSE NULL END,
       DATEADD(day, -7, GETDATE()), @seed, @processor
FROM dbo.CriticalControlPointParameters p JOIN dbo.CriticalControlPoints c ON c.Id = p.CCPId
WHERE c.AppliesTo = N'Batch' AND ISNULL(p.DataType, N'') <> N'File';
INSERT dbo.BatchOriginShares (FinalHoneyProductionId, CountryCode, Amount, Percentage, ComputedAt) VALUES (@batch, 'TR', 150, 100, @now);

-- shipment of 60 kg received by the retailer, with its CCP 10 storage record ------------------------------------
INSERT dbo.Shipments (FinalHoneyProductionId, FromOrgId, ToOrgId, Quantity, Status, Note, ShippedByUserId, ShippedAt, DecidedByUserId, DecidedAt)
VALUES (@batch, @processor, @retail, 60, N'Received', N'[DEMO] örnek sevkiyat', @seed, DATEADD(day, -4, GETDATE()), @seed, DATEADD(day, -3, GETDATE()));
DECLARE @ccp10 int = (SELECT Id FROM dbo.CriticalControlPoints WHERE Code = N'CCP 10');
IF @ccp10 IS NOT NULL
BEGIN
    INSERT dbo.HolderStorageRecords (FinalHoneyProductionId, HolderOrgId, CcpId, Status, HeldQuantity, RecordedByUserId, RecordedAt, ApprovedByUserId, ApprovedAt, SelfApproved)
    VALUES (@batch, @retail, @ccp10, 3, 60, @seed, DATEADD(day, -2, GETDATE()), @seed, DATEADD(day, -2, GETDATE()), 0);
    DECLARE @record int = SCOPE_IDENTITY();
    INSERT dbo.HolderStorageValues (RecordId, ParameterId, MeasuredNumericValue, MeasuredTextValue, MeasuredAt)
    SELECT @record, p.ParameterId,
           CASE WHEN p.DataType IN (N'YesNo', N'Text', N'File') THEN NULL
                WHEN p.MinValue IS NOT NULL AND p.MaxValue IS NOT NULL THEN (p.MinValue + p.MaxValue) / 2
                WHEN p.MinValue IS NOT NULL THEN p.MinValue + 1 WHEN p.MaxValue IS NOT NULL THEN p.MaxValue / 2 ELSE 18 END,
           CASE WHEN p.DataType = N'YesNo' THEN N'Yes' WHEN p.DataType = N'Text' THEN N'Uygun' ELSE NULL END, @now
    FROM dbo.CriticalControlPointParameters p WHERE p.CCPId = @ccp10 AND ISNULL(p.DataType, N'') <> N'File';
END;

-- consent and published storefront of the first beekeeper ----------------------------------------------------
INSERT dbo.Consents (BeekeeperId, AssociationOrgId, Purpose, NameConsented, TextVersion, TextSha256, GivenOn, DocumentStoredFileName, DocumentOriginalFileName, DocumentSha256, RecordedByUserId, RecordedAt)
VALUES (@bk1, @association, N'Storefront', 1, N'storefront-2026-10-v1', @hash, @today, N'demo-missing.pdf', N'demo-riza.pdf', @hash, @seed, @now);
INSERT dbo.BeekeeperStorefronts (BeekeeperId, Code, CreatedAt) VALUES (@bk1, N'BK-DEMQ2026', @now);

-- D-55: the Turkish letters must arrive unchanged (sqlcmd -f 65001, migration 012 applied); otherwise nothing is kept
IF NOT EXISTS (SELECT 1 FROM dbo.Beekeepers WHERE BeekeeperId = @bk1 AND BeekeeperFirstName = N'[DEMO] Ayşe')
   OR NOT EXISTS (SELECT 1 FROM dbo.Beekeepers WHERE BeekeeperId = @bk2 AND BeekeeperLastName = N'Öztürk')
   OR NOT EXISTS (SELECT 1 FROM dbo.Apiaries WHERE ApiaryId = @apiary2 AND ApiaryTitle = N'[DEMO] Çam Ormanı Arılığı')
   OR NOT EXISTS (SELECT 1 FROM dbo.Organizations WHERE OrgId = @association AND DisplayName = N'[DEMO] Arıcılar Birliği')
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR(N'Turkish letters were not stored as typed: run with sqlcmd -f 65001 after migration 012_unicode_names.sql. Nothing was created.', 16, 1);
    SET NOEXEC ON;
END;

COMMIT TRANSACTION;

SELECT CONCAT(N'Demo chain created. Batch #', @batch, N' QR /honeystory/', (SELECT FinalHoneyProductionQR FROM dbo.FinalHoneyProductions WHERE FinalHoneyProductionId = @batch),
              N' · storefront /storefront/BK-DEMQ2026 · report /verify-report?code=DEMOREPORT26') AS Result;
GO

SET NOEXEC OFF;
GO
