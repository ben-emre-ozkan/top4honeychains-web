using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-86: the demo registry numbers of the partner countries are accepted only on a demo site
    public class RegistryNumberTests
    {
        [Theory]
        [InlineData("TR061234567890", false, true)]
        [InlineData("TR821234567890", false, false)]
        [InlineData("AR-DEMO-000123", false, false)]
        [InlineData("AR-DEMO-000123", true, true)]
        [InlineData("pl-demo-123456", true, true)]
        [InlineData("LV DEMO 123456", true, false)]
        [InlineData("TR-DEMO-000123", true, false)]
        [InlineData("AR-DEMO-12345", true, false)]
        [InlineData("TR061234567890", true, true)]
        public void Format(string number, bool demo, bool expected) => Assert.Equal(expected, RegistryNumber.IsWellFormed(number, demo));

        [Theory]
        [InlineData("TR061234567890", "TR")]
        [InlineData("lv-demo-000001", "LV")]
        public void Country(string number, string country) => Assert.Equal(country, RegistryNumber.CountryOf(number));
    }
}
