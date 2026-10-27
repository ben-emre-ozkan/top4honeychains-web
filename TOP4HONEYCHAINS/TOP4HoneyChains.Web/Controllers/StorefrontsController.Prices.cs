using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P3 (O-6, D-82): the association reads a member's asking prices only with the beekeeper's AssociationPrice consent
    public partial class StorefrontsController
    {
        public record PriceRow(RawHoneyProduction Lot, List<LotPrice> Prices);

        public async Task<IActionResult> Prices(string id)
        {
            var active = await _current.GetAsync();
            if (_current.IsPlatformAdmin || active == null || !active.Has(Capabilities.Association)
                || !Perm.Allows(PermissionAreas.Storefront, PermissionAreas.Read, active.OrgRole)
                || !await HttpContext.RequestServices.GetRequiredService<LotPriceReader>().AssociationMayReadAsync(id, active.OrgId))
                return NotFound();
            var beekeeper = await _context.Beekeepers.AsNoTracking().FirstAsync(b => b.BeekeeperId == id);
            var lots = await _context.RawHoneyProductions.AsNoTracking().Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Where(r => r.RawHoneyProductionApiary!.ApiaryBeekeeperId == id && _context.LotPrices.Any(p => p.RawHoneyProductionId == r.RawHoneyProductionId))
                .OrderByDescending(r => r.RawHoneyProductionDate).ToListAsync();
            var ids = lots.Select(l => l.RawHoneyProductionId).ToList();
            var prices = await _context.LotPrices.AsNoTracking().Where(p => ids.Contains(p.RawHoneyProductionId)).OrderByDescending(p => p.PriceId).ToListAsync();
            ViewBag.Beekeeper = beekeeper;
            ViewBag.Controller = "Storefronts";
            ViewBag.Action = "Prices";
            return View(lots.Select(l => new PriceRow(l, prices.Where(p => p.RawHoneyProductionId == l.RawHoneyProductionId).ToList())).ToList());
        }

        private async Task<HashSet<string>> PriceConsentsAsync(int associationOrgId, List<string> beekeeperIds) =>
            (await _context.Consents.AsNoTracking()
                .Where(c => beekeeperIds.Contains(c.BeekeeperId) && c.Purpose == ConsentPurpose.AssociationPrice && c.WithdrawnAt == null
                            && (c.AssociationOrgId == null || c.AssociationOrgId == associationOrgId))
                .Select(c => c.BeekeeperId).ToListAsync()).ToHashSet();
    }
}
