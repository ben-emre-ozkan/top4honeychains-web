namespace TOP4HoneyChains.Web.Models.ViewModels.Timeline
{
    public class CCPOverviewViewModel
    {
        public int RawHoneyProductionId { get; set; }

        public string BeekeeperName { get; set; }
        public string ApiaryName { get; set; }
        public string HoneyType { get; set; }
        public string? WholesalerName { get; set; }
        public DateTime? ProductionDate { get; set; }
        public decimal? Amount { get; set; }

        public List<CCPTimelineItem> Timeline { get; set; } = new();
    }

}
