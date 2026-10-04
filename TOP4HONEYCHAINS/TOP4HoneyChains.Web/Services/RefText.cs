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

        public void Invalidate() => _table = null;

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
                return value;
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
        public string Role(string? code) => T(RefEntity.OrgRole, code, "Name", code);
        public string Country(string? code, string? name) => T(RefEntity.Country, code, "Name", name ?? code);
    }
}
