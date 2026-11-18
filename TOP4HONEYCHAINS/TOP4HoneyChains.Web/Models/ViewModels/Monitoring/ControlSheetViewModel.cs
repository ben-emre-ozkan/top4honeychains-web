using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    // D-88: a control sheet: the CCPs of a lot (CCP 2–3) or of a batch (CCP 4–10) on one page, entered at once and approved
    // in one action; every CCP keeps its own record, status, approval and audit entry
    public class ControlSheetViewModel
    {
        public string Kind { get; set; } = "";
        public int Id { get; set; }
        public CCPMonitoringViewModelForRawHoney? Lot { get; set; }
        public CCPMonitoringViewModelForFinalBatch? Batch { get; set; }
        public List<CcpMonitoringViewModelBase> Ccps { get; set; } = new();
        public string? LockReason { get; set; }

        // the CCP records the user may approve now, and why the others that wait for approval cannot be approved by the user
        public List<int> Approvable { get; set; } = new();
        public List<string> NotApprovable { get; set; } = new();

        // a save that failed: the problems, and the "Not applicable" choices as posted
        public List<string> Errors { get; set; } = new();
        public Dictionary<int, (bool Chosen, string? Reason)> NotApplicable { get; set; } = new();

        public static bool Editable(CcpMonitoringViewModelBase c) =>
            c.CanEnter && c.LockReason == null && c.StatusId != AssignmentStatusIds.NotApplicable;

        public bool CanEnterAny => Ccps.Any(Editable);
    }

    // The posted sheet; Ccps is keyed by CCP id (form field Ccps.Index)
    public class ControlSheetPost
    {
        public string Kind { get; set; } = "";
        public int Id { get; set; }
        public List<ControlSheetCcpPost> Ccps { get; set; } = new();
    }

    public class ControlSheetCcpPost
    {
        public int CCPId { get; set; }
        public bool NotApplicable { get; set; }
        public string? NotApplicableReason { get; set; }
        public List<CCPParameterGroup> Groups { get; set; } = new();
    }
}
