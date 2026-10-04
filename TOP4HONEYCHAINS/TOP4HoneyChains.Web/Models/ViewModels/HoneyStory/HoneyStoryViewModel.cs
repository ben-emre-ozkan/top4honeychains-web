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
    }
}
