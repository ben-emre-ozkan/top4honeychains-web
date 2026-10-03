namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    public class CCPMonitoringViewModelForFinalBatch : CcpMonitoringViewModelBase
    {
        public int FinalHoneyProductionId { get; set; }

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

        public override string OwnerKind => "batch";
        public override int OwnerId => FinalHoneyProductionId;
    }
}
