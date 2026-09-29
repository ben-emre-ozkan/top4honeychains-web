using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class CriticalControlPoint
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string ProcessingStep { get; set; } = null!;

        [Required, StringLength(20)]
        public string Code { get; set; } = null!;

        [StringLength(150)]
        public string? Title { get; set; }

        public string? CriticalityJustification { get; set; }

        public virtual ICollection<CriticalControlPointParameter> CriticalControlPointParameters { get; set; }
            = new List<CriticalControlPointParameter>();

        public virtual ICollection<CriticalControlPointMonitoringLog> CriticalControlPointMonitoringLogs { get; set; }
            = new List<CriticalControlPointMonitoringLog>();
    }

}
