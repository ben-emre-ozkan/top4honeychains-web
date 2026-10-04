using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-53: a decimal typed with a comma or a dot is stored as typed, in Turkish (dot = thousands there) and English
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class DecimalBindingTests
    {
        private readonly E2EApp _app;

        public DecimalBindingTests(E2EApp app) => _app = app;

        [Fact]
        public async Task Amounts_with_comma_or_dot_are_stored_as_typed()
        {
            var (apiaryId, wholesalerId, period, types) = await _app.WithContextAsync(async db =>
            {
                var w = new Wholesaler { WholesalerTitle = "DEC Wholesaler", WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(w);
                var bk = new Beekeeper { BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "DEC", BeekeeperLastName = "Beekeeper", BeekeeperBirthDate = new DateOnly(1980, 1, 1), BeekeeperBusinessNumber = "DEC-BEE-1", IsActive = true };
                db.Beekeepers.Add(bk);
                await db.SaveChangesAsync();
                var ap = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId, ApiaryTitle = "DEC Apiary", ApiaryBeehivesCount = 3, ApiaryQRCode = Guid.NewGuid() };
                db.Apiaries.Add(ap);
                await db.SaveChangesAsync();
                return (ap.ApiaryId, w.WholesalerId, await db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).FirstAsync(),
                    await db.HoneyTypes.OrderBy(h => h.HoneyTypeId).Select(h => h.HoneyTypeId).Take(3).ToListAsync());
            });
            await _app.CreateUserAsync("e2e_dec_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "e2e_dec_admin");

            async Task<decimal?> PostAsync(string culture, string amount, int honeyType)
            {
                await admin.PostAsync($"/RawHoneyProductions/Create?culture={culture}", new()
                {
                    ["RawHoneyProductionApiaryId"] = apiaryId.ToString(), ["RawHoneyProductionPeriodId"] = period.ToString(),
                    ["RawHoneyProductionHoneyType"] = honeyType.ToString(), ["RawHoneyProductionWholesalerId"] = wholesalerId.ToString(),
                    ["RawHoneyProductionAmount"] = amount, ["RawHoneyProductionDate"] = DateTime.Today.ToString("yyyy-MM-dd"), ["OriginCountry"] = "TR"
                }, $"/RawHoneyProductions/Create?apiaryId={apiaryId}");
                return await _app.WithContextAsync(db => db.RawHoneyProductions
                    .Where(r => r.RawHoneyProductionApiaryId == apiaryId && r.RawHoneyProductionHoneyType == honeyType)
                    .Select(r => r.RawHoneyProductionAmount).FirstOrDefaultAsync());
            }

            Assert.Equal(1.5m, await PostAsync("tr", "1,5", types[0]));
            Assert.Equal(2.5m, await PostAsync("tr", "2.5", types[1]));   // not 25: Turkish groups thousands with a dot
            Assert.Equal(3.5m, await PostAsync("en", "3,5", types[2]));   // not 35: English groups thousands with a comma
        }
    }
}
