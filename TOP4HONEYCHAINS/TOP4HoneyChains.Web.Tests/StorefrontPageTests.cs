using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // The redesigned beekeeper page (docs/design/beekeeper-page): share of the beekeeper's honey in a batch,
    // limit bars, the map marker at the province centre and the demo-only content
    public class StorefrontPageTests
    {
        [Theory]
        [InlineData(100, 150, 67)]
        [InlineData(50, 150, 33)]
        [InlineData(60, 150, 40)]
        [InlineData(150, 150, 100)]
        [InlineData(1, 200, 1)]      // 0.5 % rounds up, a contributing beekeeper never shows 0 %
        [InlineData(0, 150, 0)]
        [InlineData(10, 0, 0)]
        [InlineData(200, 150, 100)]  // capped
        public void Share_of_the_beekeeper_in_a_batch(int beekeeperKg, int batchKg, int expected)
        {
            Assert.Equal(expected, StorefrontPage.SharePercent(beekeeperKg, batchKg));
        }

        [Fact]
        public void Limit_bar_puts_a_maximum_at_80_percent_and_a_lone_minimum_at_40_percent()
        {
            var max = StorefrontPage.LimitBar(17.2m, null, 20m)!.Value;
            Assert.Equal(80, max.Limit);
            Assert.Equal(68.8, max.Value);
            Assert.Null(max.Min);

            var min = StorefrontPage.LimitBar(12m, 8m, null)!.Value;
            Assert.Equal(40, min.Limit);
            Assert.Equal(60, min.Value);

            var range = StorefrontPage.LimitBar(5m, 2m, 10m)!.Value;
            Assert.Equal(80, range.Limit);
            Assert.Equal(16, range.Min);

            Assert.Equal(100, StorefrontPage.LimitBar(500m, null, 20m)!.Value.Value);   // far above the limit: full bar
            Assert.Null(StorefrontPage.LimitBar(5m, null, null));
        }

        [Fact]
        public void Map_projection_matches_the_design()
        {
            var (ax, ay) = ProvinceMap.ToMap(39.93, 32.85);      // Ankara
            Assert.InRange(ax, 241.5, 243.5);
            Assert.InRange(ay, 111.3, 113.3);
            var (kx, ky) = ProvinceMap.ToMap(41.087, 31.12);     // Akçakoca
            Assert.InRange(kx, 185.5, 187.5);
            Assert.InRange(ky, 61.9, 63.9);
        }

        [Fact]
        public void Every_province_has_a_centre_inside_the_map_and_a_region()
        {
            Assert.Equal(81, ProvinceMap.Count);
            for (var plate = 1; plate <= 81; plate++)
            {
                var c = ProvinceMap.ByPlate(plate)!;
                Assert.InRange(c.Latitude, 35.8, 42.2);
                Assert.InRange(c.Longitude, 25.6, 44.9);
                var (x, y) = ProvinceMap.ToMap(c.Latitude, c.Longitude);
                Assert.InRange(x, 0, 640);
                Assert.InRange(y, 0, 300);
                Assert.Contains(c.Region, ProvinceMap.Regions.All);
            }
            Assert.Null(ProvinceMap.ByPlate(0));
            Assert.Null(ProvinceMap.ByPlate(82));
        }

        [Theory]
        [InlineData("Düzce", 81)]
        [InlineData("düzce", 81)]
        [InlineData("İstanbul", 34)]
        [InlineData("Iğdır", 76)]
        [InlineData(" Ankara ", 6)]
        [InlineData("Atlantis", 0)]
        [InlineData(null, 0)]
        public void Province_plate_by_name(string? name, int plate)
        {
            Assert.Equal(plate, Provinces.PlateOf(name));
        }

        [Fact]
        public void Sample_content_only_on_the_demo_storefront()
        {
            Assert.True(StorefrontPage.IsDemo(StorefrontPage.DemoCode, "[DEMO] Arıcılar Birliği"));
            Assert.False(StorefrontPage.IsDemo(StorefrontPage.DemoCode, "Arıcılar Birliği"));
            Assert.False(StorefrontPage.IsDemo("BK-ABCDEFGH", "[DEMO] Arıcılar Birliği"));
            Assert.False(StorefrontPage.IsDemo(StorefrontPage.DemoCode, null));
            // a real code can never be the demo code: generated codes have no "0"
            Assert.Contains('0', StorefrontPage.DemoCode);
            Assert.All(Enumerable.Range(0, 500).Select(_ => StorefrontService.NewCode()), c => Assert.DoesNotContain('0', c));
        }

        [Theory]
        [InlineData("% (Percentage)", "%")]
        [InlineData("DN (Schade)", "DN")]
        [InlineData("mg/kg", "mg/kg")]
        [InlineData("°C (Celsius)", "°C")]
        [InlineData("% (Yüzde)", "%")]
        [InlineData("None", "")]
        [InlineData(null, "")]
        [InlineData("(note)", "(note)")]
        public void Public_pages_show_only_the_unit_symbol(string? unit, string symbol)
        {
            Assert.Equal(symbol, Units.Symbol(unit));
        }

        [Theory]
        [InlineData("[DEMO] Ayşe Petek", "AP")]
        [InlineData("ayşe", "AY")]
        [InlineData("İsmail Hakkı Işık", "İI")]
        [InlineData("BK-XAXAXAXA", "BX")]
        public void Initials_of_the_shown_name(string name, string expected)
        {
            Assert.Equal(expected, StorefrontPage.Initials(name));
        }
    }
}
