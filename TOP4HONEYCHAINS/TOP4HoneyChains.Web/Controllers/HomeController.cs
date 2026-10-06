using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Models.ViewModels;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Public pages (D-47)
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class HomeController : Controller
    {
        public static readonly string[] StepImages = { "step-1-apiary", "step-2-delivery", "step-3-lab", "step-4-processing", "step-5-shelf" };

        private readonly LandingStatsService _stats;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly FeatureFlags _flags;

        public HomeController(LandingStatsService stats, IWebHostEnvironment env, IConfiguration config, IOptions<FeatureFlags> flags)
        {
            _stats = stats;
            _env = env;
            _config = config;
            _flags = flags.Value;
        }

        // Landing page (D-65)
        public async Task<IActionResult> Index()
        {
            var demoQr = Guid.TryParse(_config["Landing:DemoHoneyStoryQr"], out var qr) ? qr : (Guid?)null;
            var stats = await _stats.GetAsync(demoQr);
            return View(new LandingViewModel
            {
                Stats = stats,
                OrgModel = _flags.OrgRoleModel,
                DemoQr = stats.DemoStoryAvailable ? demoQr : null,
                Steps = StepImages.Select(name => LandingImages.Find(_env, name)).ToList(),
                OgImage = LandingImages.Find(_env, "og-image"),
                PublicBaseUrl = PublicUrl.Build(HttpContext, ""),
                PartnerLogos = LandingImages.PartnerLogos(_env)
            });
        }

        public static readonly string[] Languages = { "tr", "en", "es", "pl", "lv" };

        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (!Languages.Contains(culture))
                culture = "tr";
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Path = "/",              // 🔴 ÖNEMLİ
                    IsEssential = true
                });

            // Only redirect within the site; an empty or external returnUrl goes to the start page
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                returnUrl = "/";

            return LocalRedirect(returnUrl);
        }

    }
}
