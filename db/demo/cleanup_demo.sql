-- db/demo/cleanup_demo.sql
-- Removes the fictional demo chain created by create_demo.sql (everything named "[DEMO]"; D-48).
-- NEVER run this against production. The script aborts unless it runs on a LocalDB instance and in the local
-- development database TOP4HoneyChainsDb (or the test database TOP4HoneyChainsE2E).
-- Run: sqlcmd -S "(localdb)\MSSQLLocalDB" -d TOP4HoneyChainsDb -I -b -f 65001 -i db\demo\cleanup_demo.sql
-- Audit rows written while the demo was used in the app stay in the append-only log (D-28).

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

BEGIN TRANSACTION;

DECLARE @orgs TABLE (id int);
INSERT @orgs SELECT OrgId FROM dbo.Organizations WHERE DisplayName LIKE N'[[]DEMO]%' AND IsPlatform = 0;
DECLARE @ws TABLE (id int);
INSERT @ws SELECT WholesalerId FROM dbo.Wholesalers WHERE WholesalerTitle LIKE N'[[]DEMO]%';
DECLARE @bk TABLE (id nvarchar(450));
INSERT @bk SELECT BeekeeperId FROM dbo.Beekeepers WHERE BeekeeperBusinessNumber LIKE N'DEMO-BEE-%' AND BeekeeperFirstName LIKE N'[[]DEMO]%';
DECLARE @ap TABLE (id int);
INSERT @ap SELECT ApiaryId FROM dbo.Apiaries WHERE ApiaryBeekeeperId IN (SELECT id FROM @bk);
DECLARE @lots TABLE (id int);
INSERT @lots SELECT RawHoneyProductionId FROM dbo.RawHoneyProductions WHERE RawHoneyProductionApiaryId IN (SELECT id FROM @ap);
DECLARE @batches TABLE (id int);
INSERT @batches SELECT FinalHoneyProductionId FROM dbo.FinalHoneyProductions WHERE FinalHoneyProductionTitle LIKE N'[[]DEMO]%' AND FinalHoneyProductionWholesalerId IN (SELECT id FROM @ws);
DECLARE @samples TABLE (id int);
INSERT @samples SELECT SampleId FROM dbo.Samples WHERE RawHoneyProductionId IN (SELECT id FROM @lots);

SELECT (SELECT COUNT(*) FROM @orgs) orgs, (SELECT COUNT(*) FROM @bk) beekeepers, (SELECT COUNT(*) FROM @lots) lots, (SELECT COUNT(*) FROM @batches) batches;

DELETE v FROM dbo.HolderStorageValues v JOIN dbo.HolderStorageRecords r ON r.RecordId = v.RecordId WHERE r.FinalHoneyProductionId IN (SELECT id FROM @batches);
DELETE FROM dbo.HolderStorageRecords WHERE FinalHoneyProductionId IN (SELECT id FROM @batches);
DELETE FROM dbo.Shipments WHERE FinalHoneyProductionId IN (SELECT id FROM @batches);
DELETE FROM dbo.BatchOriginShares WHERE FinalHoneyProductionId IN (SELECT id FROM @batches);
DELETE FROM dbo.CcpCorrections WHERE (OwnerKind = N'raw' AND OwnerId IN (SELECT id FROM @lots)) OR (OwnerKind = N'batch' AND OwnerId IN (SELECT id FROM @batches));
DELETE FROM dbo.CriticalControlPointMonitoringLogs WHERE RawHoneyProductionId IN (SELECT id FROM @lots) OR FinalHoneyProductionId IN (SELECT id FROM @batches);
DELETE FROM dbo.LabResults WHERE ReportId IN (SELECT ReportId FROM dbo.LabReports WHERE SampleId IN (SELECT id FROM @samples));
DELETE FROM dbo.LabReports WHERE SampleId IN (SELECT id FROM @samples);
DELETE FROM dbo.SampleCustodyEvents WHERE SampleId IN (SELECT id FROM @samples);
DELETE FROM dbo.Samples WHERE SampleId IN (SELECT id FROM @samples);
DELETE FROM dbo.FinalHoneyCCPAssignments WHERE FinalHoneyProductionId IN (SELECT id FROM @batches);
DELETE FROM dbo.FinalHoneyProductionDetails WHERE FinalHoneyProductionId IN (SELECT id FROM @batches);
DELETE FROM dbo.FinalHoneyProductions WHERE FinalHoneyProductionId IN (SELECT id FROM @batches);
DELETE FROM dbo.LotIntakes WHERE RawHoneyProductionId IN (SELECT id FROM @lots);
DELETE FROM dbo.RawHoneyCCPAssignments WHERE RawHoneyProductionId IN (SELECT id FROM @lots);
DELETE FROM dbo.RawHoneyProductions WHERE RawHoneyProductionId IN (SELECT id FROM @lots);
DELETE FROM dbo.Consents WHERE BeekeeperId IN (SELECT id FROM @bk);
DELETE FROM dbo.BeekeeperStorefronts WHERE BeekeeperId IN (SELECT id FROM @bk);
DELETE FROM dbo.BeekeeperAssociations WHERE BeekeeperId IN (SELECT id FROM @bk) OR AssociationOrgId IN (SELECT id FROM @orgs);
DELETE FROM dbo.RegistryVerifications WHERE BeekeeperId IN (SELECT id FROM @bk);
DELETE FROM dbo.BeekeeperRegistry WHERE BeekeeperId IN (SELECT id FROM @bk);
DELETE FROM dbo.ApiaryPhotos WHERE ApiaryPhotoApiaryId IN (SELECT id FROM @ap);
DELETE FROM dbo.Apiaries WHERE ApiaryId IN (SELECT id FROM @ap);
DELETE FROM dbo.Beekeepers WHERE BeekeeperId IN (SELECT id FROM @bk);
DELETE FROM dbo.Memberships WHERE OrgId IN (SELECT id FROM @orgs);
DELETE FROM dbo.OrgDocuments WHERE OrgId IN (SELECT id FROM @orgs);
DELETE FROM dbo.OrgVerificationEvents WHERE OrgId IN (SELECT id FROM @orgs);
DELETE FROM dbo.OrgCapabilities WHERE OrgId IN (SELECT id FROM @orgs);
DELETE FROM dbo.Organizations WHERE OrgId IN (SELECT id FROM @orgs);
DELETE FROM dbo.Wholesalers WHERE WholesalerId IN (SELECT id FROM @ws);

COMMIT TRANSACTION;
PRINT N'Demo chain removed.';
GO

SET NOEXEC OFF;
GO
