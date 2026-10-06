using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // One status pill style everywhere (D-66): an icon and a text, never colour alone (Views/Shared/_StatusPill.cshtml)
    // Fresh: the record was approved just now; the pill draws its check mark once (D-67)
    public record StatusPill(string Kind, string Text, string? Id = null, string? Title = null, bool Fresh = false)
    {
        public const string Draft = "draft", Submitted = "submitted", Approved = "approved", Returned = "returned", Locked = "locked",
            NotApplicable = "na", OutOfLimit = "outlimit", InLimit = "inlimit", Pending = "pending";

        // CCP assignment status (AssignmentStatusIds)
        public static string ForAssignment(int status, bool approved) => status switch
        {
            AssignmentStatusIds.Submitted => Submitted,
            AssignmentStatusIds.Completed => approved ? Approved : Submitted,
            AssignmentStatusIds.NotApplicable => NotApplicable,
            AssignmentStatusIds.Rejected => Returned,
            _ => Draft
        };

        // lab report status; a draft with a return reason was returned
        public static string ForReport(string? status, bool returned) => status switch
        {
            LabReportStatus.Approved => Approved,
            LabReportStatus.Submitted => Submitted,
            _ => returned ? Returned : Draft
        };

        public static string ForShipment(string? status) => status switch
        {
            ShipmentStatus.Pending => Pending,
            ShipmentStatus.Received => Approved,
            _ => Returned
        };

        public static string ForSample(string? status) => status switch
        {
            SampleStatus.Sent => Pending,
            SampleStatus.Received => Approved,
            _ => Returned
        };
    }
}
