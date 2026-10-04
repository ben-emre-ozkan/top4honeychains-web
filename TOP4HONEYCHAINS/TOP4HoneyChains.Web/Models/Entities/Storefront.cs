using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // P5 (ROLE-MODEL section 11, D-30, D-43): consent of a beekeeper, recorded by the association
    public class Consent
    {
        [Key]
        public int ConsentId { get; set; }

        [StringLength(450)]
        public string BeekeeperId { get; set; } = null!;

        public int AssociationOrgId { get; set; }

        [StringLength(30)]
        public string Purpose { get; set; } = ConsentPurpose.Storefront;

        // The beekeeper agreed to show the name; otherwise the storefront shows the neutral code
        public bool NameConsented { get; set; }

        [StringLength(30)]
        public string TextVersion { get; set; } = null!;

        [StringLength(64)]
        public string TextSha256 { get; set; } = null!;

        public DateOnly GivenOn { get; set; }

        [StringLength(100)]
        public string DocumentStoredFileName { get; set; } = null!;

        [StringLength(260)]
        public string DocumentOriginalFileName { get; set; } = null!;

        [StringLength(64)]
        public string DocumentSha256 { get; set; } = null!;

        [StringLength(450)]
        public string RecordedByUserId { get; set; } = null!;

        public DateTime RecordedAt { get; set; }

        public DateTime? WithdrawnAt { get; set; }

        [StringLength(450)]
        public string? WithdrawnByUserId { get; set; }

        [StringLength(500)]
        public string? WithdrawReason { get; set; }

        public virtual Beekeeper? Beekeeper { get; set; }
        public virtual Organization? AssociationOrg { get; set; }
    }

    public static class ConsentPurpose
    {
        public const string Storefront = "Storefront";
    }

    // Stable, random public code of a beekeeper's storefront (never derived from the name)
    public class BeekeeperStorefront
    {
        [Key, StringLength(450)]
        public string BeekeeperId { get; set; } = null!;

        [StringLength(20)]
        public string Code { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }

    // Country share of a batch by weight, stored at finalize (EU 2024/1438, D-31)
    public class BatchOriginShare
    {
        public int FinalHoneyProductionId { get; set; }

        [StringLength(2)]
        public string CountryCode { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; }

        public DateTime ComputedAt { get; set; }
    }
}
