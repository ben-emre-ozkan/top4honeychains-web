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

        // Where the CCP is recorded: CcpAppliesTo.RawLot or CcpAppliesTo.Batch (D-17)
        [Required(ErrorMessage = "Choose where this CCP applies.")]
        [RegularExpression("^(RawLot|Batch)$", ErrorMessage = "Choose where this CCP applies.")]
        [StringLength(10)]
        public string? AppliesTo { get; set; }

        // Whether free-text values of this CCP may appear on the public Honey Story page (D-20)
        public bool PublicTextValues { get; set; }

        public virtual ICollection<CriticalControlPointParameter> CriticalControlPointParameters { get; set; }
            = new List<CriticalControlPointParameter>();

        public virtual ICollection<CriticalControlPointMonitoringLog> CriticalControlPointMonitoringLogs { get; set; }
            = new List<CriticalControlPointMonitoringLog>();
    }

}
