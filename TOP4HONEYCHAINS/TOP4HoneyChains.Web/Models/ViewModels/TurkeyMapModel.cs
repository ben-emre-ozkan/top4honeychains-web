namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // D-72: input of the shared Türkiye map (Views/Shared/_TurkeyMap.cshtml, wwwroot/css/tmap.css, wwwroot/js/tmap.js).
    // Two modes: a province choropleth (Areas) or a single marker (Marker). D-98: with Districts, a province with data (or the
    // marked one) drills down to its districts, shaded by DistrictAreas. Outlines only; never a coordinate of an apiary.
    public class TurkeyMapModel
    {
        public string Id { get; init; } = "tmap";
        public string Label { get; init; } = "";                    // accessible name of the map
        public List<TurkeyMapArea> Areas { get; init; } = new();     // choropleth: the provinces that have data
        public TurkeyMapMarker? Marker { get; init; }                // marker mode
        public string? LegendTitle { get; init; }
        public string? LegendLow { get; init; }
        public string? LegendHigh { get; init; }
        public bool FillIn { get; init; }                            // the provinces fill in on first view (Honey Story)
        public string? CssClass { get; init; }
        public bool Districts { get; init; }                         // D-98: drill down to the districts (with the attribution line)
        public List<TurkeyMapDistrictArea> DistrictAreas { get; init; } = new();   // districts with data, shaded inside the drill-down

        // honey shade 1-5 of a value, relative to the largest value on the map
        public int Level(double value)
        {
            var max = Areas.Count == 0 ? 0 : Areas.Max(a => a.Value);
            return Shade(value, max);
        }

        // honey shade 1-5 of a district's value, relative to the largest district value on the map
        public int DistrictLevel(double value) => Shade(value, DistrictAreas.Count == 0 ? 0 : DistrictAreas.Max(a => a.Value));

        private static int Shade(double value, double max) =>
            max <= 0 || value <= 0 ? 0 : Math.Clamp((int)Math.Ceiling(5 * value / max), 1, 5);
    }

    // Tip: the second line of the tooltip and of the accessible name, e.g. "3 beekeepers · 4 apiaries"
    public record TurkeyMapArea(int Plate, double Value, string Tip);

    // D-98: District is the name in the Districts table (TurkeyDistricts.Find matches it within the province)
    public record TurkeyMapDistrictArea(int Plate, string District, double Value, string Tip);

    // D-98: a small map of one province with one district filled (Views/Shared/_DistrictOutline.cshtml); District null:
    // the whole province is filled. Label: the accessible name, e.g. "Akçakoca, Düzce"
    public record DistrictOutlineModel(int Plate, string? District, string Label);

    // X, Y on the 640 x 300 map (ProvinceMap.ToMap of the province centre); Label e.g. "Akçakoca, Düzce"
    public record TurkeyMapMarker(int Plate, double X, double Y, string Label, string? Sub);
}
