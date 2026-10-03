using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    public class ManageRawHoneyCCPViewModel
    {
        public int RawHoneyProductionId { get; set; }

        public int ApiaryId { get; set; }

        public RawHoneyProduction Production { get; set; } = null!;

        public List<int> SelectedCCPIds { get; set; } = new();

        public List<CriticalControlPoint> AllCCPs { get; set; } = new();

        // Set when the lot is used in a finalized batch (D-14); the page is then read-only
        public string? LockReason { get; set; }
    }

}
