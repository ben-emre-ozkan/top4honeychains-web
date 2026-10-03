using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class RawHoneyCCPAssignment
    {
        [Key]
        public int RawHoneyCCPAssignmentId { get; set; }

        [Required]
        public int RawHoneyCCPAssignmentCCP { get; set; }

        [Required]
        public int RawHoneyProductionId { get; set; }

        public DateTime RawHoneyCCPAssignmentStartDate { get; set; }

        public DateTime? RawHoneyCCPAssignmentEndDate { get; set; }

        [Required]
        public int RawHoneyCCPAssignmentStatus { get; set; }

        // Set together with status "Not applicable" (D-15); a DB check constraint enforces it
        [StringLength(500)]
        public string? NotApplicableReason { get; set; }

        [StringLength(256)]
        public string? NotApplicableBy { get; set; }

        public DateTime? NotApplicableAt { get; set; }

        // Four-eyes approval of the CCP record (D-27); SelfApproved = approved by its recorder under an exemption
        [StringLength(450)]
        public string? ApprovedByUserId { get; set; }

        public int? ApprovedByOrgId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public bool SelfApproved { get; set; }

        // 🔹 Navigation Properties

        [ForeignKey(nameof(RawHoneyCCPAssignmentCCP))]
        public virtual CriticalControlPoint? CCP { get; set; }

        [ForeignKey(nameof(RawHoneyProductionId))]
        public virtual RawHoneyProduction? RawHoneyProduction { get; set; }

        [ForeignKey(nameof(RawHoneyCCPAssignmentStatus))]
        public virtual AssignmentStatus? AssignmentStatus { get; set; }
    }
}
