using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;

namespace TOP4HoneyChains.Web.Services
{
    // D-52: provinces and districts (tables Provinces / Districts, seeded by 011), cached; forms store the names
    public class Places
    {
        public record ProvinceItem(string Code, string Name, IReadOnlyList<string> Districts);

        private static readonly CultureInfo Turkish = new("tr-TR");
        private readonly IServiceScopeFactory _scopes;
        private volatile List<ProvinceItem>? _list;

        public Places(IServiceScopeFactory scopes) => _scopes = scopes;

        public IReadOnlyList<ProvinceItem> All()
        {
            var list = _list;
            if (list != null)
                return list;
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
            try
            {
                var districts = db.Districts.AsNoTracking().ToList().GroupBy(d => d.ProvinceCode)
                    .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(d => d.Name).OrderBy(n => n, StringComparer.Create(Turkish, false)).ToList());
                list = db.Provinces.AsNoTracking().ToList()
                    .OrderBy(p => p.Name, StringComparer.Create(Turkish, false))
                    .Select(p => new ProvinceItem(p.Code, p.Name, districts.TryGetValue(p.Code, out var d) ? d : Array.Empty<string>()))
                    .ToList();
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                // before 011 is applied: free text only
                list = new();
            }
            _list = list;
            return list;
        }

        public bool Available => All().Count > 0;

        public ProvinceItem? Province(string? name) =>
            name == null ? null : All().FirstOrDefault(p => string.Equals(p.Name, name.Trim(), StringComparison.CurrentCultureIgnoreCase));

        public ProvinceItem? ByPlate(string? code) => All().FirstOrDefault(p => p.Code == code);

        // "TR061234567890" -> Ankara (TR + 2-digit plate code); null for other formats
        public ProvinceItem? FromRegistryNumber(string? number)
        {
            if (number == null || number.Length < 4 || !number.StartsWith("TR", StringComparison.OrdinalIgnoreCase)
                || !char.IsDigit(number[2]) || !char.IsDigit(number[3]))
                return null;
            return ByPlate(number.Substring(2, 2));
        }

        // a district that belongs to the province (exact list name), or null
        public string? DistrictOf(ProvinceItem province, string? district) =>
            district == null ? null : province.Districts.FirstOrDefault(d => string.Equals(d, district.Trim(), StringComparison.CurrentCultureIgnoreCase));
    }
}
