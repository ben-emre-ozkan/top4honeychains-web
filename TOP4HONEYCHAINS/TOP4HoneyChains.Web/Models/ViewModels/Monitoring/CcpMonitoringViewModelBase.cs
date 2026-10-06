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

        // Organization model (P1): what the user may do here, and the four-eyes approval (D-27)
        public bool OrgModelOn { get; set; }
        public bool CanEnter { get; set; } = true;
        public string? EnterDeniedReason { get; set; }
        public bool CanApprove { get; set; }
        public string? ApproveDeniedReason { get; set; }
        public string? ApprovedBy { get; set; }

        // D-67: the record strip ("Entered by … · Approved by …"), from the latest value log
        public string? EnteredBy { get; set; }
        public DateTime? EnteredAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool SelfApproved { get; set; }

        // Platform administrators with the organization model: read-only, changes through corrections (D-35)
        public bool AdminCorrections { get; set; }
        public List<TOP4HoneyChains.Web.Models.Entities.CcpCorrection> Corrections { get; set; } = new();
    }
}
