-- 015_ccp0_reception_officer.sql
-- New default responsibility for CCP 0 (D-63, confirmed by the project lead): the sample acceptance officer of the
-- organization that owns the lot (the person who records the delivery) performs CCP 0, and a quality approver of
-- that organization approves it. Before: the field inspector of the beekeeper's association, approved by the
-- association's quality approver. FieldInspector stays an organization role that an administrator can still assign
-- to CCP 0 on the CCP responsibilities page.
--
-- Changed only while CCP 0 still has the old default (association / Association / FieldInspector / QualityApprover),
-- so a choice an administrator made later is never overwritten. Idempotent; a second run changes nothing.
-- Needs 005 (CcpResponsibilities). SQL Server 2019 (level 150). Run with sqlcmd -I.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.CcpResponsibilities', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 005_org_model_p2.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

DECLARE @ccp0 int = (SELECT Id FROM dbo.CriticalControlPoints WHERE Code = N'CCP 0');

IF @ccp0 IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.CcpResponsibilities WHERE CcpId = @ccp0 AND OwnerRule = N'BeekeeperAssociation')
   AND NOT EXISTS (SELECT 1 FROM dbo.CcpResponsibilityCapabilities WHERE CcpId = @ccp0 AND Capability <> N'Association')
   AND NOT EXISTS (SELECT 1 FROM dbo.CcpResponsibilityRoles WHERE CcpId = @ccp0
                   AND NOT ((Action = N'Enter' AND OrgRole = N'FieldInspector') OR (Action = N'Approve' AND OrgRole = N'QualityApprover')))
BEGIN
    BEGIN TRANSACTION;
    UPDATE dbo.CcpResponsibilities SET OwnerRule = N'Owner', RequiresApproval = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedByUserId = NULL WHERE CcpId = @ccp0;
    DELETE FROM dbo.CcpResponsibilityCapabilities WHERE CcpId = @ccp0;
    INSERT dbo.CcpResponsibilityCapabilities (CcpId, Capability) VALUES (@ccp0, N'Processing');
    DELETE FROM dbo.CcpResponsibilityRoles WHERE CcpId = @ccp0;
    INSERT dbo.CcpResponsibilityRoles (CcpId, Action, OrgRole) VALUES (@ccp0, N'Enter', N'SampleAcceptanceOfficer'), (@ccp0, N'Approve', N'QualityApprover');
    COMMIT TRANSACTION;
    PRINT N'CCP 0: sample acceptance officer of the lot owner enters, quality approver of the lot owner approves.';
END
ELSE
    PRINT N'CCP 0 responsibility left as it is (already changed, or not the old default).';
GO

SET NOEXEC OFF;
GO
