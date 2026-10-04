using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // The consent text the association reads to the beekeeper; its version and hash are stored with the
    // consent. The wording is a placeholder until the partners confirm the KVKK text (PQ-09).
    public static class ConsentText
    {
        public const string Version = "storefront-2026-10-v1";

        public const string Text =
            "Arıcı, TOP4HoneyChains platformunda herkese açık bir arıcı sayfası yayımlanmasına onay verir. Sayfada yalnızca doğrulanmış veriler gösterilir: " +
            "(onay verildiyse) ad soyad, aksi halde nötr bir kod; il ve ilçe; bal türleri; doğrulanmış lot sayısı; onaylı laboratuvar sonuçları; balının bulunduğu partiler ve birliği. " +
            "Telefon, fotoğraf, doğum tarihi ve kesin konum gösterilmez. Onay her zaman birlik aracılığıyla geri çekilebilir; sayfa hemen yayından kalkar.\n\n" +
            "The beekeeper agrees to a public beekeeper page on the TOP4HoneyChains platform. The page shows verified data only: the name (if agreed) or a neutral code, " +
            "province and district, honey types, the number of verified lots, approved lab results, the batches with the beekeeper's honey and the association. " +
            "No phone, photo, birth date or exact location is shown. The consent can be withdrawn at any time through the association; the page is unpublished at once.";

        public static readonly string Sha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Text))).ToLowerInvariant();
    }

    public class StorefrontService
    {
        public const string ConsentDocuments = "App_Data/consents";

        private readonly TOP4HoneyChainsContext _context;

        public StorefrontService(TOP4HoneyChainsContext context) => _context = context;

        // D-30: a storefront is public only from verified data and with an active consent
        public static bool IsPublished(bool beekeeperActive, bool registryVerified, bool associationVerified, bool consentByCurrentAssociation, bool consentActive) =>
            beekeeperActive && registryVerified && associationVerified && consentByCurrentAssociation && consentActive;

        // The name only with the beekeeper's consent; otherwise the neutral code
        public static string DisplayName(bool nameConsented, string? firstName, string? lastName, string code)
        {
            var name = $"{firstName} {lastName}".Trim();
            return nameConsented && name.Length > 0 ? name : code;
        }

        public static string NewCode()
        {
            const string alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
            return "BK-" + new string(RandomNumberGenerator.GetBytes(8).Select(b => alphabet[b % alphabet.Length]).ToArray());
        }

        public record State(string BeekeeperId, bool BeekeeperActive, bool RegistryVerified, int? AssociationOrgId, string? AssociationName,
            bool AssociationVerified, Consent? Consent, string? Code)
        {
            public bool ConsentByCurrentAssociation => Consent != null && Consent.AssociationOrgId == AssociationOrgId;
            public bool Published => IsPublished(BeekeeperActive, RegistryVerified, AssociationVerified, ConsentByCurrentAssociation, Consent != null) && Code != null;
        }

        public async Task<Dictionary<string, State>> StatesAsync(IEnumerable<string> beekeeperIds)
        {
            var ids = beekeeperIds.Distinct().ToList();
            var beekeepers = await _context.Beekeepers.AsNoTracking().Where(b => ids.Contains(b.BeekeeperId))
                .Select(b => new { b.BeekeeperId, b.IsActive }).ToListAsync();
            var registry = await _context.BeekeeperRegistry.AsNoTracking().Where(r => ids.Contains(r.BeekeeperId))
                .ToDictionaryAsync(r => r.BeekeeperId, r => r.Status == RegistryStatus.Verified);
            var links = await _context.BeekeeperAssociations.AsNoTracking().Where(a => ids.Contains(a.BeekeeperId) && a.ValidTo == null)
                .Select(a => new { a.BeekeeperId, a.AssociationOrgId, a.AssociationOrg!.DisplayName, a.AssociationOrg.Status }).ToListAsync();
            var consents = await _context.Consents.AsNoTracking()
                .Where(c => ids.Contains(c.BeekeeperId) && c.Purpose == ConsentPurpose.Storefront && c.WithdrawnAt == null).ToListAsync();
            var codes = await _context.BeekeeperStorefronts.AsNoTracking().Where(s => ids.Contains(s.BeekeeperId))
                .ToDictionaryAsync(s => s.BeekeeperId, s => s.Code);
            return beekeepers.ToDictionary(b => b.BeekeeperId, b =>
            {
                var link = links.FirstOrDefault(l => l.BeekeeperId == b.BeekeeperId);
                return new State(b.BeekeeperId, b.IsActive, registry.GetValueOrDefault(b.BeekeeperId),
                    link?.AssociationOrgId, link?.DisplayName, link?.Status == OrgStatus.Verified,
                    consents.FirstOrDefault(c => c.BeekeeperId == b.BeekeeperId), codes.GetValueOrDefault(b.BeekeeperId));
            });
        }

        public async Task<string> EnsureCodeAsync(string beekeeperId)
        {
            var existing = await _context.BeekeeperStorefronts.FirstOrDefaultAsync(s => s.BeekeeperId == beekeeperId);
            if (existing != null)
                return existing.Code;
            var code = NewCode();
            _context.BeekeeperStorefronts.Add(new BeekeeperStorefront { BeekeeperId = beekeeperId, Code = code, CreatedAt = DateTime.UtcNow });
            return code;
        }

        // Published storefront codes of the given beekeepers (for links from Honey Story)
        public async Task<Dictionary<string, string>> PublishedCodesAsync(IEnumerable<string> beekeeperIds) =>
            (await StatesAsync(beekeeperIds)).Values.Where(s => s.Published).ToDictionary(s => s.BeekeeperId, s => s.Code!);
    }
}
