using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        // Owning organization (D-36); null = the organization created from the wholesaler
        public int? OwnerOrgId { get; set; }

        // Finalize with four-eyes when the organization model is on (D-27)
        [StringLength(450)]
        public string? FinalizeRequestedByUserId { get; set; }
        public DateTime? FinalizeRequestedAt { get; set; }
        [StringLength(450)]
        public string? FinalizedByUserId { get; set; }
        public bool SelfFinalized { get; set; }


        // Not posted by the forms; without this the non-nullable navigation is implicitly
        // required and every Create/Edit POST fails validation without a visible message
        [ValidateNever]
        [ForeignKey(nameof(FinalHoneyProductionWholesalerId))]
        public virtual Wholesaler FinalHoneyProductionWholesaler { get; set; } = null!;

        public virtual ICollection<FinalHoneyProductionDetail> FinalHoneyProductionDetails { get; set; }
            = new List<FinalHoneyProductionDetail>();

        public virtual ICollection<CriticalControlPointMonitoringLog> CriticalControlPointMonitoringLogs { get; set; } = new List<CriticalControlPointMonitoringLog>();

        public virtual ICollection<FinalHoneyCCPAssignment> FinalHoneyCCPAssignments { get; set; } = new List<FinalHoneyCCPAssignment>();
    }

}
