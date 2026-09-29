using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    public class RawHoneyCCPManageViewModel
    {
        public int RawHoneyProductionId { get; set; }

        public List<int> SelectedCCPIds { get; set; } = new();

        public List<CriticalControlPoint> AllCCPs { get; set; } = new();
    }
}
