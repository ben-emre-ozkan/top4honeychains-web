-- 014_conditional_required.sql
-- A CCP parameter can be required only when another parameter of the same CCP has a given answer (D-62):
--   * CriticalControlPointParameters.RequiredIfParameterId  int NULL (FK to the same table)
--   * CriticalControlPointParameters.RequiredIfValue        nvarchar(50) NULL  ("Yes" for a yes/no question)
-- IsRequired still applies; the condition only narrows it. NULL = required as before.
-- Setting: the CCP 0 follow-up "If yes, which methods are employed to clean and disinfect hive tools and equipment?" is
-- required only when "Are hive components regularly cleaned and disinfected?" is answered "Yes". The parameters are
-- found by CCP code and text, not by id.
--
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.CriticalControlPointParameters', N'RequiredIfParameterId') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD RequiredIfParameterId int NULL;
GO
IF COL_LENGTH(N'dbo.CriticalControlPointParameters', N'RequiredIfValue') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters ADD RequiredIfValue nvarchar(50) NULL;
GO
IF OBJECT_ID(N'dbo.FK_CcpParameters_RequiredIf', N'F') IS NULL
    ALTER TABLE dbo.CriticalControlPointParameters
        ADD CONSTRAINT FK_CcpParameters_RequiredIf FOREIGN KEY (RequiredIfParameterId) REFERENCES dbo.CriticalControlPointParameters (ParameterId);
GO

UPDATE f
SET RequiredIfParameterId = q.ParameterId, RequiredIfValue = N'Yes'
FROM dbo.CriticalControlPointParameters f
JOIN dbo.CriticalControlPoints c ON c.Id = f.CCPId AND c.Code = N'CCP 0'
JOIN dbo.CriticalControlPointParameters q ON q.CCPId = f.CCPId AND q.DataType = N'YesNo'
     AND q.ParameterName = N'Are hive components regularly cleaned and disinfected?'
WHERE f.ParameterName LIKE N'If yes, which methods are employed to clean and disinfect%'
  AND f.RequiredIfParameterId IS NULL;
PRINT CONCAT(N'Conditional requirements set: ', @@ROWCOUNT);
GO
