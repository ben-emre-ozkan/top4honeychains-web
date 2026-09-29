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

        // Final CCPs (CCP4-CCP10) - batch level
        public List<HoneyStoryCCPRow> FinalCCPs { get; set; } = new();
    }
}
