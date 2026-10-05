using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.HoneyStory
{
    // The public parameter page (mobile: parameter detail): what the value means, and, when opened from a Honey
    // Story, the value measured on one lot of that batch ("In this jar"); compliance only for verified limits (D-18)
    public class HoneyStoryParameterViewModel
    {
        public CriticalControlPointParameter Parameter { get; set; } = null!;
        public CriticalControlPointParameterContent? Content { get; set; }

        public Guid? Qr { get; set; }
        public int? Lot { get; set; }
        public HoneyStoryCCPRow? InJar { get; set; }
        public string? JarSource { get; set; }

        // Parameters that are read together with this one (same CCP)
        public List<CriticalControlPointParameter> Related { get; set; } = new();
    }
}
