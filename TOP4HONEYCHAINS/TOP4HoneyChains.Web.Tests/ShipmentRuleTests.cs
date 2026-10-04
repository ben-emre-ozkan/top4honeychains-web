using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // P3 rule (D-41): only a finalized batch can be shipped, by its holder, to another verified organization,
    // for at most the quantity still available
    public class ShipmentRuleTests
    {
        private const string Draft = "Only a finalized batch can be shipped.";

        [Fact]
        public void Draft_batch_cannot_open_the_shipment_form() =>
            Assert.Equal(Draft, CustodyService.DraftBatchError(batchFinalized: false));

        [Fact]
        public void Finalized_batch_can_open_the_shipment_form() =>
            Assert.Null(CustodyService.DraftBatchError(batchFinalized: true));

        [Fact]
        public void Draft_batch_cannot_be_shipped_even_with_valid_data() =>
            Assert.Equal(Draft, CustodyService.CheckShipment(false, fromOrgId: 1, toOrgId: 2, receiverVerified: true, quantity: 10, available: 100, note: null));

        [Fact]
        public void Draft_rule_is_checked_before_the_quantity() =>
            Assert.Equal(Draft, CustodyService.CheckShipment(false, 1, 2, true, quantity: 1000, available: 1, note: null));

        [Fact]
        public void Finalized_batch_within_the_available_quantity_can_be_shipped() =>
            Assert.Null(CustodyService.CheckShipment(true, 1, 2, true, quantity: 100, available: 100, note: "ok"));

        [Fact]
        public void More_than_available_is_refused() =>
            Assert.Equal("You can ship at most 70 kg of this batch.", CustodyService.CheckShipment(true, 1, 2, true, 70.01m, 70, null));

        [Theory]
        [InlineData(1, true)]    // to itself
        [InlineData(2, false)]   // to an unverified organization
        public void Shipping_to_itself_or_an_unverified_org_is_refused(int toOrgId, bool verified) =>
            Assert.Equal("Choose another verified organization as receiver.", CustodyService.CheckShipment(true, 1, toOrgId, verified, 1, 10, null));

        [Theory]
        [InlineData(0)]
        [InlineData(null)]
        public void Zero_or_missing_quantity_is_refused(int? quantity) =>
            Assert.Equal("The amount must be greater than 0.", CustodyService.CheckShipment(true, 1, 2, true, quantity, 10, null));
    }
}
