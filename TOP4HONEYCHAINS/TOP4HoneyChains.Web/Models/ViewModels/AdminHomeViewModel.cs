using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // D-72: the admin home (Views/Dashboard/Index.cshtml)
    public class AdminHomeViewModel
    {
        public string UserName { get; init; } = "";
        public bool OrgModel { get; init; }
        public AdminHomeFigures Figures { get; init; } = null!;
        public List<AdminActivity> Activity { get; init; } = new();
        public List<AdminWaiting> Waiting { get; init; } = new();   // the oldest few
        public int WaitingTotal { get; init; }
        public IReadOnlyList<LandingImage> Steps { get; init; } = Array.Empty<LandingImage>();
    }
}
