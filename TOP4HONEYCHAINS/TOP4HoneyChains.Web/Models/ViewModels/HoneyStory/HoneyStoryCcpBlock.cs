namespace TOP4HoneyChains.Web.Models.ViewModels.HoneyStory
{
    // One CCP of a lot or of the batch on the public page. Gaps are shown, not hidden (D-16)
    public class HoneyStoryCcpBlock
    {
        public const string Recorded = "Recorded";
        public const string Incomplete = "Incomplete";
        public const string NotApplicable = "NotApplicable";
        public const string NotRecorded = "NotRecorded";

        public int CCPId { get; set; }
        public string? CCPCode { get; set; }
        public string? CCPTitle { get; set; }
        public string? ProcessingStep { get; set; }

        public string State { get; set; } = NotRecorded;
        public string? NotApplicableReason { get; set; }

        // Every parameter of the CCP; Recorded = false for parameters without a value
        public List<HoneyStoryCCPRow> Parameters { get; set; } = new();
    }
}
