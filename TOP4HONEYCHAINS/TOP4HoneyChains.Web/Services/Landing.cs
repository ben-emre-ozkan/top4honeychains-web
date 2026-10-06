using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Services
{
    // Public landing page (D-65): aggregate counters only, no personal data; cached for 10 minutes
    public record LandingStats(int FinalizedBatches, int VerifiedOrganizations, int ApprovedLabReports, bool DemoStoryAvailable)
    {
        public bool Any => FinalizedBatches > 0 || VerifiedOrganizations > 0 || ApprovedLabReports > 0;
        public static readonly LandingStats Empty = new(0, 0, 0, false);
    }

    public class LandingStatsService
    {
        public static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(10);
        private const string CacheKey = "landing-stats";

        private readonly TOP4HoneyChainsContext _context;
        private readonly IMemoryCache _cache;
        private readonly FeatureFlags _flags;

        public LandingStatsService(TOP4HoneyChainsContext context, IMemoryCache cache, IOptions<FeatureFlags> flags)
        {
            _context = context;
            _cache = cache;
            _flags = flags.Value;
        }

        public async Task<LandingStats> GetAsync(Guid? demoQr)
        {
            // the organization tables are used only with the organization model
            if (!_flags.OrgRoleModel)
                return LandingStats.Empty;
            return (await _cache.GetOrCreateAsync(CacheKey + demoQr, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheFor;
                return new LandingStats(
                    await _context.FinalHoneyProductions.CountAsync(f => f.FinalHoneyProductionIsFinalized),
                    await _context.Organizations.CountAsync(o => o.Status == OrgStatus.Verified),
                    await _context.LabReports.CountAsync(r => r.Status == LabReportStatus.Approved),
                    demoQr != null && await _context.FinalHoneyProductions.AnyAsync(f => f.FinalHoneyProductionQR == demoQr && f.FinalHoneyProductionIsFinalized));
            }))!;
        }
    }

    // Landing images live in wwwroot/img/landing as <name>.jpg; scripts/landing-images.py adds <name>-<width>.webp.
    // Until the JPEG exists the page shows a patterned placeholder.
    public record LandingImage(string Name, string? Jpeg, IReadOnlyList<(string Url, int Width)> WebP)
    {
        public bool Exists => Jpeg != null;
        public string WebPSrcSet => string.Join(", ", WebP.Select(w => $"{w.Url} {w.Width}w"));
    }

    public static class LandingImages
    {
        public const string Folder = "img/landing";
        public static readonly int[] Widths = { 480, 960, 1440 };

        public static LandingImage Find(IWebHostEnvironment env, string name)
        {
            var files = env.WebRootFileProvider;
            var jpeg = files.GetFileInfo($"{Folder}/{name}.jpg").Exists ? $"/{Folder}/{name}.jpg" : null;
            var webp = Widths.Where(w => files.GetFileInfo($"{Folder}/{name}-{w}.webp").Exists)
                .Select(w => ($"/{Folder}/{name}-{w}.webp", w)).ToList();
            return new LandingImage(name, jpeg, jpeg == null ? Array.Empty<(string, int)>() : webp);
        }

        // partner logos: any image in wwwroot/img/partners (empty for now)
        public static IReadOnlyList<string> PartnerLogos(IWebHostEnvironment env)
        {
            var dir = env.WebRootFileProvider.GetDirectoryContents("img/partners");
            if (!dir.Exists)
                return Array.Empty<string>();
            return dir.Where(f => !f.IsDirectory && new[] { ".svg", ".png", ".webp", ".jpg" }.Contains(Path.GetExtension(f.Name).ToLowerInvariant()))
                .OrderBy(f => f.Name).Select(f => "/img/partners/" + f.Name).ToList();
        }
    }
}
