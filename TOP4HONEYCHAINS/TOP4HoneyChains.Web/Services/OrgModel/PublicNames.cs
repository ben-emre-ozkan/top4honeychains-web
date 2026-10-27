using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P3 (D78-SPEC 6.4 C5, O-8, D-82): how a beekeeper appears on public pages (Honey Story). The name only with an active
    // consent to show it (PublicName, or a storefront consent with the name); otherwise the neutral storefront code. Legacy
    // beekeepers (never registered through D-78) follow PublicNames:LegacyMode: HideUntilConsent (default, the code until a
    // consent is recorded) or ShowUntilReverified (the name until the beekeeper is registered again, then the consent rule).
    public class PublicNameOptions
    {
        public string LegacyMode { get; set; } = LegacyNameMode.HideUntilConsent;
    }

    public static class LegacyNameMode
    {
        public const string HideUntilConsent = "HideUntilConsent";
        public const string ShowUntilReverified = "ShowUntilReverified";
    }

    public class PublicNameService
    {
        public record PublicName(string Text, bool IsName);

        private readonly TOP4HoneyChainsContext _context;
        private readonly StorefrontService _storefronts;
        private readonly PublicNameOptions _options;

        public PublicNameService(TOP4HoneyChainsContext context, StorefrontService storefronts, IOptionsSnapshot<PublicNameOptions> options)
        {
            _context = context;
            _storefronts = storefronts;
            _options = options.Value;
        }

        public static bool ShowsName(bool nameConsent, bool registered, string legacyMode) =>
            nameConsent || (!registered && string.Equals(legacyMode, LegacyNameMode.ShowUntilReverified, StringComparison.OrdinalIgnoreCase));

        public async Task<Dictionary<string, PublicName>> NamesAsync(IEnumerable<string> beekeeperIds)
        {
            var ids = beekeeperIds.Distinct().ToList();
            if (ids.Count == 0)
                return new();
            var people = await _context.Beekeepers.AsNoTracking().Where(b => ids.Contains(b.BeekeeperId))
                .Select(b => new { b.BeekeeperId, b.BeekeeperFirstName, b.BeekeeperLastName }).ToListAsync();
            var consented = (await _context.Consents.AsNoTracking()
                .Where(c => ids.Contains(c.BeekeeperId) && c.WithdrawnAt == null && c.NameConsented
                            && (c.Purpose == ConsentPurpose.PublicName || c.Purpose == ConsentPurpose.Storefront))
                .Select(c => c.BeekeeperId).ToListAsync()).ToHashSet();
            var registered = (await _context.BeekeeperRegistrations.AsNoTracking().Where(r => ids.Contains(r.BeekeeperId))
                .Select(r => r.BeekeeperId).Distinct().ToListAsync()).ToHashSet();
            var codes = await CodesAsync(people.Where(p => !ShowsName(consented.Contains(p.BeekeeperId!), registered.Contains(p.BeekeeperId!), _options.LegacyMode))
                .Select(p => p.BeekeeperId!).ToList());
            return people.ToDictionary(p => p.BeekeeperId!, p =>
            {
                var name = $"{p.BeekeeperFirstName} {p.BeekeeperLastName}".Trim();
                return codes.TryGetValue(p.BeekeeperId!, out var code) || name.Length == 0
                    ? new PublicName(code ?? "", false)
                    : new PublicName(name, true);
            });
        }

        // the stable neutral code of each beekeeper (created once, the same one the storefront uses)
        private async Task<Dictionary<string, string>> CodesAsync(List<string> ids)
        {
            if (ids.Count == 0)
                return new();
            var codes = await _context.BeekeeperStorefronts.AsNoTracking().Where(s => ids.Contains(s.BeekeeperId)).ToDictionaryAsync(s => s.BeekeeperId, s => s.Code);
            var missing = ids.Where(id => !codes.ContainsKey(id)).ToList();
            if (missing.Count == 0)
                return codes;
            foreach (var id in missing)
                codes[id] = await _storefronts.EnsureCodeAsync(id);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // another request created the code at the same time: read the stored ones
                foreach (var entry in _context.ChangeTracker.Entries<BeekeeperStorefront>().ToList())
                    entry.State = EntityState.Detached;
                return await _context.BeekeeperStorefronts.AsNoTracking().Where(s => ids.Contains(s.BeekeeperId)).ToDictionaryAsync(s => s.BeekeeperId, s => s.Code);
            }
            return codes;
        }
    }
}
