using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-78 P3 (O-5, O-8, D-82): when a beekeeper may enter a price, the default currency, when a public page shows the name
    public class PortalRulesTests
    {
        private static LotIntake Intake(bool containers, string? evidence) => new() { ContainersRecorded = containers, EvidenceStatus = evidence };

        [Fact]
        public void A_price_is_entered_from_the_confirmation_until_the_lot_is_in_a_finalized_batch()
        {
            Assert.Null(BeekeeperPortalService.PriceRule(Intake(true, EvidenceStatus.Accepted), locked: false));
            Assert.Null(BeekeeperPortalService.PriceRule(Intake(false, null), locked: false));   // a legacy delivery
            Assert.StartsWith("The delivery is not confirmed yet", BeekeeperPortalService.PriceRule(Intake(true, EvidenceStatus.PendingReview), locked: false));
            Assert.StartsWith("The delivery is not confirmed yet", BeekeeperPortalService.PriceRule(Intake(true, EvidenceStatus.Rejected), locked: false));
            Assert.StartsWith("The lot is in a finalized batch", BeekeeperPortalService.PriceRule(Intake(true, EvidenceStatus.Accepted), locked: true));
        }

        [Theory]
        [InlineData("TR", "TRY")]
        [InlineData("AR", "ARS")]
        [InlineData("PL", "PLN")]
        [InlineData("LV", "EUR")]
        [InlineData(null, "EUR")]
        public void The_default_currency_follows_the_origin(string? origin, string currency) => Assert.Equal(currency, BeekeeperPortalService.DefaultCurrency(origin));

        [Theory]
        [InlineData(true, false, LegacyNameMode.HideUntilConsent, true)]
        [InlineData(false, false, LegacyNameMode.HideUntilConsent, false)]
        [InlineData(false, false, LegacyNameMode.ShowUntilReverified, true)]
        [InlineData(false, true, LegacyNameMode.ShowUntilReverified, false)]
        [InlineData(true, true, LegacyNameMode.ShowUntilReverified, true)]
        public void The_name_is_shown_with_consent_and_for_legacy_beekeepers_only_in_show_mode(bool consent, bool registered, string mode, bool shown) =>
            Assert.Equal(shown, PublicNameService.ShowsName(consent, registered, mode));
    }
}
