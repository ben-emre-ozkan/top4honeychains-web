using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P1 (F5, D78-SPEC 4.2): lookups for the apiary place fields: the districts of a province, and the province under
    // the map pin (computed on the server from the province map, D-72). Panel users only.
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class PlacesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public PlacesController(TOP4HoneyChainsContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Districts(string? province)
        {
            var list = await _context.Districts.AsNoTracking().Where(d => d.ProvinceCode == province)
                .Select(d => new { id = d.DistrictId, name = d.Name }).ToListAsync();
            var turkish = StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), false);
            return Json(list.OrderBy(d => d.name, turkish));
        }

        [HttpGet]
        public IActionResult ProvinceAt(double? lat, double? lng)
        {
            var plate = TurkeyMap.PlateAt(lat, lng);
            return Json(new { code = plate is >= 1 and <= 81 ? plate.ToString("D2") : null });
        }
    }
}
