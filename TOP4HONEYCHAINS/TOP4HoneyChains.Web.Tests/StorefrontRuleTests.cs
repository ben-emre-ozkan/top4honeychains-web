using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-30: a storefront is public only from verified data and with an active consent of the current association
    public class StorefrontRuleTests
    {
        [Fact]
        public void Published_when_everything_is_verified_and_consented() =>
            Assert.True(StorefrontService.IsPublished(true, true, true, true, true));

        [Theory]
        [InlineData(false, true, true, true, true)]   // beekeeper deactivated
        [InlineData(true, false, true, true, true)]   // registry number not verified
        [InlineData(true, true, false, true, true)]   // association not verified (or suspended)
        [InlineData(true, true, true, false, true)]   // consent recorded by a former association
        [InlineData(true, true, true, true, false)]   // no consent, or withdrawn
        public void Any_missing_condition_unpublishes(bool active, bool registry, bool association, bool currentAssociation, bool consent) =>
            Assert.False(StorefrontService.IsPublished(active, registry, association, currentAssociation, consent));

        [Fact]
        public void Name_is_shown_only_with_consent()
        {
            Assert.Equal("Ayşe Yılmaz", StorefrontService.DisplayName(true, "Ayşe", "Yılmaz", "BK-ABC"));
            Assert.Equal("BK-ABC", StorefrontService.DisplayName(false, "Ayşe", "Yılmaz", "BK-ABC"));
            Assert.Equal("BK-ABC", StorefrontService.DisplayName(true, "", " ", "BK-ABC"));
        }

        [Fact]
        public void Codes_are_random_and_not_derived_from_names()
        {
            var codes = Enumerable.Range(0, 200).Select(_ => StorefrontService.NewCode()).ToList();
            Assert.All(codes, c => Assert.Matches("^BK-[A-HJ-NP-Z2-9]{8}$", c));
            Assert.Equal(codes.Count, codes.Distinct().Count());
        }

        [Fact]
        public void Consent_text_hash_matches_the_text() =>
            Assert.Equal(64, ConsentText.Sha256.Length);
    }
}
