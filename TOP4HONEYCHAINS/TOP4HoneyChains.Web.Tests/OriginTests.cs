using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-31 (EU 2024/1438): country percentages of a batch by weight
    public class OriginTests
    {
        [Fact]
        public void Single_country_is_100_percent()
        {
            var s = Origin.Compute(new (string?, decimal)[] { ("TR", 40), ("TR", 60) });
            Assert.Single(s);
            Assert.Equal(("TR", 100m, 100m), (s[0].Country, s[0].Amount, s[0].Percentage));
        }

        [Fact]
        public void Lot_without_country_counts_as_Turkiye() =>
            Assert.Equal("TR", Assert.Single(Origin.Compute(new (string?, decimal)[] { (null, 10), ("", 5) })).Country);

        [Fact]
        public void Shares_are_largest_first_and_add_up_to_exactly_100()
        {
            var s = Origin.Compute(new (string?, decimal)[] { ("UA", 1), ("TR", 1), ("BG", 1) });
            Assert.Equal(100m, s.Sum(x => x.Percentage));
            Assert.Equal(new[] { 33.34m, 33.33m, 33.33m }, s.Select(x => x.Percentage));
            Assert.True(s.Zip(s.Skip(1)).All(p => p.First.Percentage >= p.Second.Percentage));
        }

        [Fact]
        public void Two_thirds_and_one_third()
        {
            var s = Origin.Compute(new (string?, decimal)[] { ("UA", 50), ("TR", 100) });
            Assert.Equal(new[] { ("TR", 66.67m), ("UA", 33.33m) }, s.Select(x => (x.Country, x.Percentage)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void No_positive_weight_gives_no_shares(int amount) =>
            Assert.Empty(Origin.Compute(new (string?, decimal)[] { ("TR", amount) }));

        [Fact]
        public void Country_codes_are_normalized() =>
            Assert.Equal("UA", Assert.Single(Origin.Compute(new (string?, decimal)[] { (" ua ", 3) })).Country);
    }
}
