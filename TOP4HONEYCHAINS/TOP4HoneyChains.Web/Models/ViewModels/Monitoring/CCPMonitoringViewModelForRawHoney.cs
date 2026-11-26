namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    public class CCPMonitoringViewModelForRawHoney : CcpMonitoringViewModelBase
    {
        public int RawHoneyProductionId { get; set; }
        public string? BeekeeperName { get; set; }
        public string? ApiaryName { get; set; }
        public string? HoneyType { get; set; }
        public DateTime? ProductionDate { get; set; }
        public decimal? Amount { get; set; }
        public int ApiaryId { get; set; }

        // D-98: the path of the lot's owner (CcpPaths) and the association path's "ready for packing" (CCP 1 page only)
        public string? Path { get; set; }
        public DateTime? PackingReadyAt { get; set; }
        public string? PackingReadyDenied { get; set; }

        public override string OwnerKind => "raw";
        public override int OwnerId => RawHoneyProductionId;
    }

}
