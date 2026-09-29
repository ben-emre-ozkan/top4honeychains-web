using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.FinalProduct
{
    public class RawHoneyBatchItemViewModel
    {
        public RawHoneyProduction RawHoney { get; set; } = null!;

        public decimal AlreadyUsed { get; set; }

        public decimal AvailableAmount { get; set; }

        public decimal CurrentBatchAmount { get; set; }
    }
}
