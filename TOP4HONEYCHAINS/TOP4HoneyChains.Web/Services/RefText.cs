using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services
{
    // D-51: reference-data texts in the selected language. Looks up ReferenceTranslations (cached, reloaded when
    // the translation screen saves); the order is: selected language, then English, then the base column.
    public class RefText
    {
        private readonly IServiceScopeFactory _scopes;
        private volatile Dictionary<(string Entity, string Key, string Field, string Culture), string>? _table;
        private readonly object _lock = new();

        public RefText(IServiceScopeFactory scopes) => _scopes = scopes;

        public static string Culture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        // D-73: each language shows its own abbreviation for a critical control point (Turkish KKN, Spanish PCC, Polish KPK,
        // Latvian KKP, English CCP). Display only: the codes in the data stay "CCP n"; a code written without a space
        // (an input example such as "CCP1") is left as it is.
        private static readonly System.Text.RegularExpressions.Regex CcpWord = new(@"\bCCP(?=s?\b)", System.Text.RegularExpressions.RegexOptions.Compiled);
        private static readonly Dictionary<string, string> CcpAbbreviation = new() { ["tr"] = "KKN", ["es"] = "PCC", ["pl"] = "KPK", ["lv"] = "KKP" };

        public static string CcpText(string? text) =>
            string.IsNullOrEmpty(text) || !CcpAbbreviation.TryGetValue(Culture, out var abbreviation) ? text ?? "" : CcpWord.Replace(text, abbreviation);

        // "CCP 4" -> "KKN 4" in Turkish; also for composed titles ("CCP 0 · #20070")
        public string Code(string? text) => CcpText(text);

        public void Invalidate()
        {
            _table = null;
            _bases = null;
            _limitMaps.Clear();
        }

        // base texts that appear inside composed limit descriptions (parameter names, units, honey types)
        private volatile List<(string Entity, string Key, string Field, string Text)>? _bases;
        private readonly ConcurrentDictionary<string, List<(System.Text.RegularExpressions.Regex Pattern, string Text)>> _limitMaps = new();

        private List<(string Entity, string Key, string Field, string Text)> Bases()
        {
            var bases = _bases;
            if (bases != null)
                return bases;
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
            var list = new List<(string, string, string, string)>();
            foreach (var p in db.CriticalControlPointParameters.AsNoTracking().Select(p => new { p.ParameterId, p.ParameterName, p.ParameterUnit }).ToList())
            {
                if (!string.IsNullOrWhiteSpace(p.ParameterName)) list.Add((RefEntity.Parameter, p.ParameterId.ToString(CultureInfo.InvariantCulture), "Name", p.ParameterName));
                if (!string.IsNullOrWhiteSpace(p.ParameterUnit) && p.ParameterUnit != "None") list.Add((RefEntity.Parameter, p.ParameterId.ToString(CultureInfo.InvariantCulture), "Unit", p.ParameterUnit));
            }
            foreach (var h in db.HoneyTypes.AsNoTracking().Select(h => new { h.HoneyTypeId, h.HoneyTypeTitle }).ToList())
                list.Add((RefEntity.HoneyType, h.HoneyTypeId.ToString(CultureInfo.InvariantCulture), "Title", h.HoneyTypeTitle));
            foreach (var c in db.CriticalControlPoints.AsNoTracking().Select(c => new { c.Id, c.Title }).ToList())
                if (c.Title != null)
                    list.Add((RefEntity.Ccp, c.Id.ToString(CultureInfo.InvariantCulture), "Title", c.Title));
            foreach (var s in db.AssignmentStatuses.AsNoTracking().Select(s => new { s.AssignmentStatusId, s.AssignmentStatusName }).ToList())
                list.Add((RefEntity.AssignmentStatus, s.AssignmentStatusId.ToString(CultureInfo.InvariantCulture), "Name", s.AssignmentStatusName));
            foreach (var p in db.RawHoneyProductionPeriods.AsNoTracking().Select(p => new { p.RawHoneyProductionPeriodId, p.RawHoneyProductionPeriodTitle }).ToList())
                if (p.RawHoneyProductionPeriodTitle != null)
                    list.Add((RefEntity.ProductionPeriod, p.RawHoneyProductionPeriodId.ToString(CultureInfo.InvariantCulture), "Title", p.RawHoneyProductionPeriodTitle));
            // organization roles and capabilities are named by their code in server messages
            foreach (var code in db.OrgRoleDefinitions.AsNoTracking().Select(r => r.Code).ToList())
                list.Add((RefEntity.OrgRole, code, "Name", code));
            foreach (var code in db.Capabilities.AsNoTracking().Select(c => c.Code).ToList())
                list.Add((RefEntity.Capability, code, "Name", code));
            _bases = list;
            return list;
        }

        // shortened text for list columns
        public static string Short(string? text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length > max ? text[..max] + "..." : text;

        // translation of a reference-data text of any kind (used for the variable parts of server messages)
        public string? AnyText(string text)
        {
            var b = Bases().FirstOrDefault(x => x.Text == text);
            if (b.Key == null)
                return null;
            var t = T(b.Entity, b.Key, b.Field, b.Entity == RefEntity.OrgRole ? RoleEnglish(text) : text);
            return t == text ? null : t;
        }

        // translation by base text, for view models that carry only the text (honey type names, units)
        public string Text(string entity, string field, string? baseText)
        {
            if (string.IsNullOrWhiteSpace(baseText))
                return baseText ?? "";
            var b = Bases().FirstOrDefault(x => x.Entity == entity && x.Field == field && x.Text == baseText);
            return b.Key == null ? baseText : T(entity, b.Key, field, baseText);
        }

        // D-51: a limit description ("min 8 DN (Schade); Pine: max 23 % (Percentage) if HMF <= 15") in the selected
        // language. Composed and stored descriptions stay English in the database; they are translated for display.
        public string Limit(string? text, Func<string, string> ui)
        {
            if (string.IsNullOrWhiteSpace(text) || Culture == "en")
                return text ?? "";
            var map = _limitMaps.GetOrAdd(Culture, _ =>
            {
                var pairs = new Dictionary<string, string>();
                foreach (var b in Bases())
                {
                    var t = T(b.Entity, b.Key, b.Field, b.Text);
                    if (t != b.Text && !pairs.ContainsKey(b.Text))
                        pairs[b.Text] = t;
                }
                foreach (var c in HoneyClasses.All)
                    pairs.TryAdd(c, ui(c));
                return pairs.OrderByDescending(p => p.Key.Length)
                    .Select(p => (new System.Text.RegularExpressions.Regex(@"(?<![\w])" + System.Text.RegularExpressions.Regex.Escape(p.Key) + @"(?![\w])"), p.Value))
                    .ToList();
            });
            // placeholders keep replaced text from being replaced again
            var parts = new List<string>();
            foreach (var (pattern, value) in map)
                text = pattern.Replace(text, _ => { parts.Add(value); return "\u0001" + (parts.Count - 1) + "\u0002"; });
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bmin\b", ui("Limit.min"));
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bmax\b", ui("Limit.max"));
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bif\b", ui("Limit.if"));
            text = text.Replace("(not checkable)", "(" + ui("Limit.notCheckable") + ")");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bhoney type (\d+)", m => T(RefEntity.HoneyType, m.Groups[1].Value, "Title", ui("Limit.honeyType") + " " + m.Groups[1].Value));
            return System.Text.RegularExpressions.Regex.Replace(text, "\u0001(\\d+)\u0002", m => parts[int.Parse(m.Groups[1].Value)]);
        }

        private Dictionary<(string, string, string, string), string> Table()
        {
            var table = _table;
            if (table != null)
                return table;
            lock (_lock)
            {
                if (_table != null)
                    return _table;
                using var scope = _scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
                try
                {
                    _table = db.ReferenceTranslations.AsNoTracking()
                        .Select(t => new { t.Entity, t.EntityKey, t.Field, t.Culture, t.Value })
                        .ToList()
                        .ToDictionary(t => (t.Entity, t.EntityKey, t.Field, t.Culture), t => t.Value);
                }
                catch (Microsoft.Data.SqlClient.SqlException)
                {
                    // before 009 is applied: base texts only
                    _table = new();
                }
                return _table;
            }
        }

        public string T(string entity, object? key, string field, string? fallback)
        {
            if (key == null)
                return fallback ?? "";
            var k = Convert.ToString(key, CultureInfo.InvariantCulture) ?? "";
            var table = Table();
            if (table.TryGetValue((entity, k, field, Culture), out var value) && value.Length > 0)
                return CcpText(value);
            if (table.TryGetValue((entity, k, field, "en"), out value) && value.Length > 0)
                return value;
            return fallback ?? "";
        }

        // convenience helpers for the common reference data
        public string HoneyType(int? id, string? title) => T(RefEntity.HoneyType, id, "Title", title);
        public string HoneyTypeDescription(int? id, string? text) => T(RefEntity.HoneyType, id, "Description", text);
        public string Period(int? id, string? title) => T(RefEntity.ProductionPeriod, id, "Title", title);
        public string Status(int? id, string? name) => T(RefEntity.AssignmentStatus, id, "Name", name);
        public string CcpTitle(int? id, string? title) => T(RefEntity.Ccp, id, "Title", title);
        public string CcpStep(int? id, string? text) => T(RefEntity.Ccp, id, "ProcessingStep", text);
        public string CcpJustification(int? id, string? text) => T(RefEntity.Ccp, id, "CriticalityJustification", text);
        public string Category(int? id, string? name) => T(RefEntity.ParameterCategory, id, "Name", name);
        public string CategoryDescription(int? id, string? text) => T(RefEntity.ParameterCategory, id, "Description", text);
        public string Parameter(int? id, string? name) => T(RefEntity.Parameter, id, "Name", name);
        public string Unit(int? id, string? unit) => T(RefEntity.Parameter, id, "Unit", unit);
        public string LimitNote(int? id, string? text) => T(RefEntity.Parameter, id, "LimitNote", text);
        public string LimitSource(int? id, string? text) => T(RefEntity.Parameter, id, "LimitSource", text);
        public string Capability(string? code) => T(RefEntity.Capability, code, "Name", code);
        public string CapabilityDescription(string? code, string? text) => T(RefEntity.Capability, code, "Description", text);
        // D-67: English has no translation rows for the organization roles; show a readable name instead of the code
        public string Role(string? code) => T(RefEntity.OrgRole, code, "Name", RoleEnglish(code));

        private static readonly Dictionary<string, string> RoleNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Analyst"] = "Analyst", ["FieldInspector"] = "Field inspector", ["LabManager"] = "Lab manager", ["OrgAdmin"] = "Organisation admin",
            ["QualityApprover"] = "Quality approver", ["SampleAcceptanceOfficer"] = "Sample acceptance officer", ["Staff"] = "Staff"
        };

        // a known role's English name, otherwise the code split at its capitals ("LabManager" -> "Lab manager")
        public static string? RoleEnglish(string? code)
        {
            if (string.IsNullOrEmpty(code))
                return code;
            if (RoleNames.TryGetValue(code, out var name))
                return name;
            var words = System.Text.RegularExpressions.Regex.Split(code, "(?<=[a-z])(?=[A-Z])");
            return string.Join(" ", words.Select((w, i) => i == 0 ? w : w.ToLowerInvariant()));
        }
        public string Country(string? code, string? name) => T(RefEntity.Country, code, "Name", name ?? code);
    }
}
