using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // A limit that overrides the parameter's default min/max for one honey class, under a condition
    // on another parameter of the same lot/batch, or for the sum with another parameter (D-18)
    public class CriticalControlPointParameterLimit
    {
        [Key]
        public int LimitId { get; set; }

        public int ParameterId { get; set; }

        // Null = every honey class
        [StringLength(20)]
        public string? HoneyClass { get; set; }

        // Applies only when the value of this parameter is <= ConditionMaxValue
        public int? ConditionParameterId { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? ConditionMaxValue { get; set; }

        // The limit applies to value + the value of this parameter
        public int? SumWithParameterId { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? MinValue { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? MaxValue { get; set; }

        [StringLength(300)]
        public string? Note { get; set; }

        public virtual CriticalControlPointParameter? Parameter { get; set; }
        public virtual CriticalControlPointParameter? ConditionParameter { get; set; }
        public virtual CriticalControlPointParameter? SumWithParameter { get; set; }
    }
}
