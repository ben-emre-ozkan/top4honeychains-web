using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.FinalProduct
{
    public class ManageBatchViewModel
    {
        public FinalHoneyProduction Batch { get; set; } = null!;
        public Wholesaler? Wholesaler { get; set; }

        public List<RawHoneyBatchItemViewModel> RawHoneyList { get; set; } = new();
    }
}
