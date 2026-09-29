namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    public class CCPMonitoringViewModelForRawHoney
    {
        public int CCPId { get; set; }
        public int RawHoneyProductionId { get; set; }
        public string CCPCode { get; set; } = "";
        public string CCPTitle { get; set; } = "";
        public string ProcessingStep { get; set; } = "";
        public string CriticalityJustification { get; set; } = "";
        public List<CCPParameterGroup> Groups { get; set; } = new();
        public string? BeekeeperName { get; set; }
        public string? ApiaryName { get; set; }
        public string? HoneyType { get; set; }
        public DateTime? ProductionDate { get; set; }
        public decimal? Amount { get; set; }
        public int ApiaryId { get; set; }
    }

}
