using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    public record CcpConfig(int CcpId, string OwnerRule, bool RequiresApproval, IReadOnlySet<string> Capabilities,
        IReadOnlySet<string> EnterRoles, IReadOnlySet<string> ApproveRoles);

    public record CcpAccess(bool Read, bool Enter, bool Approve, string? EnterDenied, string? ApproveDenied);

    // The CCP responsibility matrix (D-25) and the access rules built on it (P2):
    //   * a CCP of a lot/batch belongs to the organization named by the matrix: the owning org of the
    //     item (D-36) or, for CCP 0, the association of the lot's beekeeper;
    //   * that org enters it when it is Verified, has one of the CCP's capabilities and the user has an
    //     entering role; another person with an approving role approves it (D-27);
    //   * an org reads its own CCPs, the earlier CCPs of the same item and every CCP of the lots in the
    //     batches it owns (upstream); it never sees items it has nothing to do with;
    //   * platform administrators read everything and change nothing (D-35): corrections go through
    //     correction records.
    // Read per request, so changes to the matrix act at once.
    public class CcpResponsibilityService
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private Dictionary<int, CcpConfig>? _configs;

        public CcpResponsibilityService(TOP4HoneyChainsContext context, ICurrentOrg current)
        {
            _context = context;
            _current = current;
        }

        public async Task<Dictionary<int, CcpConfig>> ConfigsAsync()
        {
            if (_configs != null)
                return _configs;
            var rows = await _context.CcpResponsibilities.AsNoTracking()
                .Include(r => r.Capabilities).Include(r => r.Roles).ToListAsync();
            _configs = rows.ToDictionary(r => r.CcpId, r => new CcpConfig(r.CcpId, r.OwnerRule, r.RequiresApproval,
                r.Capabilities.Select(c => c.Capability).ToHashSet(),
                r.Roles.Where(x => x.Action == CcpActions.Enter).Select(x => x.OrgRole).ToHashSet(),
                r.Roles.Where(x => x.Action == CcpActions.Approve).Select(x => x.OrgRole).ToHashSet()));
            return _configs;
        }

        public async Task<bool> RequiresApprovalAsync(int ccpId) =>
            !(await ConfigsAsync()).TryGetValue(ccpId, out var c) || c.RequiresApproval;

        // ---- owners (D-36): the owning org, or the org created from the wholesaler while none is set ----

        public Task<int?> LotOwnerAsync(int rawId) =>
            _context.RawHoneyProductions.Where(r => r.RawHoneyProductionId == rawId)
                .Select(r => r.OwnerOrgId ?? _context.Organizations.Where(o => o.LegacyWholesalerId == r.RawHoneyProductionWholesalerId).Select(o => (int?)o.OrgId).FirstOrDefault())
                .FirstOrDefaultAsync();

        public Task<int?> BatchOwnerAsync(int batchId) =>
            _context.FinalHoneyProductions.Where(f => f.FinalHoneyProductionId == batchId)
                .Select(f => f.OwnerOrgId ?? _context.Organizations.Where(o => o.LegacyWholesalerId == f.FinalHoneyProductionWholesalerId).Select(o => (int?)o.OrgId).FirstOrDefault())
                .FirstOrDefaultAsync();

        private Task<List<int>> LotAssociationsAsync(int rawId) =>
            _context.RawHoneyProductions.Where(r => r.RawHoneyProductionId == rawId)
                .SelectMany(r => _context.BeekeeperAssociations
                    .Where(a => a.ValidTo == null && a.BeekeeperId == r.RawHoneyProductionApiary!.ApiaryBeekeeperId)
                    .Select(a => a.AssociationOrgId))
                .ToListAsync();

        // Organizations responsible for a CCP of a lot ("raw") or batch ("batch")
        public async Task<List<int>> ResponsibleOrgsAsync(string kind, int ownerId, int ccpId)
        {
            if (!(await ConfigsAsync()).TryGetValue(ccpId, out var cfg))
                return new();
            if (cfg.OwnerRule == CcpOwnerRules.BeekeeperAssociation)
                return kind == "raw" ? await LotAssociationsAsync(ownerId) : new();
            var owner = kind == "raw" ? await LotOwnerAsync(ownerId) : await BatchOwnerAsync(ownerId);
            return owner == null ? new() : new() { owner.Value };
        }

        private Task<List<int>> AssignedCcpIdsAsync(string kind, int ownerId) => kind == "raw"
            ? _context.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == ownerId).Select(a => a.RawHoneyCCPAssignmentCCP).ToListAsync()
            : _context.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == ownerId).Select(a => a.FinalHoneyCCPAssignmentCCP).ToListAsync();

        // The lot is in a batch owned by the org: the whole lot is upstream of its item
        private async Task<bool> LotInOwnedBatchAsync(int rawId, int orgId)
        {
            var batchIds = await _context.FinalHoneyProductionDetails.Where(d => d.RawHoneyProductionId == rawId)
                .Select(d => d.FinalHoneyProductionId).Distinct().ToListAsync();
            foreach (var b in batchIds)
                if (await BatchOwnerAsync(b) == orgId)
                    return true;
            return false;
        }

        // ---- access ----

        public async Task<CcpAccess> AccessAsync(string kind, int ownerId, int ccpId)
        {
            if (!_current.Enabled)
            {
                // v1.0: administrators read and enter everything; there is no approval step
                var admin = _current.IsPlatformAdmin;
                return new CcpAccess(admin, admin, false, admin ? null : "No access.", "Approvals need the organization model.");
            }

            if (_current.IsPlatformAdmin)
                return new CcpAccess(true, false, false,
                    "Platform administrators are read-only on business data; request a correction instead.",
                    "Platform administrators do not approve CCP records.");

            var active = await _current.GetAsync();
            if (active == null)
                return new CcpAccess(false, false, false, "No active membership.", "No active membership.");

            var configs = await ConfigsAsync();
            configs.TryGetValue(ccpId, out var cfg);
            var responsible = (await ResponsibleOrgsAsync(kind, ownerId, ccpId)).Contains(active.OrgId);

            var read = responsible || (active.Has(Capabilities.PublicAuthority) && active.IsVerified);
            if (!read)
            {
                // Earlier CCPs of an item the org works on
                foreach (var other in (await AssignedCcpIdsAsync(kind, ownerId)).Where(id => id > ccpId))
                    if ((await ResponsibleOrgsAsync(kind, ownerId, other)).Contains(active.OrgId)) { read = true; break; }
            }
            if (!read && kind == "raw")
                read = await LotInOwnedBatchAsync(ownerId, active.OrgId);
            // Later holders of a batch read its records (D-26, D-41)
            if (!read && kind == "batch")
                read = await _context.Shipments.AnyAsync(s => s.FinalHoneyProductionId == ownerId && s.ToOrgId == active.OrgId && s.Status == ShipmentStatus.Received);

            string? enterDenied = null, approveDenied = null;
            if (!responsible || cfg == null)
                enterDenied = approveDenied = "This control point is the responsibility of another organization; you can read it only.";
            else if (!active.IsVerified)
                enterDenied = approveDenied = $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            else if (!cfg.Capabilities.Any(active.Has))
                enterDenied = approveDenied = $"This control point needs one of these capabilities: {string.Join(", ", cfg.Capabilities)}.";
            else
            {
                if (!cfg.EnterRoles.Contains(active.OrgRole))
                    enterDenied = $"Your role ({active.OrgRole}) cannot enter this control point; entering roles: {string.Join(", ", cfg.EnterRoles)}.";
                if (!cfg.ApproveRoles.Contains(active.OrgRole))
                    approveDenied = $"Your role ({active.OrgRole}) cannot approve this control point; approving roles: {string.Join(", ", cfg.ApproveRoles)}.";
            }
            return new CcpAccess(read, read && enterDenied == null, read && approveDenied == null, enterDenied, approveDenied);
        }

        // CCP ids of an item the current user may open (for lists)
        public async Task<HashSet<int>> ReadableCcpIdsAsync(string kind, int ownerId, IEnumerable<int> ccpIds)
        {
            var set = new HashSet<int>();
            foreach (var id in ccpIds.Distinct())
                if ((await AccessAsync(kind, ownerId, id)).Read)
                    set.Add(id);
            return set;
        }
    }
}
