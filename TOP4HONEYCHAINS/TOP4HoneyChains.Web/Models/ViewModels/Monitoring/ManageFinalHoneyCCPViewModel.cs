using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    public class ManageFinalHoneyCCPViewModel
    {
        public int FinalHoneyProductionId { get; set; }

        public FinalHoneyProduction Production { get; set; } = null!;

        public List<int> SelectedCCPIds { get; set; } = new();

        public List<CriticalControlPoint> AllCCPs { get; set; } = new();
    }
}
