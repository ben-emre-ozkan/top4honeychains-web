using System.Globalization;
using System.Text.Json;

namespace TOP4HoneyChains.Web.Services
{
    // D-72: the 81 provinces of Türkiye as SVG paths on the 640 x 300 map of ProvinceMap.ToMap (Natural Earth admin-1,
    // public domain; Data/Geo/README.md). Used by the shared map partial (_TurkeyMap) and to place a point in a province
    // on the server, so that pages show provinces only and never a coordinate.
    public static class TurkeyMap
    {
        public record Province(int Plate, string Name, string Path, double Cx, double Cy, IReadOnlyList<(double X, double Y)[]> Rings);

        private const string Resource = "TOP4HoneyChains.Web.turkey-provinces.json";
        private const double NearestWithin = 12;   // map units (about 30 km): points just off the simplified coast or border
        private static readonly Lazy<(string ViewBox, string Outline, IReadOnlyList<Province> Provinces)> Data = new(Load);

        public static string ViewBox => Data.Value.ViewBox;
        public static string Outline => Data.Value.Outline;
        public static IReadOnlyList<Province> Provinces => Data.Value.Provinces;

        public static Province? ByPlate(int plate) => plate is >= 1 and <= 81 ? Provinces[plate - 1] : null;

        // The province of a WGS84 point: inside a province path, else the nearest centroid within a short distance; 0 when unknown
        public static int PlateAt(double? latitude, double? longitude)
        {
            if (latitude is not { } lat || longitude is not { } lon || lat is < 34 or > 44 || lon is < 24 or > 47)
                return 0;
            var (x, y) = ProvinceMap.ToMap(lat, lon);
            foreach (var p in Provinces)
                if (Inside(p, x, y))
                    return p.Plate;
            var nearest = Provinces.Select(p => (p.Plate, D: Distance(p, x, y))).MinBy(t => t.D);
            return nearest.D <= NearestWithin ? nearest.Plate : 0;
        }

        // shortest distance from the point to the province's border
        private static double Distance(Province p, double x, double y)
        {
            var best = double.MaxValue;
            foreach (var ring in p.Rings)
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

        // even-odd rule over every ring of the province (islands and holes included)
        private static bool Inside(Province p, double x, double y)
        {
            var inside = false;
            foreach (var ring in p.Rings)
                for (int i = 0, j = ring.Length - 1; i < ring.Length; j = i++)
                    if ((ring[i].Y > y) != (ring[j].Y > y) && x < (ring[j].X - ring[i].X) * (y - ring[i].Y) / (ring[j].Y - ring[i].Y) + ring[i].X)
                        inside = !inside;
            return inside;
        }

        private static (string, string, IReadOnlyList<Province>) Load()
        {
            using var stream = typeof(TurkeyMap).Assembly.GetManifestResourceStream(Resource)
                ?? throw new InvalidOperationException($"Embedded resource {Resource} is missing");
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;
            var provinces = root.GetProperty("provinces").EnumerateArray().Select(e =>
            {
                var plate = int.Parse(e.GetProperty("code").GetString()!, CultureInfo.InvariantCulture);
                var path = e.GetProperty("d").GetString()!;
                return new Province(plate, Services.Provinces.ByPlate(plate)!, path,
                    e.GetProperty("cx").GetDouble(), e.GetProperty("cy").GetDouble(), Rings(path));
            }).OrderBy(p => p.Plate).ToList();
            if (provinces.Count != 81 || provinces.Select((p, i) => p.Plate == i + 1).Any(ok => !ok))
                throw new InvalidOperationException("turkey-provinces.json must hold the plates 01-81");
            return (root.GetProperty("viewBox").GetString()!, root.GetProperty("outline").GetString()!, provinces);
        }

        // "M1 2L3 4L5 6ZM…" (absolute M/L/Z only, as written by scripts/turkey-map.py)
        private static List<(double X, double Y)[]> Rings(string path) =>
            path.Split('Z', StringSplitOptions.RemoveEmptyEntries)
                .Select(ring => ring.Split(new[] { 'M', 'L' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(pt => pt.Split(' '))
                    .Select(xy => (double.Parse(xy[0], CultureInfo.InvariantCulture), double.Parse(xy[1], CultureInfo.InvariantCulture)))
                    .ToArray())
                .Where(r => r.Length >= 3)
                .ToList();
    }
}
