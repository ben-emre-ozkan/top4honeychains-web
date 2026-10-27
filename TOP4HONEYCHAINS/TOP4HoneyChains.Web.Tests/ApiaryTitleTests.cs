using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-78 P1 (F5, D78-SPEC 3.6): generated apiary titles
    public class ApiaryTitleTests
    {
        [Fact]
        public void The_title_is_province_district_locality()
        {
            Assert.Equal("Giresun – Dereli – Kümbet Yaylası", ApiaryTitles.Compose("Giresun", "Dereli", " Kümbet Yaylası "));
            Assert.Equal("Giresun – Dereli – Kümbet Yaylası 2", ApiaryTitles.Compose("Giresun", "Dereli", "Kümbet Yaylası", 2));
        }

        [Fact]
        public void A_long_locality_is_shortened_to_fit_fifty_characters()
        {
            var t = ApiaryTitles.Compose("Kahramanmaraş", "Dulkadiroğlu", "Ahır Dağı Eteği Büyük Çam Ormanı Yanındaki Tarla");
            Assert.True(t.Length <= 50, t);
            Assert.StartsWith("Kahramanmaraş – Dulkadiroğlu – Ahır", t);
            Assert.EndsWith("…", t);
            var numbered = ApiaryTitles.Compose("Kahramanmaraş", "Dulkadiroğlu", "Ahır Dağı Eteği Büyük Çam Ormanı Yanındaki Tarla", 12);
            Assert.True(numbered.Length <= 50, numbered);
            Assert.EndsWith("… 12", numbered);
        }
    }
}
