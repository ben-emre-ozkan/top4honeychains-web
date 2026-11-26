using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TOP4HoneyChains.Web.Services
{
    // D-98: the 973 districts of Türkiye as SVG paths on the same 640 x 300 map as the provinces (TurkeyMap), keyed by
    // plate and by the name in the Districts table (geoBoundaries gbOpen ADM2, data © OpenStreetMap contributors, ODbL 1.0;
    // Data/Geo/README.md). The province map drills down to them; pages show a district outline, never a coordinate.
    public static partial class TurkeyDistricts
    {
        public record Box(double X0, double Y0, double X1, double Y1)
        {
            public double Width => X1 - X0;
            public double Height => Y1 - Y0;
        }

        public record District(int Plate, string Name, string Path, double Cx, double Cy, IReadOnlyList<(double X, double Y)[]> Rings, Box Box);

        private const string Resource = "TOP4HoneyChains.Web.turkey-districts.json";
        private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
        private static readonly Lazy<Dictionary<int, IReadOnlyList<District>>> Data = new(Load);

        public static int Count => Data.Value.Values.Sum(d => d.Count);

        // the districts of a province, by name; empty for an unknown plate
        public static IReadOnlyList<District> Of(int plate) => Data.Value.TryGetValue(plate, out var list) ? list : Array.Empty<District>();

        // "akçakoca" in Düzce -> Akçakoca (Turkish casing rules); null when the province has no district of that name
        public static District? Find(int plate, string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            return Of(plate).FirstOrDefault(d => string.Compare(d.Name, name.Trim(), Tr, CompareOptions.IgnoreCase) == 0);
        }

        // The district of a WGS84 point in the given province (server side only; the point never leaves the server): the
        // district that holds it, else the nearest one within a short distance, since the province map (Natural Earth) and
        // the district map (OpenStreetMap) draw some borders a little differently
        public static District? At(int plate, double? latitude, double? longitude)
        {
            if (latitude is not { } lat || longitude is not { } lon || Of(plate).Count == 0)
                return null;
            var (x, y) = ProvinceMap.ToMap(lat, lon);
            var inside = Of(plate).FirstOrDefault(d => Inside(d, x, y));
            if (inside != null)
                return inside;
            var nearest = Of(plate).Select(d => (District: d, Distance: Distance(d, x, y))).MinBy(t => t.Distance);
            return nearest.Distance <= NearestWithin ? nearest.District : null;
        }

        private const double NearestWithin = 12;   // map units (about 30 km), as TurkeyMap.PlateAt

        private static double Distance(District d, double x, double y)
        {
            var best = double.MaxValue;
            foreach (var ring in d.Rings)
                for (int i = 0, j = ring.Length - 1; i < ring.Length; j = i++)
                {
                    var (ax, ay, bx, by) = (ring[j].X, ring[j].Y, ring[i].X, ring[i].Y);
                    var len = (bx - ax) * (bx - ax) + (by - ay) * (by - ay);
                    var t = len == 0 ? 0 : Math.Clamp(((x - ax) * (bx - ax) + (y - ay) * (by - ay)) / len, 0, 1);
                    var (dx, dy) = (ax + t * (bx - ax) - x, ay + t * (by - ay) - y);
                    best = Math.Min(best, dx * dx + dy * dy);
                }
            return Math.Sqrt(best);
        }

        // the bounding box of a province, from its districts
        public static Box? BoxOf(int plate)
        {
            var list = Of(plate);
            return list.Count == 0 ? null : new Box(list.Min(d => d.Box.X0), list.Min(d => d.Box.Y0), list.Max(d => d.Box.X1), list.Max(d => d.Box.Y1));
        }

        // The zoom that fits a province into the 640 x 300 map with a margin: scale and translation (map units)
        public static (double Scale, double X, double Y) Zoom(int plate)
        {
            var box = BoxOf(plate);
            if (box == null)
                return (1, 0, 0);
            var scale = Math.Min(16, Math.Min(600 / Math.Max(box.Width, 1), 270 / Math.Max(box.Height, 1)));
            return (Math.Round(scale, 3), Math.Round(320 - scale * (box.X0 + box.X1) / 2, 1), Math.Round(150 - scale * (box.Y0 + box.Y1) / 2, 1));
        }

        // "x y w h" of a province with a margin of 4 %, for a small map of one province
        public static string? ViewBoxOf(int plate)
        {
            var box = BoxOf(plate);
            if (box == null)
                return null;
            var pad = Math.Max(box.Width, box.Height) * 0.04;
            string F(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);
            return $"{F(box.X0 - pad)} {F(box.Y0 - pad)} {F(box.Width + 2 * pad)} {F(box.Height + 2 * pad)}";
        }

        private static bool Inside(District d, double x, double y)
        {
            if (x < d.Box.X0 || x > d.Box.X1 || y < d.Box.Y0 || y > d.Box.Y1)
                return false;
            var inside = false;
            foreach (var ring in d.Rings)
                for (int i = 0, j = ring.Length - 1; i < ring.Length; j = i++)
                    if ((ring[i].Y > y) != (ring[j].Y > y) && x < (ring[j].X - ring[i].X) * (y - ring[i].Y) / (ring[j].Y - ring[i].Y) + ring[i].X)
                        inside = !inside;
            return inside;
        }

        private static Dictionary<int, IReadOnlyList<District>> Load()
        {
            using var stream = typeof(TurkeyDistricts).Assembly.GetManifestResourceStream(Resource)
                ?? throw new InvalidOperationException($"Embedded resource {Resource} is missing");
            using var doc = JsonDocument.Parse(stream);
            var result = new Dictionary<int, IReadOnlyList<District>>();
            foreach (var province in doc.RootElement.GetProperty("provinces").EnumerateObject())
            {
                var plate = int.Parse(province.Name, CultureInfo.InvariantCulture);
                result[plate] = province.Value.EnumerateArray().Select(e =>
                {
                    var path = e.GetProperty("d").GetString()!;
                    var rings = Rings(path);
                    var all = rings.SelectMany(r => r).ToList();
                    return new District(plate, e.GetProperty("name").GetString()!, path, e.GetProperty("cx").GetDouble(), e.GetProperty("cy").GetDouble(),
                        rings, new Box(all.Min(p => p.X), all.Min(p => p.Y), all.Max(p => p.X), all.Max(p => p.Y)));
                }).ToList();
            }
            return result;
        }

        [GeneratedRegex(@"-?(?:\d+\.?\d*|\.\d+)")]
        private static partial Regex Number();

        // "M12.3 45.6l.4-1.2 1.3…z" (as written by scripts/turkey-districts.py): an absolute start, then relative steps
        private static List<(double X, double Y)[]> Rings(string path) =>
            path.Split('z', StringSplitOptions.RemoveEmptyEntries).Select(ring =>
            {
                var n = Number().Matches(ring).Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture)).ToArray();
                var points = new (double X, double Y)[n.Length / 2];
                double x = 0, y = 0;
                for (var i = 0; i < points.Length; i++)
                {
                    x = Math.Round(x + n[2 * i], 1);
                    y = Math.Round(y + n[2 * i + 1], 1);
                    points[i] = (x, y);
                }
                return points;
            }).Where(r => r.Length >= 3).ToList();
    }
}
