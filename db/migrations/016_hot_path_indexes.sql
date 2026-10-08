-- 016_hot_path_indexes.sql
-- D-73: indexes on foreign keys that the busiest pages filter by and that had none: apiary photos by apiary (the
-- Honey Story and the apiary pages; without it every photo row, blobs included, is scanned), apiaries by beekeeper,
-- lots by apiary, batch composition by lot, parameters by CCP and monitoring values by CCP.
-- Additive and idempotent (each index only if missing); no data changes. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ApiaryPhotos', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ApiaryPhotos') AND name = N'IX_ApiaryPhotos_ApiaryPhotoApiaryId')
    CREATE NONCLUSTERED INDEX IX_ApiaryPhotos_ApiaryPhotoApiaryId ON dbo.ApiaryPhotos (ApiaryPhotoApiaryId);
GO

IF OBJECT_ID(N'dbo.Apiaries', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Apiaries') AND name = N'IX_Apiaries_ApiaryBeekeeperId')
    CREATE NONCLUSTERED INDEX IX_Apiaries_ApiaryBeekeeperId ON dbo.Apiaries (ApiaryBeekeeperId);
GO

IF OBJECT_ID(N'dbo.RawHoneyProductions', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.RawHoneyProductions') AND name = N'IX_RawHoneyProductions_RawHoneyProductionApiaryId')
    CREATE NONCLUSTERED INDEX IX_RawHoneyProductions_RawHoneyProductionApiaryId ON dbo.RawHoneyProductions (RawHoneyProductionApiaryId);
GO

IF OBJECT_ID(N'dbo.FinalHoneyProductionDetails', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.FinalHoneyProductionDetails') AND name = N'IX_FinalHoneyProductionDetails_RawHoneyProductionId')
    CREATE NONCLUSTERED INDEX IX_FinalHoneyProductionDetails_RawHoneyProductionId ON dbo.FinalHoneyProductionDetails (RawHoneyProductionId);
GO

IF OBJECT_ID(N'dbo.CriticalControlPointParameters', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.CriticalControlPointParameters') AND name = N'IX_CriticalControlPointParameters_CCPId')
    CREATE NONCLUSTERED INDEX IX_CriticalControlPointParameters_CCPId ON dbo.CriticalControlPointParameters (CCPId);
GO

IF OBJECT_ID(N'dbo.CriticalControlPointMonitoringLogs', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.CriticalControlPointMonitoringLogs') AND name = N'IX_CriticalControlPointMonitoringLogs_CCPId')
    CREATE NONCLUSTERED INDEX IX_CriticalControlPointMonitoringLogs_CCPId ON dbo.CriticalControlPointMonitoringLogs (CCPId);
GO
