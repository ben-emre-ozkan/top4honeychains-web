namespace TOP4HoneyChains.Web.Models.ViewModels.HoneyStory
{
    public class HoneyStoryViewModel
    {
        // Batch info
        public int FinalHoneyProductionId { get; set; }
        public string? BatchTitle { get; set; }
        public Guid BatchQR { get; set; }
        public DateTime? ProductionDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? WholesalerName { get; set; }

        // Raw sources (apiary/beekeeper level)
        public List<HoneyStoryRawSource> RawSources { get; set; } = new();

        // Every CCP that applies to batches - batch level
        public List<HoneyStoryCcpBlock> BatchCcpBlocks { get; set; } = new();

        // P4 badges; null when the organization model is off (D-42)
        public HoneyStoryBadges? Badges { get; set; }

        // D-72: the batch's real steps for the journey section
        public HoneyStoryJourney Journey { get; set; } = new();

        // D-98: the laboratory result of the batch's final blend (company path), apart from the lot results; null = none
        public HoneyStoryFinalResult? FinalResult { get; set; }

        // D-108: how the honey was protected on its way (counts only, never seal or container codes); null without the organization model
        public HoneyStoryProtection? Protection { get; set; }

        // D-108: the explanation of each lab parameter on the page (active content, first by sort order), for the value sheet
        public Dictionary<int, Entities.CriticalControlPointParameterContent> ParameterContents { get; set; } = new();
    }

    // D-108: counts of the custody records of the batch's lots and of its final blend sample
    public class HoneyStoryProtection
    {
        public int Containers { get; set; }
        public int SealedContainers { get; set; }
        public int Jars { get; set; }
        public int JarsCheckedAtLab { get; set; }
        public int Deliveries { get; set; }
        public int SmsConfirmedDeliveries { get; set; }
    }

    public class HoneyStoryFinalResult
    {
        public string ReportCode { get; set; } = "";
        public string? LabName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool Accredited { get; set; }
        // Compliant only when the limit is verified (public pages show verified limits only)
        public List<(int ParameterId, string? Name, decimal? Number, string? Text, string? Unit, bool? Compliant, string? Limit)> Values { get; set; } = new();
    }

    // D-72: what is on record for each step; Tracked = the organization model is on (deliveries, reports and the shelf exist)
    public class HoneyStoryJourney
    {
        public bool Tracked { get; set; }
        public DateOnly? HarvestFrom { get; set; }
        public DateOnly? HarvestTo { get; set; }
        public int Deliveries { get; set; }
        public DateOnly? DeliveredOn { get; set; }
        public List<string> Receivers { get; set; } = new();
        public int ApprovedReports { get; set; }
        public List<string> Labs { get; set; } = new();
        public DateTime? LabApprovedAt { get; set; }
        public bool LabValuesRecorded { get; set; }
        public int ControlPointsRecorded { get; set; }
        public int ControlPointsTotal { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public List<string> Retailers { get; set; } = new();
        public DateTime? OnShelfSince { get; set; }
    }

    // Badges from approved, verified data only (D-42)
    public class HoneyStoryBadges
    {
        public bool AccreditedLab { get; set; }
        public List<string> AccreditationNumbers { get; set; } = new();
        public bool InHouseLab { get; set; }
        public bool AllCcpsApproved { get; set; }
        public bool LimitsCompliant { get; set; }
        public List<string> ReportCodes { get; set; } = new();

        // P5 (D-31): country shares by weight, largest first
        public List<HoneyStoryOrigin> Origins { get; set; } = new();
    }

    public record HoneyStoryOrigin(string Country, decimal Percentage);
}
