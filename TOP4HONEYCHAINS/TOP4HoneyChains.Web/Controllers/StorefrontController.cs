using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Public beekeeper storefront (D-30, D-43): only verified data, only with an active consent. Shows a
    // display name (or the neutral code), province/district, honey types, verified lots, approved lab
    // results, the batches with the beekeeper's honey and the association. Never a phone, photo, birth
    // date or exact location.
    [RequireOrgModel]
    [Route("storefront")]
    public class StorefrontController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly StorefrontService _storefronts;

        public StorefrontController(TOP4HoneyChainsContext context, StorefrontService storefronts)
        {
            _context = context;
            _storefronts = storefronts;
        }

        public record BatchLink(Guid Qr, string? Title, DateTime? Date);
        public record LabBadge(string Code, bool Accredited, string? AccreditationNumber, string Laboratory, bool InHouse);

        public class Page
        {
            public string Code { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public bool NameShown { get; set; }
            public string? Province { get; set; }
            public string? District { get; set; }
            public string? Association { get; set; }
            public List<string> HoneyTypes { get; set; } = new();
            public int VerifiedLots { get; set; }
            public List<LabBadge> LabReports { get; set; } = new();
            public List<BatchLink> Batches { get; set; } = new();
        }

        private async Task<(StorefrontService.State State, Beekeeper Beekeeper)?> PublishedAsync(string code)
        {
            code = code.Trim().ToUpperInvariant();
            var sf = await _context.BeekeeperStorefronts.AsNoTracking().FirstOrDefaultAsync(s => s.Code == code);
            if (sf == null)
                return null;
            var state = (await _storefronts.StatesAsync(new[] { sf.BeekeeperId })).GetValueOrDefault(sf.BeekeeperId);
            var bk = await _context.Beekeepers.AsNoTracking().FirstOrDefaultAsync(b => b.BeekeeperId == sf.BeekeeperId);
            return state is { Published: true } && bk != null ? (state, bk) : null;
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> Index(string code)
        {
            if (await PublishedAsync(code) is not var (state, bk))
                return NotFound();
            var registry = await _context.BeekeeperRegistry.AsNoTracking().FirstOrDefaultAsync(r => r.BeekeeperId == bk.BeekeeperId);

            // Verified lots: lots of the beekeeper used in a finalized (published) batch
            var lots = await _context.FinalHoneyProductionDetails.AsNoTracking()
                .Where(d => d.FinalHoneyProduction.FinalHoneyProductionIsFinalized
                            && d.RawHoneyProduction.RawHoneyProductionApiary!.ApiaryBeekeeperId == bk.BeekeeperId)
                .Select(d => new
                {
                    d.RawHoneyProductionId,
                    HoneyType = d.RawHoneyProduction.RawHoneyProductionHoneyTypeNavigation!.HoneyTypeTitle,
                    d.FinalHoneyProduction.FinalHoneyProductionQR,
                    d.FinalHoneyProduction.FinalHoneyProductionTitle,
                    d.FinalHoneyProduction.FinalHoneyProductionDate
                })
                .ToListAsync();
            var lotIds = lots.Select(l => l.RawHoneyProductionId).Distinct().ToList();
            var reports = await _context.LabReports.AsNoTracking()
                .Where(r => r.Status == LabReportStatus.Approved && lotIds.Contains(r.Sample!.RawHoneyProductionId) && r.LabOrg!.Status == OrgStatus.Verified)
                .Select(r => new LabBadge(r.ReportCode, r.IsAccredited, r.AccreditationNumber, r.LabOrg!.DisplayName, r.InHouse))
                .ToListAsync();

            var page = new Page
            {
                Code = state.Code!,
                DisplayName = StorefrontService.DisplayName(state.Consent!.NameConsented, bk.BeekeeperFirstName, bk.BeekeeperLastName, state.Code!),
                NameShown = state.Consent.NameConsented,
                // PQ-12 is open: the registry's own fields if set, else the province of the verified registry
                // number and the district recorded at intake of the beekeeper's verified lots
                Province = registry?.Province ?? Provinces.FromRegistryNumber(registry?.RegistryNumber),
                District = registry?.District ?? await _context.LotIntakes.AsNoTracking().Where(i => lotIds.Contains(i.RawHoneyProductionId))
                    .GroupBy(i => i.District).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefaultAsync(),
                Association = state.AssociationName,
                HoneyTypes = lots.Select(l => l.HoneyType).Where(t => t != null).Distinct().OrderBy(t => t).ToList()!,
                VerifiedLots = lotIds.Count,
                LabReports = reports,
                Batches = lots.GroupBy(l => l.FinalHoneyProductionQR)
                    .Select(g => new BatchLink(g.Key, g.First().FinalHoneyProductionTitle, g.First().FinalHoneyProductionDate))
                    .OrderByDescending(b => b.Date).ToList()
            };
            return View(page);
        }

        // The QR code of a published storefront (holds its full public address)
        [HttpGet("{code}/qr")]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> Qr(string code)
        {
            if (await PublishedAsync(code) is not var (state, _))
                return NotFound();
            var url = PublicUrl.Build(HttpContext, Url.Content("~/storefront/" + state.Code))
                ?? throw new InvalidOperationException("App:PublicBaseUrl is not configured; QR codes need the public site address.");
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qr = new QRCode(data);
            using var image = qr.GetGraphic(pixelsPerModule: 12, darkColor: Color.Black, lightColor: Color.White, drawQuietZones: true);
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return File(ms.ToArray(), "image/png");
        }
    }
}
