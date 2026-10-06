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

        // Parameter metadata (D-18). DataType: CcpDataType.Number / YesNo / Text / File
        [StringLength(10)]
        [RegularExpression("^(Number|YesNo|Text|File)$", ErrorMessage = "Choose a data type.")]
        public string? DataType { get; set; }

        // Default limits; honey-class specific and conditional limits are in Limits
        [Column(TypeName = "decimal(10,4)")]
        public decimal? MinValue { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? MaxValue { get; set; }

        public bool IsRequired { get; set; } = true;

        [StringLength(300)]
        public string? LimitNote { get; set; }

        [StringLength(200)]
        public string? LimitSource { get; set; }

        // False while the limits are unverified seeds; only verified limits are shown publicly
        public bool LimitVerified { get; set; }

        // D-62: required only when the parameter RequiredIfParameterId (same CCP) has the answer RequiredIfValue
        public int? RequiredIfParameterId { get; set; }

        [StringLength(50)]
        public string? RequiredIfValue { get; set; }

        [ForeignKey(nameof(CCPId))]
        public virtual CriticalControlPoint? CCP { get; set; }

        [ForeignKey(nameof(ParameterCategory))]
        public virtual CriticalControlPointParameterCategory? ParameterCategoryNavigation { get; set; }

        public virtual ICollection<CriticalControlPointMonitoringLog> CriticalControlPointMonitoringLogs { get; set; }
            = new List<CriticalControlPointMonitoringLog>();

        public virtual ICollection<CriticalControlPointParameterContent> CriticalControlPointParameterContents { get; set; }
    = new List<CriticalControlPointParameterContent>();

        public virtual ICollection<CriticalControlPointParameterLimit> Limits { get; set; }
            = new List<CriticalControlPointParameterLimit>();
    }


}
