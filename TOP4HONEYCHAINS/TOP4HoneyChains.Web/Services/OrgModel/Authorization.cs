using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    public static class AuthPolicies
    {
        // Who may use the web panel: administrators; with the flag on also users with an active membership
        public const string PanelUser = "PanelUser";
        public const string PlatformAdmin = "PlatformAdmin";
        // Any signed-in user (own profile, sign-out)
        public const string SignedIn = "SignedIn";
        // D-78 P3 (D78-SPEC 2.4): an active beekeeper account (the portal /Me)
        public const string BeekeeperSelf = "BeekeeperSelf";
    }

    public class PanelUserRequirement : IAuthorizationRequirement { }

    public class PanelUserHandler : AuthorizationHandler<PanelUserRequirement>
    {
        private readonly ICurrentOrg _current;
        public PanelUserHandler(ICurrentOrg current) => _current = current;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PanelUserRequirement requirement)
        {
            if (context.User.IsInRole(CurrentOrg.AdminRole) || (_current.Enabled && await _current.GetAsync() != null))
                context.Succeed(requirement);
        }
    }

    // Which organization owns a lot or batch, and which lots and batches the active membership may list
    // (ROLE-MODEL section 14). Owner = OwnerOrgId (D-36), or the organization created from the wholesaler
    // while none is set; existing rows are not modified.
    public class OrgScope
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly CcpResponsibilityService _resp;
        private readonly IConfiguration _config;

        public OrgScope(TOP4HoneyChainsContext context, ICurrentOrg current, CcpResponsibilityService resp, IConfiguration config)
        {
            _context = context;
            _current = current;
            _resp = resp;
            _config = config;
        }

        public Task<int?> LotOrgIdAsync(int rawHoneyProductionId) => _resp.LotOwnerAsync(rawHoneyProductionId);

        public Task<int?> BatchOrgIdAsync(int finalHoneyProductionId) => _resp.BatchOwnerAsync(finalHoneyProductionId);

        // Administrators and public authorities read everything
        public async Task<bool> ReadsAllAsync()
        {
            if (_current.IsPlatformAdmin)
                return true;
            var active = await _current.GetAsync();
            return active != null && active.Has(Capabilities.PublicAuthority) && active.IsVerified;
        }

        private async Task<int?> LegacyWholesalerOfAsync(int orgId) =>
            await _context.Organizations.Where(o => o.OrgId == orgId).Select(o => o.LegacyWholesalerId).FirstOrDefaultAsync();

        public async Task<IQueryable<FinalHoneyProduction>> OwnedBatchesAsync(int orgId)
        {
            var ws = await LegacyWholesalerOfAsync(orgId);
            return _context.FinalHoneyProductions.Where(f =>
                f.OwnerOrgId == orgId || (f.OwnerOrgId == null && ws != null && f.FinalHoneyProductionWholesalerId == ws));
        }

        // Own lots, lots of member beekeepers (associations, CCP 0) and the lots of own batches (upstream)
        public async Task<IQueryable<RawHoneyProduction>> VisibleLotsAsync()
        {
            if (await ReadsAllAsync())
                return _context.RawHoneyProductions;
            var active = await _current.GetAsync();
            if (active == null)
                return _context.RawHoneyProductions.Where(r => false);
            var orgId = active.OrgId;
            var ws = await LegacyWholesalerOfAsync(orgId);
            var ownBatchIds = (await OwnedBatchesAsync(orgId)).Select(f => f.FinalHoneyProductionId);
            return _context.RawHoneyProductions.Where(r =>
                r.OwnerOrgId == orgId
                || (r.OwnerOrgId == null && ws != null && r.RawHoneyProductionWholesalerId == ws)
                || _context.BeekeeperAssociations.Any(a => a.AssociationOrgId == orgId && a.ValidTo == null
                                                           && a.BeekeeperId == r.RawHoneyProductionApiary!.ApiaryBeekeeperId)
                || _context.FinalHoneyProductionDetails.Any(d => d.RawHoneyProductionId == r.RawHoneyProductionId && ownBatchIds.Contains(d.FinalHoneyProductionId)));
        }

        public async Task<IQueryable<FinalHoneyProduction>> VisibleBatchesAsync()
        {
            if (await ReadsAllAsync())
                return _context.FinalHoneyProductions;
            var active = await _current.GetAsync();
            return active == null ? _context.FinalHoneyProductions.Where(f => false) : await OwnedBatchesAsync(active.OrgId);
        }

        public async Task<IQueryable<Organization>> VisibleOrganizationsAsync()
        {
            if (await ReadsAllAsync())
                return _context.Organizations;
            var active = await _current.GetAsync();
            if (active == null)
                return _context.Organizations.Where(o => false);
            // A configured central association also lists the associations it verifies
            if (OrganizationAuthorizationHandler.IsCentralVerifier(_config, active))
                return _context.Organizations.Where(o => o.OrgId == active.OrgId || o.Capabilities.Any(c => c.Capability == Capabilities.Association));
            return _context.Organizations.Where(o => o.OrgId == active.OrgId);
        }
    }

    // ---- Organization resources ----

    public static class OrgOperations
    {
        public static readonly OperationAuthorizationRequirement Read = new() { Name = nameof(Read) };
        public static readonly OperationAuthorizationRequirement ManageDocuments = new() { Name = nameof(ManageDocuments) };
        public static readonly OperationAuthorizationRequirement Verify = new() { Name = nameof(Verify) };
        public static readonly OperationAuthorizationRequirement Configure = new() { Name = nameof(Configure) };
    }

    public class OrganizationAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Organization>
    {
        private readonly ICurrentOrg _current;
        private readonly OrgScope _scope;
        private readonly IConfiguration _config;

        public OrganizationAuthorizationHandler(ICurrentOrg current, OrgScope scope, IConfiguration config)
        {
            _current = current;
            _scope = scope;
            _config = config;
        }

        public static bool IsCentralVerifier(IConfiguration config, ActiveMembership active) =>
            string.Equals(config["OrgModel:AssociationVerifier"], Capabilities.CentralAssociation, StringComparison.OrdinalIgnoreCase)
            && active.Has(Capabilities.CentralAssociation) && active.IsVerified && active.OrgRole == OrgRoles.OrgAdmin;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Organization org)
        {
            if (_current.IsPlatformAdmin)
            {
                context.Succeed(requirement);
                return;
            }
            var active = await _current.GetAsync();
            if (active == null)
                return;

            var own = active.OrgId == org.OrgId;
            // Associations may be verified by a verified central association, if configured (ROLE-MODEL section 8)
            var centralVerifier = !own && IsCentralVerifier(_config, active) && org.Capabilities.Any(c => c.Capability == Capabilities.Association);
            switch (requirement.Name)
            {
                case nameof(OrgOperations.Read):
                    if (own || centralVerifier || await _scope.ReadsAllAsync())
                        context.Succeed(requirement);
                    break;
                case nameof(OrgOperations.ManageDocuments):
                    // A Pending org must be able to upload its documents to get verified
                    if (own && active.OrgRole == OrgRoles.OrgAdmin && active.OrgStatus != OrgStatus.Suspended)
                        context.Succeed(requirement);
                    break;
                case nameof(OrgOperations.Verify):
                    if (centralVerifier)
                        context.Succeed(requirement);
                    break;
                // Configure (capabilities, members, four-eyes exemption): platform administrators only
            }
        }
    }

    // ---- CCP records of a lot or batch ----

    public record CcpResource(string OwnerKind, int OwnerId, int CcpId);

    public static class CcpOperations
    {
        public static readonly OperationAuthorizationRequirement Read = new() { Name = nameof(Read) };
        public static readonly OperationAuthorizationRequirement Enter = new() { Name = nameof(Enter) };
        public static readonly OperationAuthorizationRequirement Approve = new() { Name = nameof(Approve) };
    }

    // Thin wrapper around CcpResponsibilityService (P2 rules); adds four-eyes on approvals: the approver
    // must not be one of the people who recorded the values, unless the organization has the exemption
    // set by a platform administrator (D-27). Lab results never get the exemption (P4).
    public class CcpAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, CcpResource>
    {
        public const string SelfApprovalKey = "CcpSelfApproval";

        private readonly ICurrentOrg _current;
        private readonly CcpResponsibilityService _resp;
        private readonly TOP4HoneyChainsContext _context;
        private readonly IHttpContextAccessor _http;

        public CcpAuthorizationHandler(ICurrentOrg current, CcpResponsibilityService resp, TOP4HoneyChainsContext context, IHttpContextAccessor http)
        {
            _current = current;
            _resp = resp;
            _context = context;
            _http = http;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, CcpResource r)
        {
            var access = await _resp.AccessAsync(r.OwnerKind, r.OwnerId, r.CcpId);
            switch (requirement.Name)
            {
                case nameof(CcpOperations.Read):
                    if (access.Read)
                        context.Succeed(requirement);
                    break;

                case nameof(CcpOperations.Enter):
                    if (access.Enter)
                        context.Succeed(requirement);
                    else if (access.Read && access.EnterDenied != null)
                        context.Fail(new AuthorizationFailureReason(this, access.EnterDenied));
                    break;

                case nameof(CcpOperations.Approve):
                    if (!access.Approve)
                    {
                        if (access.Read && access.ApproveDenied != null)
                            context.Fail(new AuthorizationFailureReason(this, access.ApproveDenied));
                        break;
                    }
                    var active = await _current.GetAsync();
                    var self = await CcpRecorders.OfAsync(_context, r.OwnerKind, r.OwnerId, r.CcpId, _current.UserId, _current.UserName);
                    if (self == CcpRecorders.Self.None)
                        context.Succeed(requirement);
                    else if (active != null && active.FourEyesExempt)
                    {
                        // Allowed under the exemption; flagged on the record and in the audit log (D-27)
                        _http.HttpContext!.Items[SelfApprovalKey] = true;
                        context.Succeed(requirement);
                    }
                    else
                        context.Fail(new AuthorizationFailureReason(this, self == CcpRecorders.Self.NotApplicable
                            ? "Four-eyes: you marked this CCP \"Not applicable\", so another person must approve it."
                            : "Four-eyes: you recorded values of this CCP, so another person must approve it."));
                    break;
            }
        }
    }

    // Who recorded a CCP record (four-eyes, D-27): any user who recorded one of its values, and (D-75) the user who marked
    // it "Not applicable" (stored by user name). Used by the approve rule and by My work, so My work offers only what
    // the rule allows.
    public static class CcpRecorders
    {
        public enum Self { None, Values, NotApplicable }

        public static async Task<Self> OfAsync(TOP4HoneyChainsContext context, string ownerKind, int ownerId, int ccpId, string? userId, string? userName)
        {
            var raw = ownerKind == "raw";
            if (userId != null && await (raw
                    ? context.CriticalControlPointMonitoringLogs.Where(l => l.RawHoneyProductionId == ownerId && l.CCPId == ccpId)
                    : context.CriticalControlPointMonitoringLogs.Where(l => l.FinalHoneyProductionId == ownerId && l.CCPId == ccpId))
                .AnyAsync(l => l.RecordedByUserId == userId))
                return Self.Values;
            if (string.IsNullOrEmpty(userName))
                return Self.None;
            var markedNotApplicable = raw
                ? await context.RawHoneyCCPAssignments.AnyAsync(a => a.RawHoneyProductionId == ownerId && a.RawHoneyCCPAssignmentCCP == ccpId
                    && a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.NotApplicable && a.NotApplicableBy == userName)
                : await context.FinalHoneyCCPAssignments.AnyAsync(a => a.FinalHoneyProductionId == ownerId && a.FinalHoneyCCPAssignmentCCP == ccpId
                    && a.FinalHoneyCCPAssignmentStatus == AssignmentStatusIds.NotApplicable && a.NotApplicableBy == userName);
            return markedNotApplicable ? Self.NotApplicable : Self.None;
        }
    }
}
