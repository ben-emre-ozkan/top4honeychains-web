using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.FinalProduct
{
    public class ManageBatchViewModel
    {
        public FinalHoneyProduction Batch { get; set; } = null!;
        public Wholesaler? Wholesaler { get; set; }

        public List<RawHoneyBatchItemViewModel> RawHoneyList { get; set; } = new();

        // Lots of the same wholesaler that are not eligible yet, with the missing CCP checks
        public List<IneligibleRawHoneyItemViewModel> IneligibleRawHoneyList { get; set; } = new();

        // What still prevents finalizing (D-16); empty = the batch can be finalized
        public List<string> FinalizeBlockers { get; set; } = new();
    }
}
