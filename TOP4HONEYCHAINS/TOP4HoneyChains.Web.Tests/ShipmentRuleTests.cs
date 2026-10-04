using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Tests
{
    // P3 rule (D-41): only a finalized batch can be shipped, by its holder, to another verified organization,
    // for at most the quantity still available
    [Tests]
    public static class ShipmentRuleTests
    {
        private const string Draft = "Only a finalized batch can be shipped.";

        public static void Draft_batch_cannot_open_the_shipment_form() =>
            Assert.Equal(Draft, CustodyService.DraftBatchError(batchFinalized: false));

        public static void Finalized_batch_can_open_the_shipment_form() =>
            Assert.Null(CustodyService.DraftBatchError(batchFinalized: true));

        public static void Draft_batch_cannot_be_shipped_even_with_valid_data() =>
            Assert.Equal(Draft, CustodyService.CheckShipment(false, fromOrgId: 1, toOrgId: 2, receiverVerified: true, quantity: 10, available: 100, note: null));

        public static void Draft_rule_is_checked_before_the_quantity() =>
            Assert.Equal(Draft, CustodyService.CheckShipment(false, 1, 2, true, quantity: 1000, available: 1, note: null));

        public static void Finalized_batch_within_the_available_quantity_can_be_shipped() =>
            Assert.Null(CustodyService.CheckShipment(true, 1, 2, true, quantity: 100, available: 100, note: "ok"));

        public static void More_than_available_is_refused() =>
            Assert.Equal("You can ship at most 70 kg of this batch.", CustodyService.CheckShipment(true, 1, 2, true, 70.01m, 70, null));

        public static void Shipping_to_itself_or_an_unverified_org_is_refused()
        {
            Assert.Equal("Choose another verified organization as receiver.", CustodyService.CheckShipment(true, 1, 1, true, 1, 10, null), "self");
            Assert.Equal("Choose another verified organization as receiver.", CustodyService.CheckShipment(true, 1, 2, false, 1, 10, null), "unverified");
        }

        public static void Zero_or_missing_quantity_is_refused()
        {
            Assert.NotNull(CustodyService.CheckShipment(true, 1, 2, true, 0, 10, null), "zero");
            Assert.NotNull(CustodyService.CheckShipment(true, 1, 2, true, null, 10, null), "missing");
        }
    }
}
