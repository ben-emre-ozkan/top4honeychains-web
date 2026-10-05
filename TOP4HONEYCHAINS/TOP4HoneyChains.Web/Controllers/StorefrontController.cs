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
    // Public beekeeper storefront (D-30, D-43): only verified data, only with an active consent. The page
    // (design docs/design/beekeeper-page) is built by StorefrontPage. Never a phone, personal photo, birth date
    // or exact location; contact goes through the association.
    // Public pages (D-47)
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [RequireOrgModel]
    [Route("storefront")]
    public class StorefrontController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly StorefrontService _storefronts;
        private readonly StorefrontPage _page;

        public StorefrontController(TOP4HoneyChainsContext context, StorefrontService storefronts, StorefrontPage page)
        {
            _context = context;
            _storefronts = storefronts;
            _page = page;
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
            return View(await _page.BuildAsync(state, bk));
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
