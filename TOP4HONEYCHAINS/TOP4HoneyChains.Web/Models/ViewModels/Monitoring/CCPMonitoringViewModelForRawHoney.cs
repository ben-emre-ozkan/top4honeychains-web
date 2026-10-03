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

        public override string OwnerKind => "raw";
        public override int OwnerId => RawHoneyProductionId;
    }

}
