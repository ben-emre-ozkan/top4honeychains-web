using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.FinalProduct
{
    /// <summary>
    /// A raw honey lot of the batch's wholesaler that cannot be used yet,
    /// because not all of its CCP checks are completed (docs/DECISIONS.md, D-05).
    /// </summary>
    public class IneligibleRawHoneyItemViewModel
    {
        public RawHoneyProduction RawHoney { get; set; } = null!;

        // No CCP check has been assigned to the lot at all
        public bool NoChecksAssigned => PendingChecks.Count == 0;

        // Assigned CCP checks that are not completed yet
        public List<PendingCcpCheckViewModel> PendingChecks { get; set; } = new();
    }

    public class PendingCcpCheckViewModel
    {
        public int CcpId { get; set; }

        public string Code { get; set; } = "";

        public string? Title { get; set; }

        public string StatusName { get; set; } = "";
    }
}
