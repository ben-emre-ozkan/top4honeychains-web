using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class Wholesaler
    {
        [Key]
        public int WholesalerId { get; set; }

        [Required]
        [StringLength(200)]
        public string WholesalerTitle { get; set; } = null!;

        [StringLength(50)]
        public string? WholesalerTaxNo { get; set; }

        [StringLength(150)]
        public string? WholesalerContactName { get; set; }

        [StringLength(50)]
        public string? WholesalerPhone { get; set; }

        [StringLength(150)]
        public string? WholesalerEmail { get; set; }

        [StringLength(400)]
        public string? WholesalerAddress { get; set; }

        [StringLength(100)]
        public string? WholesalerCity { get; set; }

        [StringLength(100)]
        public string? WholesalerCountry { get; set; }

        public bool WholesalerIsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation (Raw Honey ile ilişki)
        [InverseProperty("RawHoneyProductionWholesaler")]
        public virtual ICollection<RawHoneyProduction> RawHoneyProductions { get; set; }
            = new List<RawHoneyProduction>();

        // Navigation (Final Honey ile ilişki)
        [InverseProperty("FinalHoneyProductionWholesaler")]
        public virtual ICollection<FinalHoneyProduction> FinalHoneyProductions { get; set; }
            = new List<FinalHoneyProduction>();
    }
}
