using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class FinalHoneyProduction
    {
        [Key]
        public int FinalHoneyProductionId { get; set; }

        [Required(ErrorMessage = "The title is required.")]
        [StringLength(150)]
        public string? FinalHoneyProductionTitle { get; set; }

        public Guid FinalHoneyProductionQR { get; set; }

        [Column(TypeName = "date")]
        [Required(ErrorMessage = "The date is required.")]
        public DateTime? FinalHoneyProductionDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? FinalHoneyProductionTotalAmount { get; set; }

        [Required]
        public int FinalHoneyProductionWholesalerId { get; set; }

        [Required]
        public bool FinalHoneyProductionIsFinalized { get; set; } = false;

        public DateTime? FinalHoneyProductionFinalizedDate { get; set; }


        [ForeignKey(nameof(FinalHoneyProductionWholesalerId))]
        public virtual Wholesaler FinalHoneyProductionWholesaler { get; set; } = null!;

        public virtual ICollection<FinalHoneyProductionDetail> FinalHoneyProductionDetails { get; set; }
            = new List<FinalHoneyProductionDetail>();

        public virtual ICollection<CriticalControlPointMonitoringLog> CriticalControlPointMonitoringLogs { get; set; } = new List<CriticalControlPointMonitoringLog>();

        public virtual ICollection<FinalHoneyCCPAssignment> FinalHoneyCCPAssignments { get; set; } = new List<FinalHoneyCCPAssignment>();
    }

}
