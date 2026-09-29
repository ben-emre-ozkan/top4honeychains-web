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

        // 🔹 Navigation Properties

        [ForeignKey(nameof(RawHoneyCCPAssignmentCCP))]
        public virtual CriticalControlPoint? CCP { get; set; }

        [ForeignKey(nameof(RawHoneyProductionId))]
        public virtual RawHoneyProduction? RawHoneyProduction { get; set; }

        [ForeignKey(nameof(RawHoneyCCPAssignmentStatus))]
        public virtual AssignmentStatus? AssignmentStatus { get; set; }
    }
}
