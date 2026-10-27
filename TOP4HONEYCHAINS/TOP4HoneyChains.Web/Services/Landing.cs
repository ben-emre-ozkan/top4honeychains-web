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

    // Landing images live in wwwroot/img/landing as <name>.jpg; scripts/landing-images.py adds <name>-<width>.avif and
    // .webp (D-70). Until the JPEG exists the page shows a patterned placeholder.
    public record LandingImage(string Name, string? Jpeg, IReadOnlyList<(string Url, int Width)> WebP, IReadOnlyList<(string Url, int Width)>? Avif = null)
    {
        public bool Exists => Jpeg != null;
        public string WebPSrcSet => string.Join(", ", WebP.Select(w => $"{w.Url} {w.Width}w"));
        public string AvifSrcSet => string.Join(", ", (Avif ?? Array.Empty<(string, int)>()).Select(w => $"{w.Item1} {w.Item2}w"));
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
            var avif = Widths.Where(w => files.GetFileInfo($"{Folder}/{name}-{w}.avif").Exists)
                .Select(w => ($"/{Folder}/{name}-{w}.avif", w)).ToList();
            return jpeg == null
                ? new LandingImage(name, null, Array.Empty<(string, int)>(), Array.Empty<(string, int)>())
                : new LandingImage(name, jpeg, webp, avif);
        }

        // D-78 P4 (D78-SPEC 4.11, D-77): the consortium partners in the README order (coordinator first). A file named
        // "NN-..." in wwwroot/img/partners takes the name of partner NN; any other file its own name.
        public static readonly (string Name, string Country)[] Partners =
        {
            ("Boğaziçi University", "Türkiye"), ("Işık University", "Türkiye"), ("Apiculture Research Institute", "Türkiye"),
            ("Instituto Nacional de Tecnología Agropecuaria (INTA)", "Argentina"), ("NEXCO S.A.", "Argentina"),
            ("Coop. Agrop. y Apícola Norte Grande", "Argentina"), ("Alimentos Naturales Natural Foods S.A.", "Argentina"),
            ("University of Economics in Katowice", "Poland"), ("Latvia University of Life Sciences and Technologies", "Latvia")
        };

        public record Logo(string Url, string Name, string? Country);

        private static readonly string[] LogoTypes = { ".svg", ".png", ".webp", ".jpg" };

        private static IEnumerable<Microsoft.Extensions.FileProviders.IFileInfo> Images(IWebHostEnvironment env, string folder)
        {
            var dir = env.WebRootFileProvider.GetDirectoryContents(folder);
            return dir.Exists
                ? dir.Where(f => !f.IsDirectory && LogoTypes.Contains(Path.GetExtension(f.Name).ToLowerInvariant())).OrderBy(f => f.Name, StringComparer.Ordinal)
                : Enumerable.Empty<Microsoft.Extensions.FileProviders.IFileInfo>();
        }

        // "03-apiculture-research-institute.png" -> 3; "tubitak.png" -> null
        private static int? Number(string fileName) =>
            fileName.Length > 2 && char.IsDigit(fileName[0]) && char.IsDigit(fileName[1]) && fileName[2] == '-' ? int.Parse(fileName[..2]) : null;

        // "02-Agencia Nacional.png" -> "Agencia Nacional"
        public static string NameOf(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            if (Number(fileName) != null)
                name = name[3..];
            return name.Replace('_', ' ').Trim();
        }

        public static IReadOnlyList<Logo> PartnerLogos(IWebHostEnvironment env) =>
            Images(env, "img/partners").Select(f => Number(f.Name) is int n && n >= 1 && n <= Partners.Length
                ? new Logo("/img/partners/" + f.Name, Partners[n - 1].Name, Partners[n - 1].Country)
                : new Logo("/img/partners/" + f.Name, NameOf(f.Name), null)).ToList();

        // the funding agencies' logos, whatever files are in wwwroot/img/funders (named "NN-Agency name.png"); no code change needed
        public static IReadOnlyList<Logo> FunderLogos(IWebHostEnvironment env) =>
            Images(env, "img/funders").Select(f => new Logo("/img/funders/" + f.Name, NameOf(f.Name), null)).ToList();
    }
}
