using System.Globalization;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-94: interface fixes found while writing the user manual: British dates and the UK flag for English,
    // only the applying limit for a known honey, phone numbers shown with spaces
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class D94InterfaceTests
    {
        private readonly E2EApp _app;

        public D94InterfaceTests(E2EApp app) => _app = app;

        [Fact]
        public async Task English_uses_british_dates_and_the_uk_flag()
        {
            const string name = "e2e_d94_admin";
            await _app.CreateUserAsync(name, "Administrator");
            var admin = await UserClient.LoginAsync(_app, name);
            var response = await admin.GetAsync("/Dashboard/Index?culture=en");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            var today = DateTime.Today;
            Assert.Contains($"{today.Day} {today.ToString("MMMM", CultureInfo.InvariantCulture)} {today.Year}", html);
            Assert.DoesNotContain(today.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture), html);
            Assert.Contains("flags/uk_flag.svg", html);
            Assert.DoesNotContain("us_flag", html);
        }

        [Fact]
        public void A_known_honey_shows_only_the_main_limit_that_applies()
        {
            using var scope = _app.Services.CreateScope();
            var formatter = scope.ServiceProvider.GetRequiredService<LimitFormatter>();
            var parameter = new CriticalControlPointParameter
            {
                ParameterId = 990094,
                ParameterName = "D94 conductivity",
                ParameterUnit = "mS/cm",
                Limits = new List<CriticalControlPointParameterLimit>
                {
                    new() { LimitId = 1, HoneyClass = "Floral", MaxValue = 0.8m },
                    new() { LimitId = 2, HoneyClass = "Honeydew", MinValue = 0.8m },
                    new() { LimitId = 3, HoneyTypeId = 990006, MinValue = 0.8m },
                },
            };
            var before = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("en");
            try
            {
                // chestnut (a honey type with its own row, in the floral class): only the type's row
                Assert.Equal("≥ 0.8 mS/cm", formatter.Describe(parameter, new HoneyProfile(990006, "Floral")));
                // another floral honey: the class row
                Assert.Equal("≤ 0.8 mS/cm", formatter.Describe(parameter, new HoneyProfile(990001, "Floral")));
                // no honey known (the parameter list): every row with its scope
                var all = formatter.Describe(parameter)!;
                Assert.Contains("≤ 0.8 mS/cm", all);
                Assert.Contains("≥ 0.8 mS/cm", all);
            }
            finally
            {
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = before;
            }
        }

        [Fact]
        public void Phone_numbers_are_shown_with_spaces()
        {
            Assert.Equal("+90 555 000 08 02", PhoneNumberPolicy.Display("+905550000802"));
        }
    }
}
