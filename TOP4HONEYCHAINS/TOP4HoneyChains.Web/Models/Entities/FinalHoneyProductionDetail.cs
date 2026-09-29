using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class FinalHoneyProductionDetail
    {
        [Key]
        public int FinalHoneyProductionDetailId { get; set; }

        [Required]
        public int FinalHoneyProductionId { get; set; }

        [Required]
        public int RawHoneyProductionId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UsedAmount { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(FinalHoneyProductionId))]
        public virtual FinalHoneyProduction FinalHoneyProduction { get; set; } = null!;

        [ForeignKey(nameof(RawHoneyProductionId))]
        public virtual RawHoneyProduction RawHoneyProduction { get; set; } = null!;
    }


}
