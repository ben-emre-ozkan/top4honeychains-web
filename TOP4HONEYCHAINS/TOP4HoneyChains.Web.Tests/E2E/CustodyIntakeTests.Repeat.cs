using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-89: an apiary may deliver several times in a period, each delivery its own lot; the same honey type from the same apiary on
    // the same day is pointed out on the form, never refused.
    public partial class CustodyIntakeTests
    {
        // the same delivery with other seal numbers (seals are unique per organization while active)
        private static Dictionary<string, string> Reseal(Dictionary<string, string> f, string suffix)
        {
            var copy = new Dictionary<string, string>(f);
            foreach (var key in f.Keys.Where(k => k.EndsWith("Seal", StringComparison.Ordinal) || k.EndsWith(".Reseal", StringComparison.Ordinal)))
                copy[key] = f[key] + suffix;
            return copy;
        }

        [Fact]
        public async Task An_apiary_delivers_several_times_in_a_period_and_the_same_day_is_only_pointed_out() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("cr1");
            var first = await ConfirmBySmsAsync(p, Form(p));

            // the same apiary, honey type and period on the same day: a warning on the form, and the delivery becomes its own lot
            var second = Reseal(Form(p), "b");
            var html = await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(second, "send"), $"/Intake/Create?number={p.BusinessNumber}"));
            Assert.Contains("id=\"intake-confirm\"", html);
            Assert.Contains("id=\"same-day-warning\"", html);
            Assert.Contains($"Lot #{first.LotId} is a delivery of the same honey type from this apiary on the same day. Check that this is another delivery.", html);
            Assert.DoesNotContain("already has a production record", html);
            var again = await ConfirmBySmsAsync(p, second);
            Assert.NotEqual(first.LotId, again.LotId);

            // another day of the same period: no warning
            var third = Reseal(Form(p), "c");
            third["DeliveryDate"] = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            html = await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(third, "send"), $"/Intake/Create?number={p.BusinessNumber}"));
            Assert.Contains("id=\"intake-confirm\"", html);
            Assert.DoesNotContain("same-day-warning", html);

            Assert.Equal(2, await _app.WithContextAsync(db => db.RawHoneyProductions.CountAsync(r =>
                r.RawHoneyProductionApiaryId == p.ApiaryId && r.RawHoneyProductionPeriodId == p.PeriodId && r.RawHoneyProductionHoneyType == p.HoneyTypeId)));
            Assert.Equal(2, await _app.WithContextAsync(db => db.LotIntakes.CountAsync(i => i.RawHoneyProductionId == first.LotId || i.RawHoneyProductionId == again.LotId)));
            return true;
        });
    }
}
