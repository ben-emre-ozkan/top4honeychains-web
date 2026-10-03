namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    // Shared by the raw lot and final batch monitoring pages
    public abstract class CcpMonitoringViewModelBase
    {
        public int CCPId { get; set; }
        public string CCPCode { get; set; } = "";
        public string CCPTitle { get; set; } = "";
        public string ProcessingStep { get; set; } = "";
        public string CriticalityJustification { get; set; } = "";

        public List<CCPParameterGroup> Groups { get; set; } = new();

        // "raw" or "batch": used by the shared partials to post back to the right owner
        public abstract string OwnerKind { get; }
        public abstract int OwnerId { get; }

        // Read-only when set (finalized batch, or lot used in one; D-14)
        public string? LockReason { get; set; }

        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        // Not applicable record (D-15)
        public string? NotApplicableReason { get; set; }
        public string? NotApplicableBy { get; set; }
        public DateTime? NotApplicableAt { get; set; }

        public bool HasValues { get; set; }

        // Honey class used to pick limits (Floral / Honeydew / Pine); null = unknown or mixed
        public string? HoneyClass { get; set; }
    }
}
