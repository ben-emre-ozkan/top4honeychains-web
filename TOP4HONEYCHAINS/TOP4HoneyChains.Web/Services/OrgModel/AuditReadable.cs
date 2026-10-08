using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-73: the audit log in readable form: who, when, which record, and the changed fields as "old → new" with
    // localised field names (resource "Audit.F.<field>"). Display only: the stored rows and the hash chain are untouched.
    public record AuditChange(string Label, string? Old, string? New);

    // names for the ids the rows mention: users, organizations, assignment statuses, CCP codes, parameters
    public record AuditLookups(IReadOnlyDictionary<string, string> Users, IReadOnlyDictionary<int, string> Orgs, IReadOnlyDictionary<int, string> Statuses,
        IReadOnlyDictionary<int, string> Ccps, IReadOnlyDictionary<int, string> Parameters);

    public record AuditRow(long Id, DateTime AtLocal, DateTime AtUtc, string? UserName, string? OrgName, string Action, string Entity, string? Key,
        string? RecordUrl, List<AuditChange> Changes, string? Flags, string? OldJson, string? NewJson, string Hash);

    public class AuditReadable
    {
        // entity prefixes of the database-first column names ("RawHoneyCCPAssignmentStatus" -> "Status")
        private static readonly string[] Prefixes = { "RawHoneyCCPAssignment", "FinalHoneyCCPAssignment", "FinalHoneyProduction", "RawHoneyProduction", "Beekeeper", "Apiary" };
        private static readonly Regex Words = new("(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])");
        private static readonly Regex IsoDate = new(@"^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}(:\d{2}(\.\d+)?)?(Z|[+-]\d{2}:\d{2})?)?$");

        private readonly IStringLocalizer<SharedResource> _l;
        private readonly RefText _ref;

        public AuditReadable(IStringLocalizer<SharedResource> localizer, RefText refText)
        {
            _l = localizer;
            _ref = refText;
        }

        public static string Canonical(string field)
        {
            if (field is "CCPId" or "CcpId" or "CCP")
                return "CcpId";
            foreach (var p in Prefixes)
                if (field.StartsWith(p, StringComparison.Ordinal) && field.Length > p.Length && field != p + "Id")
                {
                    field = field[p.Length..];
                    break;
                }
            return field switch { "CCP" => "CcpId", "QR" => "QRCode", _ => field };
        }

        public string Label(string field)
        {
            var canonical = Canonical(field);
            var text = _l["Audit.F." + canonical];
            return text.ResourceNotFound ? Words.Replace(canonical, " ") : text.Value;
        }

        public List<AuditRow> Build(IEnumerable<Models.Entities.AuditEntry> entries, AuditLookups names)
        {
            var orgs = names.Orgs;
            var rows = new List<AuditRow>();
            foreach (var e in entries)
            {
                var old = Parse(e.OldValues);
                var @new = Parse(e.NewValues);
                var changes = new List<AuditChange>();
                foreach (var field in old.Keys.Concat(@new.Keys).Distinct())
                {
                    old.TryGetValue(field, out var o);
                    @new.TryGetValue(field, out var n);
                    if (e.Action == "Update" && old.ContainsKey(field) && @new.ContainsKey(field) && JsonText(o) == JsonText(n))
                        continue;
                    changes.Add(new AuditChange(Label(field), old.ContainsKey(field) ? Value(field, o, names) : null, @new.ContainsKey(field) ? Value(field, n, names) : null));
                }
                var at = DateTime.SpecifyKind(e.At, DateTimeKind.Utc);
                rows.Add(new AuditRow(e.AuditId, at.ToLocalTime(), at, e.UserName, e.OrgId is int id ? orgs.GetValueOrDefault(id) ?? "#" + id : null,
                    e.Action, e.Entity, e.EntityKey, RecordUrl(e.Entity, e.EntityKey), changes, e.Flags, e.OldValues, e.NewValues, e.Hash));
            }
            return rows;
        }

        // user ids and organization ids that the rows mention, for one lookup each
        public static (HashSet<string> Users, HashSet<int> Orgs) References(IEnumerable<Models.Entities.AuditEntry> entries)
        {
            var users = new HashSet<string>();
            var orgs = new HashSet<int>();
            foreach (var e in entries)
            {
                if (e.OrgId is int o)
                    orgs.Add(o);
                foreach (var (field, value) in Parse(e.OldValues).Concat(Parse(e.NewValues)))
                {
                    if (field.EndsWith("UserId", StringComparison.Ordinal) && value.ValueKind == JsonValueKind.String)
                        users.Add(value.GetString()!);
                    else if (field.EndsWith("OrgId", StringComparison.Ordinal) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var id))
                        orgs.Add(id);
                }
            }
            return (users, orgs);
        }

        private static Dictionary<string, JsonElement> Parse(string? json)
        {
            var result = new Dictionary<string, JsonElement>();
            if (string.IsNullOrWhiteSpace(json))
                return result;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    foreach (var p in doc.RootElement.EnumerateObject())
                        result[p.Name] = p.Value.Clone();
            }
            catch (JsonException) { /* not JSON: the raw text stays available behind the toggle */ }
            return result;
        }

        private static string? JsonText(JsonElement? v) => v?.GetRawText();

        private string Value(string field, JsonElement? element, AuditLookups names)
        {
            var (users, orgs) = (names.Users, names.Orgs);
            if (element is not { } v || v.ValueKind == JsonValueKind.Null || v.ValueKind == JsonValueKind.Undefined)
                return "—";
            var canonical = Canonical(field);
            switch (v.ValueKind)
            {
                case JsonValueKind.True:
                    return _l["Yes"].Value;
                case JsonValueKind.False:
                    return _l["No"].Value;
                case JsonValueKind.Number:
                    if (field.EndsWith("OrgId", StringComparison.Ordinal) && v.TryGetInt32(out var org))
                        return orgs.GetValueOrDefault(org) ?? "#" + org;
                    if (canonical == "Status" && v.TryGetInt32(out var status))
                        return _ref.Status(status, names.Statuses.GetValueOrDefault(status) ?? status.ToString(CultureInfo.InvariantCulture));
                    if (canonical == "CcpId" && v.TryGetInt32(out var ccp))
                        return names.Ccps.TryGetValue(ccp, out var code) ? _ref.Code(code) : "#" + ccp;
                    if (canonical == "ParameterId" && v.TryGetInt32(out var parameter))
                        return _ref.Parameter(parameter, names.Parameters.GetValueOrDefault(parameter) ?? "#" + parameter);
                    return v.TryGetDecimal(out var d) ? d.ToString("0.####", CultureInfo.CurrentCulture) : v.GetRawText();
                case JsonValueKind.String:
                    var s = v.GetString() ?? "";
                    if (field.EndsWith("UserId", StringComparison.Ordinal))
                        return users.GetValueOrDefault(s) ?? Short(s, 10);
                    if (IsoDate.IsMatch(s) && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
                        return s.Length == 10 ? date.ToString("d", CultureInfo.CurrentCulture)
                            : (date.Kind == DateTimeKind.Utc ? date.ToLocalTime() : date).ToString("g", CultureInfo.CurrentCulture);
                    if (canonical is "Status" or "Event" or "Method" or "Purpose")
                        return _l[s].Value;
                    if (canonical.EndsWith("Sha256", StringComparison.Ordinal) || canonical == "Hash")
                        return Short(s, 12);
                    return Short(s, 80);
                default:
                    return Short(v.GetRawText(), 80);
            }
        }

        private static string Short(string s, int max) => s.Length <= max ? s : s[..max] + "…";

        // a link to the record where the admin pages have one
        private static string? RecordUrl(string entity, string? key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            var pair = key.Split('/');
            var id = key;
            return entity switch
            {
                "RawHoneyCCPAssignments" when pair.Length == 2 => $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={pair[0]}&ccpId={pair[1]}",
                "FinalHoneyCCPAssignments" when pair.Length == 2 => $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={pair[0]}&ccpId={pair[1]}",
                "Beekeepers" => $"/Beekeepers/Edit/{Uri.EscapeDataString(id)}",
                "Apiaries" => $"/Apiaries/Edit/{Uri.EscapeDataString(id)}",
                "RawHoneyProductions" => $"/RawHoneyProductions/CCPOverview/{Uri.EscapeDataString(id)}",
                "FinalHoneyProductions" => $"/FinalHoneyProductions/CCPOverview/{Uri.EscapeDataString(id)}",
                "Organizations" => $"/Organizations/Details/{Uri.EscapeDataString(id)}",
                "Samples" => $"/LabReports/Edit/{Uri.EscapeDataString(id)}",
                _ => null
            };
        }
    }
}
