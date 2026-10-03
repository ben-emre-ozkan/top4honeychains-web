using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    public class ManageFinalHoneyCCPViewModel
    {
        public int FinalHoneyProductionId { get; set; }

        public FinalHoneyProduction Production { get; set; } = null!;

        public List<int> SelectedCCPIds { get; set; } = new();

        public List<CriticalControlPoint> AllCCPs { get; set; } = new();

        // Set when the batch is finalized (D-14); the page is then read-only
        public string? LockReason { get; set; }
    }
}
