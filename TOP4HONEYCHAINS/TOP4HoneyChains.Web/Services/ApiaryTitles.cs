using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;

namespace TOP4HoneyChains.Web.Services
{
    // D-78 P1 (F5, D78-SPEC 3.6): apiary titles are generated, never typed: "Province – District – Locality", at most
    // 50 characters (the ApiaryTitle column). A longer title shortens the locality with "…"; a second apiary with the same
    // three parts gets " 2", " 3", …
    public static class ApiaryTitles
    {
        public const int MaxLength = 50;
        public const int LocalityMax = 60;
        public const string Generated = "Generated";
        private const string Separator = " – ";

        public static string Compose(string province, string district, string locality, int number = 1)
        {
            var suffix = number > 1 ? " " + number : "";
            province = province.Trim();
            district = district.Trim();
            locality = locality.Trim();
            var head = province + Separator + district + Separator;
            var title = head + locality + suffix;
            if (title.Length <= MaxLength)
                return title;
            var room = MaxLength - head.Length - suffix.Length - 1;
            if (room >= 3)
                return head + locality[..Math.Min(room, locality.Length)].TrimEnd() + "…" + suffix;
            // very long province and district names: keep the start of the whole text
            return (head + locality)[..(MaxLength - suffix.Length - 1)].TrimEnd() + "…" + suffix;
        }

        // the first free title (no other apiary uses it)
        public static async Task<string> UniqueAsync(TOP4HoneyChainsContext db, string province, string district, string locality, int? exceptApiaryId = null)
        {
            for (var n = 1; ; n++)
            {
                var title = Compose(province, district, locality, n);
                if (!await db.Apiaries.AnyAsync(a => a.ApiaryTitle == title && a.ApiaryId != exceptApiaryId))
                    return title;
            }
        }

        // province (plate code), district id and locality checked against the province and district lists
        public static async Task<(string? Province, string? District, string? Error)> ResolveAsync(TOP4HoneyChainsContext db, string? provinceCode, int? districtId, string? locality)
        {
            if (string.IsNullOrWhiteSpace(provinceCode) || districtId == null || string.IsNullOrWhiteSpace(locality))
                return (null, null, "Choose the province and the district and enter the locality of the apiary.");
            if (locality.Trim().Length > LocalityMax)
                return (null, null, "The locality has at most 60 characters.");
            var province = await db.Provinces.AsNoTracking().Where(p => p.Code == provinceCode).Select(p => p.Name).FirstOrDefaultAsync();
            var district = await db.Districts.AsNoTracking().Where(d => d.DistrictId == districtId && d.ProvinceCode == provinceCode).Select(d => d.Name).FirstOrDefaultAsync();
            if (province == null || district == null)
                return (null, null, "Choose a district of the chosen province.");
            return (province, district, null);
        }
    }
}
