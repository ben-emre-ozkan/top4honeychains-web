namespace TOP4HoneyChains.Web.Models.ViewModels.HoneyStory
{
    // The lab values of a lot (CCP 1, written when the report is approved) in the groups of the mobile app's
    // lab report: freshness, maturity and authenticity, sugar profile and purity, residues. The highlights are
    // the four values of "Quality at a glance".
    public static class HoneyStoryLab
    {
        public const string LabCcp = "CCP 1";
        public const string Freshness = "freshness", Maturity = "maturity", Sugar = "sugar", Residues = "residues", Other = "other";

        public static readonly string[] Groups = { Freshness, Maturity, Sugar, Residues, Other };

        // Parameter names as stored in the database (English seeds, Turkish for the three sugars)
        private static readonly Dictionary<string, string> GroupOf = new(StringComparer.OrdinalIgnoreCase)
        {
            ["HMF"] = Freshness, ["Diastase"] = Freshness,
            ["Moisture"] = Maturity, ["Free Acidity"] = Maturity, ["pH"] = Maturity, ["Electrical Conductivity"] = Maturity,
            ["Fruktoz"] = Sugar, ["Glikoz"] = Sugar, ["Sucrose"] = Sugar, ["C4 Sugar"] = Sugar, ["Insoluble Matter"] = Sugar,
            ["Maltoz"] = Sugar, ["Proline"] = Sugar,
            ["Antibiotics"] = Residues, ["Pesticides"] = Residues, ["Naphthalene"] = Residues
        };

        public static readonly string[] Highlights = { "HMF", "Moisture", "Diastase", "Free Acidity" };

        // "Read together" on the parameter page: values that tell more side by side
        public static readonly (string A, string B)[] Pairs = { ("HMF", "Diastase"), ("Fruktoz", "Glikoz"), ("Moisture", "Free Acidity") };

        public static string GroupOfParameter(string? name) => name != null && GroupOf.TryGetValue(name.Trim(), out var g) ? g : Other;

        // The numeric lab values of a lot, in the order of the parameters
        public static List<HoneyStoryCCPRow> Values(HoneyStoryRawSource source) =>
            source.CcpBlocks.Where(b => b.CCPCode == LabCcp && b.State != HoneyStoryCcpBlock.NotApplicable && b.State != HoneyStoryCcpBlock.NotRecorded)
                .SelectMany(b => b.Parameters).Where(r => r.Recorded && r.NumericValue != null).ToList();

        public static List<HoneyStoryCCPRow> HighlightValues(HoneyStoryRawSource source)
        {
            var values = Values(source);
            return Highlights.Select(h => values.FirstOrDefault(v => string.Equals(v.ParameterName?.Trim(), h, StringComparison.OrdinalIgnoreCase)))
                .Where(v => v != null).ToList()!;
        }

        // Bar of a verified limit: the green zone and the value marker, in percent of the scale
        public static (double ZoneStart, double ZoneEnd, double Value)? Bar(HoneyStoryCCPRow row)
        {
            if (row.Compliant == null || row.NumericValue == null || (row.LimitMin == null && row.LimitMax == null))
                return null;
            var top = row.LimitMax is > 0 ? row.LimitMax.Value * 1.5m : Math.Max(row.LimitMin!.Value * 2.5m, row.NumericValue.Value * 1.2m);
            if (top <= 0)
                return null;
            static double Pct(decimal v, decimal top) => Math.Round((double)Math.Clamp(v / top * 100m, 0m, 100m), 1);
            return (row.LimitMin is { } min ? Pct(min, top) : 0, row.LimitMax is { } max ? Pct(max, top) : 100, Pct(row.NumericValue.Value, top));
        }
    }
}
