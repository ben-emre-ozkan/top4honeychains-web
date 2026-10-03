using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class CriticalControlPointMonitoringLog
    {
        [Key]
        public int LogId { get; set; }

        public int? CCPId { get; set; }
        public int? ParameterId { get; set; }

        public int? RawHoneyProductionId { get; set; }
        public int? FinalHoneyProductionId { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? MeasuredNumericValue { get; set; }

        [StringLength(100)]
        public string? MeasuredTextValue { get; set; }

        public DateTime? MeasurementDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Computed on save from the parameter's limits (D-18); null when no limit applies
        public bool? IsInRange { get; set; }

        // The limit the value was checked against, as text (e.g. "max 40")
        [StringLength(200)]
        public string? AppliedLimit { get; set; }

        // File name under App_Data/ccp-files for File parameters (D-19); MeasuredTextValue holds the original name
        [StringLength(260)]
        public string? StoredFileName { get; set; }

        [ForeignKey(nameof(CCPId))]
        public virtual CriticalControlPoint? CCP { get; set; }

        [ForeignKey(nameof(ParameterId))]
        public virtual CriticalControlPointParameter? Parameter { get; set; }

        [ForeignKey(nameof(RawHoneyProductionId))]
        public virtual RawHoneyProduction? RawHoneyProduction { get; set; }

        [ForeignKey(nameof(FinalHoneyProductionId))]
        public virtual FinalHoneyProduction? FinalHoneyProduction { get; set; }
    }


}
