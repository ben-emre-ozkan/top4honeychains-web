namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    public class CCPMonitoringViewModelForFinalBatch
    {
        public int CCPId { get; set; }
        public int FinalHoneyProductionId { get; set; }

        public string CCPCode { get; set; } = "";
        public string CCPTitle { get; set; } = "";
        public string ProcessingStep { get; set; } = "";
        public string CriticalityJustification { get; set; } = "";

        public List<CCPParameterGroup> Groups { get; set; } = new();

        // 🔹 Batch Info
        public string? BatchTitle { get; set; }
        public Guid BatchQR { get; set; }
        public DateTime? ProductionDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? WholesalerName { get; set; }
        public int WholesalerId { get; set; }

        // 🔹 Raw Sources Summary
        public int RawSourceCount { get; set; }
        public List<FinalBatchRawSourceInfo> RawSources { get; set; } = new();
    }
}
