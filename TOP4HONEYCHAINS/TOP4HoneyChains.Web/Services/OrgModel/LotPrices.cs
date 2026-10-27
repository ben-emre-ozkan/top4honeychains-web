using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P3 (D78-SPEC 2.3 note 23, 3.8, O-6, O-7, D-82): who reads a beekeeper's asking price. The organization that owns the lot
    // (roles with Price Read: OrgAdmin and ComplianceOfficer by default) and platform administrators; the beekeeper's association
    // only with the beekeeper's AssociationPrice consent. Staff, laboratories and later holders never; no public page shows it.
    public class LotPriceReader
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly Permissions _perm;
        private readonly CcpResponsibilityService _resp;

        public LotPriceReader(TOP4HoneyChainsContext context, ICurrentOrg current, Permissions perm, CcpResponsibilityService resp)
        {
            _context = context;
            _current = current;
            _perm = perm;
            _resp = resp;
        }

        // the active membership may read the prices of the lots its organization owns
        public async Task<bool> RoleMayReadAsync() =>
            _current.IsPlatformAdmin || (await _current.GetAsync() is { } m && _perm.Allows(PermissionAreas.Price, PermissionAreas.Read, m.OrgRole));

        public async Task<bool> MayReadAsync(int lotId)
        {
            if (_current.IsPlatformAdmin)
                return true;
            var m = await _current.GetAsync();
            return m != null && _perm.Allows(PermissionAreas.Price, PermissionAreas.Read, m.OrgRole) && await _resp.LotOwnerAsync(lotId) == m.OrgId;
        }

        public async Task<Dictionary<int, LotPrice>> CurrentAsync(IEnumerable<int> lotIds)
        {
            var ids = lotIds.Distinct().ToList();
            var allowed = new List<int>();
            foreach (var id in ids)
                if (await MayReadAsync(id))
                    allowed.Add(id);
            return (await _context.LotPrices.AsNoTracking().Where(p => allowed.Contains(p.RawHoneyProductionId)).ToListAsync())
                .GroupBy(p => p.RawHoneyProductionId).ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.PriceId).First());
        }

        public async Task<List<LotPrice>> HistoryAsync(int lotId) => await MayReadAsync(lotId)
            ? await _context.LotPrices.AsNoTracking().Where(p => p.RawHoneyProductionId == lotId).OrderByDescending(p => p.PriceId).ToListAsync()
            : new();

        // the association of the beekeeper, with the beekeeper's consent
        public async Task<bool> AssociationMayReadAsync(string beekeeperId, int associationOrgId) =>
            await _context.BeekeeperAssociations.AnyAsync(a => a.BeekeeperId == beekeeperId && a.AssociationOrgId == associationOrgId && a.ValidTo == null)
            && await _context.Consents.AnyAsync(c => c.BeekeeperId == beekeeperId && c.Purpose == ConsentPurpose.AssociationPrice && c.WithdrawnAt == null
                                                     && (c.AssociationOrgId == null || c.AssociationOrgId == associationOrgId));
    }
}
