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

        // D-78 (020): null for a consent given at registration (the directorate is RegisteringOrgId)
        public int? AssociationOrgId { get; set; }

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
        public string? DocumentStoredFileName { get; set; }

        [StringLength(260)]
        public string? DocumentOriginalFileName { get; set; }

        [StringLength(64)]
        public string? DocumentSha256 { get; set; }

        [StringLength(450)]
        public string RecordedByUserId { get; set; } = null!;

        public DateTime RecordedAt { get; set; }

        public DateTime? WithdrawnAt { get; set; }

        [StringLength(450)]
        public string? WithdrawnByUserId { get; set; }

        [StringLength(500)]
        public string? WithdrawReason { get; set; }

        // D-78 (020): how the consent was confirmed (SmsCode, SignedPhoto, Portal; null = signed document, before D-78),
        // the code that confirmed it, the consent text version shown and the directorate that recorded it
        [StringLength(20)]
        public string? Method { get; set; }

        public long? CodeId { get; set; }

        public int? NoticeId { get; set; }

        public int? RegisteringOrgId { get; set; }

        public virtual Beekeeper? Beekeeper { get; set; }
        public virtual Organization? AssociationOrg { get; set; }
    }

    public static class ConsentPurpose
    {
        public const string Storefront = "Storefront";
        // D-78 (D78-SPEC 6.2): optional consents captured at registration
        public const string PublicName = "PublicName";
        public const string PortalNotifications = "PortalNotifications";
        // D-78 P3 (O-6): the beekeeper lets the association read the asking prices
        public const string AssociationPrice = "AssociationPrice";
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
