using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class CriticalControlPointParameter
    {
        [Key]
        public int ParameterId { get; set; }

        [Required(ErrorMessage = "The CCP field is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "The CCP field is required.")]
        public int? CCPId { get; set; }

        [StringLength(500)]
        public string? ParameterName { get; set; }

        public int? ParameterCategory { get; set; }

        [StringLength(50)]
        public string? ParameterUnit { get; set; }

        [StringLength(100)]
        public string? CriticalLimit { get; set; }

        [ForeignKey(nameof(CCPId))]
        public virtual CriticalControlPoint? CCP { get; set; }

        [ForeignKey(nameof(ParameterCategory))]
        public virtual CriticalControlPointParameterCategory? ParameterCategoryNavigation { get; set; }

        public virtual ICollection<CriticalControlPointMonitoringLog> CriticalControlPointMonitoringLogs { get; set; }
            = new List<CriticalControlPointMonitoringLog>();

        public virtual ICollection<CriticalControlPointParameterContent> CriticalControlPointParameterContents { get; set; }
    = new List<CriticalControlPointParameterContent>();
    }


}
