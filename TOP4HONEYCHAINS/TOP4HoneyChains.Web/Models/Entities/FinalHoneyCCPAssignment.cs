using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class FinalHoneyCCPAssignment
    {
        [Key]
        public int FinalHoneyCCPAssignmentId { get; set; }

        [Required]
        public int FinalHoneyCCPAssignmentCCP { get; set; }

        [Required]
        public int FinalHoneyProductionId { get; set; }

        public DateTime FinalHoneyCCPAssignmentStartDate { get; set; }

        public DateTime? FinalHoneyCCPAssignmentEndDate { get; set; }

        [Required]
        public int FinalHoneyCCPAssignmentStatus { get; set; }

        [ForeignKey(nameof(FinalHoneyCCPAssignmentCCP))]
        public virtual CriticalControlPoint? CCP { get; set; }

        [ForeignKey(nameof(FinalHoneyProductionId))]
        public virtual FinalHoneyProduction? FinalHoneyProduction { get; set; }

        [ForeignKey(nameof(FinalHoneyCCPAssignmentStatus))]
        public virtual AssignmentStatus? AssignmentStatus { get; set; }
    }
}
