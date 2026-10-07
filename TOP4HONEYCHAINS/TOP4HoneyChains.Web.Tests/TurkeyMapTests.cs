using TOP4HoneyChains.Web.Models.ViewModels;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-72: the shared Türkiye map (Natural Earth admin-1, Data/Geo/turkey-provinces.json) and the province of a point
    public class TurkeyMapTests
    {
        [Fact]
        public void The_map_holds_the_81_provinces_with_their_names_paths_and_centroids()
        {
            Assert.Equal("0 0 640 300", TurkeyMap.ViewBox);
            Assert.StartsWith("M", TurkeyMap.Outline);
            Assert.Equal(81, TurkeyMap.Provinces.Count);
            for (var plate = 1; plate <= 81; plate++)
            {
                var p = TurkeyMap.ByPlate(plate)!;
                Assert.Equal(plate, p.Plate);
                Assert.Equal(Provinces.ByPlate(plate), p.Name);
                Assert.StartsWith("M", p.Path);
                Assert.NotEmpty(p.Rings);
                Assert.InRange(p.Cx, 0, 640);
                Assert.InRange(p.Cy, 0, 300);
            }
            Assert.Null(TurkeyMap.ByPlate(0));
            Assert.Null(TurkeyMap.ByPlate(82));
        }

        [Fact]
        public void Every_provincial_capital_lies_in_its_own_province()
        {
            for (var plate = 1; plate <= 81; plate++)
            {
                var c = ProvinceMap.ByPlate(plate)!;
                Assert.True(plate == TurkeyMap.PlateAt(c.Latitude, c.Longitude), $"{Provinces.ByPlate(plate)} ({plate}) placed in {TurkeyMap.PlateAt(c.Latitude, c.Longitude)}");
            }
        }

        [Theory]
        [InlineData(40.512345, 31.204567, 14)]   // Bolu, inland
        [InlineData(37.03, 27.43, 48)]           // Bodrum, Muğla
        [InlineData(39.0, 35.0, 0)]              // inland, central Anatolia: some province
        public void A_point_is_placed_in_a_province(double lat, double lon, int expected)
        {
            if (expected == 0)
                Assert.InRange(TurkeyMap.PlateAt(lat, lon), 1, 81);
            else
                Assert.Equal(expected, TurkeyMap.PlateAt(lat, lon));
        }

        [Theory]
        [InlineData(null, 32.0)]
        [InlineData(39.0, null)]
        [InlineData(0.0, 0.0)]          // a missing position stored as zero
        [InlineData(48.85, 2.35)]       // Paris
        [InlineData(35.4, 33.0)]        // open sea, 70 km south of Anamur
        public void A_point_outside_Turkiye_has_no_province(double? lat, double? lon)
        {
            Assert.Equal(0, TurkeyMap.PlateAt(lat, lon));
        }

        [Fact]
        public void Honey_shades_are_relative_to_the_largest_value()
        {
            var map = new TurkeyMapModel { Areas = { new(6, 10, ""), new(34, 5, ""), new(35, 1, "") } };
            Assert.Equal(5, map.Level(10));
            Assert.Equal(3, map.Level(5));
            Assert.Equal(1, map.Level(1));
            Assert.Equal(0, map.Level(0));
            Assert.Equal(0, new TurkeyMapModel().Level(3));
        }
    }
}
