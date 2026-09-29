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
