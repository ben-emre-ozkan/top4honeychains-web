using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-98: honey types added from the data-entry forms ("Other") and merged by the platform administrator.
    // A typed name is compared with every existing title, every translation of a title and every merged-away title, after
    // Normalize (case, Turkish and other accented letters, spaces, punctuation and the word "honey" ignored). A match must be
    // picked from the list; otherwise the type is created with its honey class (the CCP limits need it), in the transaction of
    // the form that uses it. Sample acceptance officers and registration officers of a verified organization and the platform
    // administrator may add; the administrator renames and merges.
    public class HoneyTypeCatalog
    {
        // the value of "Other" in a honey type list
        public const int Other = -1;
        public const int MaxName = 100;
        // read by TR_RawHoneyProductions_DeliveryLocked (migration 035)
        public const string MergeSessionKey = "top4.honey_type_merge";

        public static readonly string[] AddingRoles = { OrgRoles.SampleAcceptanceOfficer, OrgRoles.RegistrationOfficer };

        public record NewType(string Name, string HoneyClass);

        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly RefText _ref;
        private readonly TimeProvider _clock;
        private readonly AuditWriter _audit;

        public HoneyTypeCatalog(TOP4HoneyChainsContext context, ICurrentOrg current, RefText refText, TimeProvider clock, AuditWriter audit)
        {
            _context = context;
            _current = current;
            _ref = refText;
            _clock = clock;
            _audit = audit;
        }

        // ---------------------------------------------------------------- comparing names

        // the words for "honey" in the five languages (after Normalize's letter mapping); "de"/"del" only right after "miel"
        private static readonly HashSet<string> HoneyWords = new(StringComparer.Ordinal) { "bal", "bali", "honey", "miel", "miod", "medus" };

        // "Çam Balı" -> "cam", "Pine Honey" -> "pine", "Miel de pino" -> "pino", "KESTANE  bal" -> "kestane"
        public static string Normalize(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            var s = name.Replace('İ', 'i').Replace('I', 'i').Replace('ı', 'i').Replace('Ł', 'l').Replace('ł', 'l').Replace('ß', 's')
                .ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var words = new List<string>();
            var word = new StringBuilder();
            foreach (var ch in s.Append(' '))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                    continue;
                if (char.IsLetterOrDigit(ch))
                {
                    word.Append(ch);
                    continue;
                }
                if (word.Length > 0)
                    words.Add(word.ToString());
                word.Clear();
            }
            var kept = new List<string>();
            for (var i = 0; i < words.Count; i++)
            {
                if (HoneyWords.Contains(words[i]))
                    continue;
                if (words[i] is "de" or "del" && i > 0 && words[i - 1] == "miel")
                    continue;
                kept.Add(words[i]);
            }
            return string.Concat(kept.Count > 0 ? kept : words);
        }

        // the existing honey type a name stands for (title, any translation of it, or a title merged into it); null when new
        public async Task<HoneyType?> MatchAsync(string? name, int? exceptId = null)
        {
            var key = Normalize(name);
            if (key.Length == 0)
                return null;
            var types = await _context.HoneyTypes.AsNoTracking().ToListAsync();
            var names = types.Select(t => (t.HoneyTypeId, t.HoneyTypeTitle)).ToList();
            names.AddRange((await _context.ReferenceTranslations.AsNoTracking()
                    .Where(r => r.Entity == RefEntity.HoneyType && r.Field == "Title").Select(r => new { r.EntityKey, r.Value }).ToListAsync())
                .Where(r => int.TryParse(r.EntityKey, NumberStyles.None, CultureInfo.InvariantCulture, out _))
                .Select(r => (int.Parse(r.EntityKey, CultureInfo.InvariantCulture), r.Value)));
            names.AddRange((await _context.HoneyTypeMerges.AsNoTracking().Select(m => new { m.TargetHoneyTypeId, m.SourceTitle }).ToListAsync())
                .Select(m => (m.TargetHoneyTypeId, m.SourceTitle)));
            var id = names.Where(n => n.Item1 != exceptId && Normalize(n.Item2) == key).Select(n => (int?)n.Item1).FirstOrDefault();
            return id == null ? null : types.FirstOrDefault(t => t.HoneyTypeId == id);
        }

        // ---------------------------------------------------------------- who may add

        public async Task<bool> MayAddAsync()
        {
            if (_current.IsPlatformAdmin)
                return true;
            var active = await _current.GetAsync();
            return active != null && active.IsVerified && !active.IsPlatform && AddingRoles.Contains(active.OrgRole);
        }

        // ---------------------------------------------------------------- checking and creating

        // The checks of a new name; null when the type can be created
        public async Task<string?> CheckNewAsync(string? name, string? honeyClass)
        {
            if (!await MayAddAsync())
                return "Your role cannot add a honey type; choose one from the list.";
            var trimmed = Clean(name);
            if (trimmed == null || trimmed.Length < 2 || trimmed.Length > MaxName || Normalize(trimmed).Length < 2)
                return "Enter the name of the new honey type (2 to 100 characters).";
            if (!HoneyClasses.All.Contains(honeyClass ?? ""))
                return "Choose the honey class of the new honey type (floral, honeydew or pine).";
            if (await MatchAsync(trimmed) is HoneyType existing)
                return $"This honey type already exists as {existing.HoneyTypeTitle}: choose it from the list.";
            return null;
        }

        // Adds the type inside the caller's transaction (call CheckNewAsync first). The typed name is the title; a translation row
        // in the officer's language keeps that name when the administrator later gives the type an English title.
        public async Task<int> CreateAsync(NewType t)
        {
            var name = Clean(t.Name)!;
            var active = _current.IsPlatformAdmin ? null : await _current.GetAsync();
            var type = new HoneyType
            {
                HoneyTypeTitle = name, HoneyClass = t.HoneyClass, CreatedAt = _clock.GetUtcNow().UtcDateTime, CreatedByUserId = _current.UserId,
                CreatedByOrgId = active?.OrgId
            };
            _context.HoneyTypes.Add(type);
            await _context.SaveChangesAsync();
            var culture = RefText.Culture;
            if (culture != "en" && ReferenceCultures.Contains(culture))
            {
                _context.ReferenceTranslations.Add(new ReferenceTranslation
                {
                    Entity = RefEntity.HoneyType, EntityKey = type.HoneyTypeId.ToString(CultureInfo.InvariantCulture), Field = "Title", Culture = culture,
                    Value = name, UpdatedAt = _clock.GetUtcNow().UtcDateTime, UpdatedByUserId = _current.UserId
                });
                await _context.SaveChangesAsync();
            }
            _ref.Invalidate();
            return type.HoneyTypeId;
        }

        private static readonly string[] ReferenceCultures = { "en", "tr", "es", "pl", "lv" };

        // The honey type of a form: an existing id, or "Other" with a name and a class that pass CheckNewAsync. Error or null.
        public async Task<string?> CheckChoiceAsync(int? honeyTypeId, string? newName, string? newClass)
        {
            if (honeyTypeId == Other)
                return await CheckNewAsync(newName, newClass);
            return honeyTypeId != null && await _context.HoneyTypes.AnyAsync(h => h.HoneyTypeId == honeyTypeId) ? null : "Unknown honey type.";
        }

        // ---------------------------------------------------------------- merging (platform administrator)

        // Moves every reference of the source to the target (lots, CCP limits, beekeepers' honey types, translations the target
        // lacks, earlier merges), records the merge and removes the source, in one transaction. Error or null.
        public async Task<string?> MergeAsync(int sourceId, int targetId)
        {
            if (!_current.IsPlatformAdmin)
                return "Only the platform administrator merges honey types.";
            if (sourceId == targetId)
                return "A honey type cannot be merged into itself.";
            var source = await _context.HoneyTypes.FirstOrDefaultAsync(h => h.HoneyTypeId == sourceId);
            var target = await _context.HoneyTypes.FirstOrDefaultAsync(h => h.HoneyTypeId == targetId);
            if (source == null || target == null)
                return "Unknown honey type.";
            // two limits of their own for the same parameter would compete; the administrator removes one on the Limits page first
            var targetParameters = await _context.CriticalControlPointParameterLimits.Where(l => l.HoneyTypeId == targetId).Select(l => l.ParameterId).ToListAsync();
            var clash = await _context.CriticalControlPointParameterLimits.Where(l => l.HoneyTypeId == sourceId && targetParameters.Contains(l.ParameterId))
                .Select(l => l.Parameter!.ParameterName).FirstOrDefaultAsync();
            if (clash != null)
                return $"Both honey types have their own limit for {clash}: remove one on the Limits page first.";

            var key = sourceId.ToString(CultureInfo.InvariantCulture);
            var targetKey = targetId.ToString(CultureInfo.InvariantCulture);
            var now = _clock.GetUtcNow().UtcDateTime;
            var counts = new Dictionary<string, int>();
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // the merge is recorded first: the delivery lock (035) lets a lot's honey type change only from a recorded source to its
            // target, and only while the session key is set; earlier merges into the source now point to the target
            foreach (var m in await _context.HoneyTypeMerges.Where(m => m.TargetHoneyTypeId == sourceId).ToListAsync())
                m.TargetHoneyTypeId = targetId;
            _context.HoneyTypeMerges.Add(new HoneyTypeMerge
            {
                SourceHoneyTypeId = sourceId, SourceTitle = source.HoneyTypeTitle, SourceClass = source.HoneyClass, TargetHoneyTypeId = targetId,
                MergedAt = now, MergedByUserId = _current.UserId!
            });
            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key = N'" + MergeSessionKey + "', @value = 1");
            counts["Lots"] = await _context.RawHoneyProductions.Where(r => r.RawHoneyProductionHoneyType == sourceId)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.RawHoneyProductionHoneyType, targetId));
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key = N'" + MergeSessionKey + "', @value = NULL");
            counts["Limits"] = await _context.CriticalControlPointParameterLimits.Where(l => l.HoneyTypeId == sourceId)
                .ExecuteUpdateAsync(s => s.SetProperty(l => l.HoneyTypeId, targetId));
            var haveTarget = _context.BeekeeperHoneyTypes.Where(b => b.HoneyTypeId == targetId).Select(b => b.BeekeeperId);
            var moving = await _context.BeekeeperHoneyTypes.Where(b => b.HoneyTypeId == sourceId && !haveTarget.Contains(b.BeekeeperId)).Select(b => b.BeekeeperId).ToListAsync();
            foreach (var beekeeperId in moving)
                _context.BeekeeperHoneyTypes.Add(new BeekeeperHoneyType { BeekeeperId = beekeeperId, HoneyTypeId = targetId });
            await _context.SaveChangesAsync();
            counts["Beekeepers"] = moving.Count;
            await _context.BeekeeperHoneyTypes.Where(b => b.HoneyTypeId == sourceId).ExecuteDeleteAsync();

            // translations: the target keeps its own; a language only the source had moves over
            var targetFields = await _context.ReferenceTranslations.Where(r => r.Entity == RefEntity.HoneyType && r.EntityKey == targetKey)
                .Select(r => r.Field + "|" + r.Culture).ToListAsync();
            var sourceRows = await _context.ReferenceTranslations.Where(r => r.Entity == RefEntity.HoneyType && r.EntityKey == key).ToListAsync();
            var moved = 0;
            foreach (var r in sourceRows)
            {
                if (!targetFields.Contains(r.Field + "|" + r.Culture))
                {
                    _context.ReferenceTranslations.Add(new ReferenceTranslation
                    {
                        Entity = r.Entity, EntityKey = targetKey, Field = r.Field, Culture = r.Culture, Value = r.Value, UpdatedAt = now, UpdatedByUserId = _current.UserId
                    });
                    moved++;
                }
                _context.ReferenceTranslations.Remove(r);
            }
            counts["Translations"] = moved;
            _context.HoneyTypes.Remove(source);
            await _context.SaveChangesAsync();

            // the bulk updates above bypass the change tracker: one explicit audit row names the merge and what moved
            await _audit.WriteAsync("Merge", "HoneyTypes", key,
                new { source.HoneyTypeId, source.HoneyTypeTitle, source.HoneyClass },
                new { TargetHoneyTypeId = targetId, target.HoneyTypeTitle, counts });
            await transaction.CommitAsync();
            _ref.Invalidate();
            return null;
        }

        private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : string.Join(' ', s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
