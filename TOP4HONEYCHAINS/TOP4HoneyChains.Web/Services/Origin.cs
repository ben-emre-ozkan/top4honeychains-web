using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services
{
    // Country of origin of lots and the country percentages of batches (EU 2024/1438, D-31, D-43)
    public static class Origin
    {
        public const string Default = "TR";

        // ISO 3166-1 alpha-2 codes offered for a lot, with English and Turkish names
        public static readonly IReadOnlyList<(string Code, string En, string Tr)> Countries = new[]
        {
            ("TR", "Türkiye", "Türkiye"),
            ("AR", "Argentina", "Arjantin"),
            ("BG", "Bulgaria", "Bulgaristan"),
            ("CN", "China", "Çin"),
            ("DE", "Germany", "Almanya"),
            ("ES", "Spain", "İspanya"),
            ("GE", "Georgia", "Gürcistan"),
            ("GR", "Greece", "Yunanistan"),
            ("HU", "Hungary", "Macaristan"),
            ("IT", "Italy", "İtalya"),
            ("LV", "Latvia", "Letonya"),
            ("MX", "Mexico", "Meksika"),
            ("NZ", "New Zealand", "Yeni Zelanda"),
            ("PL", "Poland", "Polonya"),
            ("RO", "Romania", "Romanya"),
            ("UA", "Ukraine", "Ukrayna"),
            ("UY", "Uruguay", "Uruguay"),
            ("VN", "Vietnam", "Vietnam")
        };

        public static bool IsKnown(string? code) => code != null && Countries.Any(c => c.Code == code);

        public static string Name(string? code, string? culture = null)
        {
            code ??= Default;
            var c = Countries.FirstOrDefault(x => x.Code == code);
            if (c.Code == null)
                return code;
            // D-51: the stored translation of the selected language (ReferenceTranslations), English as fallback
            if (culture == null && Translate != null)
                return Translate(c.Code, c.En);
            return (culture ?? System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) == "tr" ? c.Tr : c.En;
        }

        // set at startup to RefText.Country
        public static Func<string, string, string>? Translate { get; set; }

        public record Share(string Country, decimal Amount, decimal Percentage);

        // Shares by weight, largest first. Percentages have two decimals and add up to exactly 100
        // (largest-remainder rounding); a lot without a country counts as Türkiye.
        public static List<Share> Compute(IEnumerable<(string? Country, decimal Amount)> parts)
        {
            var byCountry = parts.Where(p => p.Amount > 0)
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Country) ? Default : p.Country!.Trim().ToUpperInvariant())
                .Select(g => (Country: g.Key, Amount: g.Sum(x => x.Amount)))
                .ToList();
            var total = byCountry.Sum(x => x.Amount);
            if (total <= 0)
                return new();

            // work in hundredths of a percent: floor first, then hand out the rest by largest remainder
            var raw = byCountry.Select(x => (x.Country, x.Amount, Exact: x.Amount * 10000m / total)).ToList();
            var floors = raw.Select(x => (x.Country, x.Amount, Units: Math.Floor(x.Exact), Remainder: x.Exact - Math.Floor(x.Exact))).ToList();
            var missing = 10000m - floors.Sum(x => x.Units);
            var order = floors.OrderByDescending(x => x.Remainder).ThenByDescending(x => x.Amount).ThenBy(x => x.Country).Select(x => x.Country).ToList();
            var bonus = order.Take((int)missing).ToHashSet();
            return floors
                .Select(x => new Share(x.Country, x.Amount, (x.Units + (bonus.Contains(x.Country) ? 1 : 0)) / 100m))
                .OrderByDescending(x => x.Percentage).ThenBy(x => x.Country)
                .ToList();
        }

        // Stores the shares of a batch from its composition (called at finalize; replaces earlier rows)
        public static async Task StoreAsync(TOP4HoneyChainsContext context, int batchId)
        {
            var parts = await context.FinalHoneyProductionDetails
                .Where(d => d.FinalHoneyProductionId == batchId)
                .Select(d => new { d.RawHoneyProduction.OriginCountry, d.UsedAmount })
                .ToListAsync();
            context.BatchOriginShares.RemoveRange(await context.BatchOriginShares.Where(s => s.FinalHoneyProductionId == batchId).ToListAsync());
            foreach (var s in Compute(parts.Select(p => (p.OriginCountry, p.UsedAmount))))
                context.BatchOriginShares.Add(new BatchOriginShare
                {
                    FinalHoneyProductionId = batchId,
                    CountryCode = s.Country,
                    Amount = s.Amount,
                    Percentage = s.Percentage,
                    ComputedAt = DateTime.UtcNow
                });
        }
    }
}
