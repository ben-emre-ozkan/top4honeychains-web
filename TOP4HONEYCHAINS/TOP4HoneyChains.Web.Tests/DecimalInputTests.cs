using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-53: a comma or a dot is the decimal separator in every language; "1,5" and "1.5" are never 15
    public class DecimalInputTests
    {
        [Theory]
        [InlineData("1,5", "1.5")]
        [InlineData("1.5", "1.5")]
        [InlineData("15", "15")]
        [InlineData("-1,5", "-1.5")]
        [InlineData(" 12,75 ", "12.75")]
        [InlineData("0,0001", "0.0001")]
        [InlineData(",5", "0.5")]
        [InlineData("1.234,5", "1234.5")]
        [InlineData("1,234.5", "1234.5")]
        [InlineData("1.234", "1.234")]
        [InlineData("1,234", "1.234")]
        [InlineData("1.234.567", "1234567")]
        [InlineData("1 234,5", "1234.5")]
        [InlineData("1 234,5", "1234.5")]
        public void Parses(string text, string expected)
        {
            Assert.True(DecimalInput.TryParse(text, out var value), text);
            Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("1,2,3")]
        [InlineData("1.2.3")]
        [InlineData("1,23.45,6")]
        [InlineData("12.34,5.6")]
        [InlineData("1e3")]
        [InlineData(null)]
        public void Rejects(string? text) => Assert.False(DecimalInput.TryParse(text, out _), text);

        [Fact]
        public void Never_reads_a_separator_as_a_thousands_group_of_one_decimal()
        {
            foreach (var text in new[] { "1,5", "1.5" })
                foreach (var culture in new[] { "tr-TR", "en-US", "es-ES", "pl-PL", "lv-LV", "de-DE" })
                {
                    var before = System.Globalization.CultureInfo.CurrentCulture;
                    System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(culture);
                    try
                    {
                        Assert.True(DecimalInput.TryParse(text, out var value));
                        Assert.Equal(1.5m, value);
                    }
                    finally
                    {
                        System.Globalization.CultureInfo.CurrentCulture = before;
                    }
                }
        }
    }
}
