namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // D-72: input of the shared Türkiye map (Views/Shared/_TurkeyMap.cshtml, wwwroot/css/tmap.css, wwwroot/js/tmap.js).
    // Two modes: a province choropleth (Areas) or a single marker (Marker). Provinces only; never a coordinate of an apiary.
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

        // honey shade 1-5 of a value, relative to the largest value on the map
        public int Level(double value)
        {
            var max = Areas.Count == 0 ? 0 : Areas.Max(a => a.Value);
            return max <= 0 || value <= 0 ? 0 : Math.Clamp((int)Math.Ceiling(5 * value / max), 1, 5);
        }
    }

    // Tip: the second line of the tooltip and of the accessible name, e.g. "3 beekeepers · 4 apiaries"
    public record TurkeyMapArea(int Plate, double Value, string Tip);

    // X, Y on the 640 x 300 map (ProvinceMap.ToMap of the province centre); Label e.g. "Akçakoca, Düzce"
    public record TurkeyMapMarker(int Plate, double X, double Y, string Label, string? Sub);
}
