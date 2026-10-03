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

    // Which organization owns a lot or batch, and what the active membership may see (ROLE-MODEL section 14).
    // P1 has no custody columns yet: a lot or batch belongs to the organization created from its wholesaler
    // (Organizations.LegacyWholesalerId); existing rows are not modified.
    public class OrgScope
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;

        public OrgScope(TOP4HoneyChainsContext context, ICurrentOrg current)
        {
            _context = context;
            _current = current;
        }

        public Task<int?> LotOrgIdAsync(int rawHoneyProductionId) =>
            _context.RawHoneyProductions.Where(r => r.RawHoneyProductionId == rawHoneyProductionId)
                .Select(r => _context.Organizations.Where(o => o.LegacyWholesalerId == r.RawHoneyProductionWholesalerId).Select(o => (int?)o.OrgId).FirstOrDefault())
                .FirstOrDefaultAsync();

        public Task<int?> BatchOrgIdAsync(int finalHoneyProductionId) =>
            _context.FinalHoneyProductions.Where(f => f.FinalHoneyProductionId == finalHoneyProductionId)
                .Select(f => _context.Organizations.Where(o => o.LegacyWholesalerId == f.FinalHoneyProductionWholesalerId).Select(o => (int?)o.OrgId).FirstOrDefault())
                .FirstOrDefaultAsync();

        // Administrators and public authorities read everything
        public async Task<bool> ReadsAllAsync()
        {
            if (_current.IsPlatformAdmin)
                return true;
            var active = await _current.GetAsync();
            return active != null && active.Has(Capabilities.PublicAuthority) && active.IsVerified;
        }

        public async Task<IQueryable<RawHoneyProduction>> VisibleLotsAsync()
        {
            if (await ReadsAllAsync())
                return _context.RawHoneyProductions;
            var active = await _current.GetAsync();
            if (active == null)
                return _context.RawHoneyProductions.Where(r => false);
            var orgId = active.OrgId;
            var wholesalerId = await _context.Organizations.Where(o => o.OrgId == orgId).Select(o => o.LegacyWholesalerId).FirstOrDefaultAsync();
            // Own lots, plus (associations) the lots of their member beekeepers
            return _context.RawHoneyProductions.Where(r =>
                (wholesalerId != null && r.RawHoneyProductionWholesalerId == wholesalerId)
                || _context.BeekeeperAssociations.Any(a => a.AssociationOrgId == orgId && a.ValidTo == null
                                                           && a.BeekeeperId == r.RawHoneyProductionApiary!.ApiaryBeekeeperId));
        }

        public async Task<IQueryable<FinalHoneyProduction>> VisibleBatchesAsync()
        {
            if (await ReadsAllAsync())
                return _context.FinalHoneyProductions;
            var active = await _current.GetAsync();
            if (active == null)
                return _context.FinalHoneyProductions.Where(f => false);
            var wholesalerId = await _context.Organizations.Where(o => o.OrgId == active.OrgId).Select(o => o.LegacyWholesalerId).FirstOrDefaultAsync();
            return _context.FinalHoneyProductions.Where(f => wholesalerId != null && f.FinalHoneyProductionWholesalerId == wholesalerId);
        }

        public async Task<IQueryable<Organization>> VisibleOrganizationsAsync()
        {
            if (await ReadsAllAsync())
                return _context.Organizations;
            var active = await _current.GetAsync();
            return active == null
                ? _context.Organizations.Where(o => false)
                : _context.Organizations.Where(o => o.OrgId == active.OrgId);
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
            switch (requirement.Name)
            {
                case nameof(OrgOperations.Read):
                    if (own || await _scope.ReadsAllAsync())
                        context.Succeed(requirement);
                    break;
                case nameof(OrgOperations.ManageDocuments):
                    // A Pending org must be able to upload its documents to get verified
                    if (own && active.OrgRole == OrgRoles.OrgAdmin && active.OrgStatus != OrgStatus.Suspended)
                        context.Succeed(requirement);
                    break;
                case nameof(OrgOperations.Verify):
                    // Associations may be verified by a verified central association, if configured (ROLE-MODEL section 8)
                    var byCentral = string.Equals(_config["OrgModel:AssociationVerifier"], Capabilities.CentralAssociation, StringComparison.OrdinalIgnoreCase);
                    if (byCentral && !own && org.Capabilities.Any(c => c.Capability == Capabilities.Association)
                        && active.Has(Capabilities.CentralAssociation) && active.IsVerified && active.OrgRole == OrgRoles.OrgAdmin)
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

    // Read: the owning org (or admin / public authority). Enter: a Verified owning org and an entering role.
    // Approve: a Verified owning org, the QualityApprover role and four-eyes (not one of the recorders),
    // unless the org has the PlatformAdmin-set exemption (D-27). Administrators keep v1.0 access to read
    // and enter; their approvals follow four-eyes too.
    public class CcpAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, CcpResource>
    {
        public const string SelfApprovalKey = "CcpSelfApproval";

        private readonly ICurrentOrg _current;
        private readonly OrgScope _scope;
        private readonly TOP4HoneyChainsContext _context;
        private readonly IHttpContextAccessor _http;

        public CcpAuthorizationHandler(ICurrentOrg current, OrgScope scope, TOP4HoneyChainsContext context, IHttpContextAccessor http)
        {
            _current = current;
            _scope = scope;
            _context = context;
            _http = http;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, CcpResource r)
        {
            var admin = _current.IsPlatformAdmin;
            var active = await _current.GetAsync();
            var ownerOrg = r.OwnerKind == "raw" ? await _scope.LotOrgIdAsync(r.OwnerId) : await _scope.BatchOrgIdAsync(r.OwnerId);
            var own = active != null && ownerOrg != null && active.OrgId == ownerOrg;

            switch (requirement.Name)
            {
                case nameof(CcpOperations.Read):
                    if (admin || own || await _scope.ReadsAllAsync() || (r.OwnerKind == "raw" && active != null && await IsAssociationOfLotAsync(active.OrgId, r.OwnerId)))
                        context.Succeed(requirement);
                    break;

                case nameof(CcpOperations.Enter):
                    if (admin)
                        context.Succeed(requirement);
                    else if (own && active!.IsVerified && OrgRoles.CcpEnter.Contains(active.OrgRole))
                        context.Succeed(requirement);
                    else if (active != null && !active.IsVerified)
                        context.Fail(new AuthorizationFailureReason(this, $"Your organization is {active.OrgStatus}; only verified organizations can enter data."));
                    break;

                case nameof(CcpOperations.Approve):
                    if (!admin && !(own && active!.IsVerified && OrgRoles.CcpApprove.Contains(active.OrgRole)))
                    {
                        if (active != null && !active.IsVerified)
                            context.Fail(new AuthorizationFailureReason(this, $"Your organization is {active.OrgStatus}; only verified organizations can approve."));
                        break;
                    }
                    var recorders = await RecordersAsync(r);
                    var self = _current.UserId != null && recorders.Contains(_current.UserId);
                    if (!self)
                    {
                        context.Succeed(requirement);
                    }
                    else if (!admin && active!.FourEyesExempt)
                    {
                        // Allowed under the exemption; the action is flagged in the audit log (D-27).
                        // P1 has no lab results yet; P4 must refuse the exemption for them.
                        _http.HttpContext!.Items[SelfApprovalKey] = true;
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail(new AuthorizationFailureReason(this, "Four-eyes: you recorded values of this CCP, so another person must approve it."));
                    }
                    break;
            }
        }

        private Task<bool> IsAssociationOfLotAsync(int orgId, int rawId) =>
            _context.RawHoneyProductions.AnyAsync(l => l.RawHoneyProductionId == rawId
                && _context.BeekeeperAssociations.Any(a => a.AssociationOrgId == orgId && a.ValidTo == null
                                                          && a.BeekeeperId == l.RawHoneyProductionApiary!.ApiaryBeekeeperId));

        private Task<List<string>> RecordersAsync(CcpResource r) =>
            (r.OwnerKind == "raw"
                ? _context.CriticalControlPointMonitoringLogs.Where(l => l.RawHoneyProductionId == r.OwnerId && l.CCPId == r.CcpId)
                : _context.CriticalControlPointMonitoringLogs.Where(l => l.FinalHoneyProductionId == r.OwnerId && l.CCPId == r.CcpId))
            .Where(l => l.RecordedByUserId != null)
            .Select(l => l.RecordedByUserId!)
            .Distinct()
            .ToListAsync();
    }
}
