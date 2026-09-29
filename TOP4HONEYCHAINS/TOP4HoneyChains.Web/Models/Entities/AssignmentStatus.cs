using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class AssignmentStatus
    {
        [Key]
        public int AssignmentStatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string AssignmentStatusName { get; set; } = null!;

        [StringLength(250)]
        public string? AssignmentStatusDescription { get; set; }

        public virtual ICollection<RawHoneyCCPAssignment> RawHoneyCCPAssignments { get; set; }
            = new List<RawHoneyCCPAssignment>();
    }
}
