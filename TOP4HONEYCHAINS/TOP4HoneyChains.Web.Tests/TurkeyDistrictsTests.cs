using System.Text.RegularExpressions;
using TOP4HoneyChains.Web.Models.ViewModels;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Tests.Demo;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-98: the district map (geoBoundaries ADM2, Data/Geo/turkey-districts.json) covers the Districts table and lines up
    // with the province map
    public class TurkeyDistrictsTests
    {
        // the district list as seeded by 011 (the Districts table): plate -> names
        private static Dictionary<int, List<string>> Seeded()
        {
            var sql = File.ReadAllText(DemoReference.RepoFile("db", "migrations", "011_provinces_districts.sql"));
            return Regex.Matches(sql, @"SELECT '(\d\d)', v\.Name FROM \(VALUES (.*?)\) v\(Name\)", RegexOptions.Singleline)
                .ToDictionary(m => int.Parse(m.Groups[1].Value), m => Regex.Matches(m.Groups[2].Value, @"N'((?:[^']|'')*)'").Select(n => n.Groups[1].Value.Replace("''", "'")).ToList());
        }

        [Fact]
        public void Every_district_of_the_table_has_an_outline_in_its_province_and_nothing_else()
        {
            var seeded = Seeded();
            Assert.Equal(973, seeded.Values.Sum(v => v.Count));
            Assert.Equal(973, TurkeyDistricts.Count);
            for (var plate = 1; plate <= 81; plate++)
            {
                var districts = TurkeyDistricts.Of(plate);
                Assert.Equal(seeded[plate].OrderBy(n => n, StringComparer.Ordinal), districts.Select(d => d.Name).OrderBy(n => n, StringComparer.Ordinal));
                foreach (var d in districts)
                {
                    Assert.Equal(plate, d.Plate);
                    Assert.StartsWith("M", d.Path);
                    Assert.NotEmpty(d.Rings);
                    Assert.InRange(d.Cx, 0, 640);
                    Assert.InRange(d.Cy, 0, 300);
                }
            }
            Assert.Empty(TurkeyDistricts.Of(0));
            Assert.Empty(TurkeyDistricts.Of(82));
        }

        [Fact]
        public void The_districts_line_up_with_the_province_map()
        {
            for (var plate = 1; plate <= 81; plate++)
            {
                var province = TurkeyMap.ByPlate(plate)!;
                // a margin of 12 map units (about 30 km): islands such as Marmara are missing from the simplified province map
                var points = province.Rings.SelectMany(r => r).ToList();
                var (x0, y0, x1, y1) = (points.Min(p => p.X) - 12, points.Min(p => p.Y) - 12, points.Max(p => p.X) + 12, points.Max(p => p.Y) + 12);
                foreach (var d in TurkeyDistricts.Of(plate))
                    Assert.True(d.Cx >= x0 && d.Cx <= x1 && d.Cy >= y0 && d.Cy <= y1, $"{d.Name}, {province.Name} lies outside its province");
                var box = TurkeyDistricts.BoxOf(plate)!;
                Assert.InRange((box.X0 + box.X1) / 2, x0, x1);
                Assert.InRange((box.Y0 + box.Y1) / 2, y0, y1);
            }
        }

        [Fact]
        public void A_district_is_found_by_its_name_within_its_province_only()
        {
            Assert.Equal("Akçakoca", TurkeyDistricts.Find(81, "AKÇAKOCA")!.Name);
            Assert.Equal("İnegöl", TurkeyDistricts.Find(16, " inegöl ")!.Name);
            Assert.Equal("Merkez", TurkeyDistricts.Find(81, "merkez")!.Name);
            Assert.Null(TurkeyDistricts.Find(81, "İnegöl"));
            Assert.Null(TurkeyDistricts.Find(81, null));
            Assert.Null(TurkeyDistricts.Find(0, "Merkez"));
        }

        [Fact]
        public void A_point_is_placed_in_a_district_of_its_province_on_the_server()
        {
            var placed = 0;
            for (var plate = 1; plate <= 81; plate++)
            {
                var c = ProvinceMap.ByPlate(plate)!;
                if (TurkeyDistricts.At(plate, c.Latitude, c.Longitude) is { } d)
                {
                    Assert.Equal(plate, d.Plate);
                    placed++;
                }
            }
            Assert.True(placed >= 78, $"only {placed} provincial capitals lie in a district");
            Assert.Null(TurkeyDistricts.At(81, null, 31.1));
            Assert.Null(TurkeyDistricts.At(81, 48.85, 2.35));
        }

        [Fact]
        public void The_zoom_fits_the_province_into_the_map()
        {
            for (var plate = 1; plate <= 81; plate++)
            {
                var (s, tx, ty) = TurkeyDistricts.Zoom(plate);
                var box = TurkeyDistricts.BoxOf(plate)!;
                Assert.InRange(s, 1, 16);
                Assert.InRange(tx + s * box.X0, -0.5, 640.5);
                Assert.InRange(tx + s * box.X1, -0.5, 640.5);
                Assert.InRange(ty + s * box.Y0, -0.5, 300.5);
                Assert.InRange(ty + s * box.Y1, -0.5, 300.5);
                Assert.NotNull(TurkeyDistricts.ViewBoxOf(plate));
            }
            Assert.Equal((1, 0, 0), TurkeyDistricts.Zoom(0));
        }

        [Fact]
        public void District_shades_are_relative_to_the_largest_district_value()
        {
            var map = new TurkeyMapModel { DistrictAreas = { new(81, "Akçakoca", 4, ""), new(81, "Merkez", 1, "") } };
            Assert.Equal(5, map.DistrictLevel(4));
            Assert.Equal(2, map.DistrictLevel(1));
            Assert.Equal(0, new TurkeyMapModel().DistrictLevel(1));
        }
    }
}
