using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-97 (migration 033): the seal pool of an organization and its permanent containers.
    // - Seal labels are issued by the platform in numbered batches (TS-nnnnnn, unique on the platform) and printed in advance on
    //   tamper-evident VOID label stock (A4 sheet or 100 x 50 mm labels, the existing label printing).
    // - Purchased numbered plastic seals are added as a number range (prefix + numbers).
    // - Every seal typed or scanned at a delivery must come from the pool and be unused; a code is used once (Issued -> Used ->
    //   Broken). A USB scanner types the QR content into the field: a scanned web address is reduced to its last part.
    public class SealPoolService
    {
        public const string LabelPrefix = "TS-";
        public const string ContainerPrefix = "TK-";
        // D-112: the seal labels, plastic seals and containers of the test set's organizations start with TEST- and have their own
        // numbering, so they never use up or look like a real code
        public const string TestPrefix = "TEST-";
        public const int MaxBatch = 500;
        public const int MaxRange = 1000;
        private static readonly Regex PrefixPattern = new("^[A-Z0-9][A-Z0-9-]{0,19}$");

        private readonly TOP4HoneyChainsContext _context;
        private readonly TimeProvider _clock;

        public SealPoolService(TOP4HoneyChainsContext context, TimeProvider clock)
        {
            _context = context;
            _clock = clock;
        }

        private DateTime Now => _clock.GetUtcNow().UtcDateTime;

        // what a person typed or a scanner sent, as a code: trimmed, upper case, no spaces; "https://…/s/TS-000123" -> "TS-000123"
        // (D-98: the printed QR holds the seal's short address, so every code field accepts it)
        public static string? Normalize(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            var s = raw.Trim();
            if (s.Contains("://", StringComparison.Ordinal) && s.IndexOfAny(new[] { '?', '#' }) is var cut and > 0)
                s = s[..cut];
            var slash = s.TrimEnd('/').LastIndexOfAny(new[] { '/', '=' });
            if (slash >= 0)
                s = s.TrimEnd('/')[(slash + 1)..];
            s = Regex.Replace(s, @"\s+", "").ToUpperInvariant();
            return s.Length == 0 ? null : s;
        }

        // D-98: the short address of a seal or a permanent container, printed in its QR code (/s/{code}). Absolute: App:PublicBaseUrl
        // when set, else the address the page was opened with (the print page is only for signed-in members)
        public const string ShortPrefix = "/s/";

        public static string ShortPath(string code) => ShortPrefix + Uri.EscapeDataString(code);

        public static string ShortUrl(HttpContext http, string code)
        {
            var path = http.Request.PathBase + ShortPath(code);
            return PublicUrl.Build(http, path) ?? $"{http.Request.Scheme}://{http.Request.Host}{path}";
        }

        // ---------------------------------------------------------------- the pool

        private Task<bool> IsTestOrgAsync(int orgId) => _context.Organizations.AnyAsync(o => o.OrgId == orgId && o.IsTestData);

        public async Task<SealBatch> IssueLabelsAsync(int orgId, int count, string userId, string? note)
        {
            count = Math.Clamp(count, 1, MaxBatch);
            var prefix = await IsTestOrgAsync(orgId) ? TestPrefix + LabelPrefix : LabelPrefix;
            var last = await _context.SealCodes.Where(c => c.Code.StartsWith(prefix)).Select(c => c.Code).OrderByDescending(c => c).FirstOrDefaultAsync();
            var next = last != null && int.TryParse(last[prefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n + 1 : 1;
            var codes = Enumerable.Range(next, count).Select(i => prefix + i.ToString("D6", CultureInfo.InvariantCulture)).ToList();
            return await AddBatchAsync(orgId, SealKind.Label, codes, userId, note);
        }

        public async Task<(SealBatch? Batch, string? Error)> AddPlasticRangeAsync(int orgId, string? prefix, string? from, string? to, string userId, string? note)
        {
            prefix = (prefix ?? "").Trim().ToUpperInvariant();
            from = from?.Trim();
            to = to?.Trim();
            if (prefix.Length > 0 && !PrefixPattern.IsMatch(prefix))
                return (null, "The prefix has at most 20 letters, digits or hyphens.");
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || !from.All(char.IsAsciiDigit) || !to.All(char.IsAsciiDigit) || from.Length > 12 || to.Length > 12)
                return (null, "Enter the first and the last number of the seals (digits only).");
            var a = long.Parse(from, CultureInfo.InvariantCulture);
            var b = long.Parse(to, CultureInfo.InvariantCulture);
            if (b < a)
                return (null, "The last number cannot be lower than the first.");
            if (b - a + 1 > MaxRange)
                return (null, "At most 1000 seals per range.");
            var width = Math.Max(from.Length, to.Length);
            // D-112: a test organization's seals get the TEST- prefix; a real organization cannot use it
            var test = await IsTestOrgAsync(orgId);
            if (test && !prefix.StartsWith(TestPrefix, StringComparison.Ordinal))
                prefix = TestPrefix + prefix;
            else if (!test && prefix.StartsWith(TestPrefix, StringComparison.Ordinal))
                return (null, "TEST- is kept for the seals of the test set.");
            var codes = new List<string>();
            for (var i = a; i <= b; i++)
                codes.Add(prefix + i.ToString(CultureInfo.InvariantCulture).PadLeft(width, '0'));
            if (codes.Any(c => c.StartsWith(LabelPrefix, StringComparison.Ordinal) || c.StartsWith(ContainerPrefix, StringComparison.Ordinal)
                               || c.StartsWith(TestPrefix + LabelPrefix, StringComparison.Ordinal) || c.StartsWith(TestPrefix + ContainerPrefix, StringComparison.Ordinal)))
                return (null, "TS- and TK- are kept for the platform's seal labels and containers.");
            var taken = await _context.SealCodes.Where(c => codes.Contains(c.Code)).Select(c => c.Code).OrderBy(c => c).ToListAsync();
            if (taken.Count > 0)
                return (null, string.Format(CultureInfo.InvariantCulture, "Some of these seals are already in a seal pool (first: {0}).", taken[0]));
            return (await AddBatchAsync(orgId, SealKind.Plastic, codes, userId, note), null);
        }

        private async Task<SealBatch> AddBatchAsync(int orgId, string kind, List<string> codes, string userId, string? note)
        {
            note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            var batch = new SealBatch
            {
                OrgId = orgId, Kind = kind, FirstCode = codes[0], LastCode = codes[^1], CodeCount = codes.Count,
                Note = note?.Length > 200 ? note[..200] : note, IssuedByUserId = userId, IssuedAt = Now
            };
            foreach (var c in codes)
                batch.Codes.Add(new SealCode { OrgId = orgId, Code = c, Kind = kind, Status = SealStatus.Issued });
            _context.SealBatches.Add(batch);
            await _context.SaveChangesAsync();
            return batch;
        }

        public async Task<(OrgContainer? Container, string? Error)> RegisterContainerAsync(int orgId, string? kind, string? name, decimal? capacityKg, decimal? tareKg, string userId)
        {
            name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            if (!ContainerKind.All.Contains(kind ?? ""))
                return (null, "Choose the kind of container.");
            if (name?.Length > 100)
                return (null, "The name has at most 100 characters.");
            if (capacityKg is <= 0 or > 100000 || tareKg is < 0 or > 100000)
                return (null, "Enter the capacity and the empty weight in kg (or leave them empty).");
            var prefix = await IsTestOrgAsync(orgId) ? TestPrefix + ContainerPrefix : ContainerPrefix;
            var last = await _context.OrgContainers.Where(c => c.Code.StartsWith(prefix)).Select(c => c.Code).OrderByDescending(c => c).FirstOrDefaultAsync();
            var next = last != null && int.TryParse(last[prefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n + 1 : 1;
            var container = new OrgContainer
            {
                OrgId = orgId, Code = prefix + next.ToString("D4", CultureInfo.InvariantCulture), Kind = kind!, Name = name,
                CapacityKg = capacityKg, TareKg = tareKg, CreatedByUserId = userId, CreatedAt = Now
            };
            _context.OrgContainers.Add(container);
            await _context.SaveChangesAsync();
            return (container, null);
        }

        // ---------------------------------------------------------------- checks at a delivery

        // the organizations whose seals may close a container at harvest: the receiving organization and the associations the
        // beekeeper belongs to (their field inspectors seal at the apiary)
        public async Task<List<int>> OriginPoolsAsync(int orgId, string? beekeeperId)
        {
            var today = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
            var pools = new List<int> { orgId };
            if (beekeeperId != null)
                pools.AddRange(await _context.BeekeeperAssociations
                    .Where(a => a.BeekeeperId == beekeeperId && a.ValidFrom <= today && (a.ValidTo == null || a.ValidTo >= today))
                    .Select(a => a.AssociationOrgId).ToListAsync());
            return pools.Distinct().ToList();
        }

        // why a code cannot be used here, or null. kinds: the seal kinds allowed for this use (null = any)
        public async Task<string?> RefusalAsync(string code, IReadOnlyCollection<int> pools, IReadOnlyCollection<string>? kinds)
        {
            var seal = await _context.SealCodes.AsNoTracking().Include(s => s.Container).Include(s => s.Sample).FirstOrDefaultAsync(s => s.Code == code);
            return Refusal(code, seal, pools, kinds);
        }

        public static string? Refusal(string code, SealCode? seal, IReadOnlyCollection<int> pools, IReadOnlyCollection<string>? kinds)
        {
            if (seal == null || !pools.Contains(seal.OrgId))
                return string.Format(CultureInfo.InvariantCulture, "Seal {0} is not in the seal pool of your organization.", code);
            if (seal.Status != SealStatus.Issued)
            {
                var where = seal.Container?.ContainerCode ?? seal.Sample?.SampleCode;
                return where != null
                    ? string.Format(CultureInfo.InvariantCulture, "Seal {0} was already used on {1}.", code, where)
                    : string.Format(CultureInfo.InvariantCulture, "Seal {0} was already used.", code);
            }
            if (kinds != null && !kinds.Contains(seal.Kind))
                return seal.Kind == SealKind.Plastic
                    ? string.Format(CultureInfo.InvariantCulture, "Seal {0} is a numbered plastic seal; a seal label is needed here.", code)
                    : string.Format(CultureInfo.InvariantCulture, "Seal {0} is a seal label; a numbered plastic seal is needed here.", code);
            return null;
        }

        // the seal kinds allowed to close a container with the chosen closure
        public static string[]? ClosingKinds(string? closure) => closure switch
        {
            ClosureKind.SealLabel => new[] { SealKind.Label },
            ClosureKind.PlasticSeal => new[] { SealKind.Plastic },
            _ => null
        };

        // the organization's permanent container, or why it cannot be used
        public async Task<(OrgContainer? Container, string? Error)> FindContainerAsync(string? raw, IReadOnlyCollection<int> pools)
        {
            var code = Normalize(raw);
            if (code == null)
                return (null, "Scan or type the code of the association's container.");
            var c = await _context.OrgContainers.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code);
            if (c == null || !pools.Contains(c.OrgId))
                return (null, string.Format(CultureInfo.InvariantCulture, "Container {0} is not registered for your organization.", code));
            if (!c.IsActive)
                return (null, string.Format(CultureInfo.InvariantCulture, "Container {0} is retired.", code));
            return (c, null);
        }

        // marks codes as used (tracked entities; saved by the caller)
        public async Task<Dictionary<string, SealCode>> TrackAsync(IEnumerable<string> codes)
        {
            var list = codes.Distinct().ToList();
            return await _context.SealCodes.Where(s => list.Contains(s.Code)).ToDictionaryAsync(s => s.Code, StringComparer.OrdinalIgnoreCase);
        }

        public static void Use(SealCode seal, string usedAs, string userId, DateTime at, Container? container = null, Sample? sample = null)
        {
            seal.Status = SealStatus.Used;
            seal.UsedAs = usedAs;
            seal.UsedByUserId = userId;
            seal.UsedAt = at;
            seal.Container = container;
            seal.Sample = sample;
        }

        public static void Break(SealCode seal, DateTime at)
        {
            seal.Status = SealStatus.Broken;
            seal.BrokenAt = at;
        }
    }
}
