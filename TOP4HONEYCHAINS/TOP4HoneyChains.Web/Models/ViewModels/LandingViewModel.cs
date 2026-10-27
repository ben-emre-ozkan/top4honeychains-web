using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // Public landing page (D-65)
    public class LandingViewModel
    {
        public LandingStats Stats { get; set; } = LandingStats.Empty;
        public bool OrgModel { get; set; }
        public Guid? DemoQr { get; set; }
        public IReadOnlyList<LandingImage> Steps { get; set; } = Array.Empty<LandingImage>();
        public LandingImage? OgImage { get; set; }
        public string? PublicBaseUrl { get; set; }
        public IReadOnlyList<TOP4HoneyChains.Web.Services.LandingImages.Logo> PartnerLogos { get; set; } = Array.Empty<TOP4HoneyChains.Web.Services.LandingImages.Logo>();
        public IReadOnlyList<TOP4HoneyChains.Web.Services.LandingImages.Logo> FunderLogos { get; set; } = Array.Empty<TOP4HoneyChains.Web.Services.LandingImages.Logo>();
    }
}
