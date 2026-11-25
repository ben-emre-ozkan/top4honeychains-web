using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // P3 (ROLE-MODEL section 6): delivery of honey from a beekeeper record to a Processing organization
    public class LotIntake
    {
        [Key]
        public int IntakeId { get; set; }

        public int RawHoneyProductionId { get; set; }

        public int ReceivingOrgId { get; set; }

        [StringLength(450)]
        public string ReceivedByUserId { get; set; } = null!;

        public DateTime ReceivedAt { get; set; }

        public DateOnly DeliveryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [StringLength(100)]
        public string Province { get; set; } = null!;

        [StringLength(100)]
        public string District { get; set; } = null!;

        // the photo of the signed delivery form; since D-78 (023) only the fallback, so nullable
        [StringLength(100)]
        public string? FormStoredFileName { get; set; }

        [StringLength(260)]
        public string? FormOriginalFileName { get; set; }

        [StringLength(64)]
        public string? FormSha256 { get; set; }

        [StringLength(30)]
        public string? SmsStatus { get; set; }

        public DateTime? SmsAt { get; set; }

        // D-78 P2 (D78-SPEC 3.9, migration 023): how the beekeeper confirmed the delivery (SmsCode / SignedPhoto; null = legacy),
        // the hash of the confirmed content, the photo review by the Compliance Officer, and whether containers were recorded
        [StringLength(20)]
        public string? ConfirmationMethod { get; set; }

        [StringLength(64)]
        public string? ContentSha256 { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public long? CodeId { get; set; }

        [StringLength(300)]
        public string? FallbackReason { get; set; }

        [StringLength(20)]
        public string? EvidenceStatus { get; set; }

        [StringLength(450)]
        public string? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? ReviewReason { get; set; }

        public bool? ContainersRecorded { get; set; }

        // D-97 (migration 033): the cap of the delivery's sample jars (JarCap; null = recorded before D-97)
        [StringLength(20)]
        public string? JarCapType { get; set; }

        public bool IsLegacy => ContainersRecorded != true;

        public virtual ICollection<Container> Containers { get; set; } = new List<Container>();

        public virtual RawHoneyProduction? RawHoneyProduction { get; set; }
        public virtual Organization? ReceivingOrg { get; set; }
    }

    // A quantity of a finalized batch moving to another organization; custody moves on receipt
    public class Shipment
    {
        [Key]
        public int ShipmentId { get; set; }

        public int FinalHoneyProductionId { get; set; }

        public int FromOrgId { get; set; }

        public int ToOrgId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = ShipmentStatus.Pending;

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(450)]
        public string ShippedByUserId { get; set; } = null!;

        public DateTime ShippedAt { get; set; }

        [StringLength(450)]
        public string? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        [StringLength(500)]
        public string? RejectReason { get; set; }

        public virtual FinalHoneyProduction? Batch { get; set; }
        public virtual Organization? FromOrg { get; set; }
        public virtual Organization? ToOrg { get; set; }
    }

    public static class ShipmentStatus
    {
        public const string Pending = "Pending";
        public const string Received = "Received";
        public const string Rejected = "Rejected";
    }

    // CCP 10 (storage) of a later holder of a batch, for the quantity it holds
    public class HolderStorageRecord
    {
        [Key]
        public int RecordId { get; set; }

        public int FinalHoneyProductionId { get; set; }

        public int HolderOrgId { get; set; }

        public int CcpId { get; set; }

        public int Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HeldQuantity { get; set; }

        [StringLength(450)]
        public string? RecordedByUserId { get; set; }

        public DateTime? RecordedAt { get; set; }

        [StringLength(450)]
        public string? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public bool SelfApproved { get; set; }

        public virtual FinalHoneyProduction? Batch { get; set; }
        public virtual Organization? HolderOrg { get; set; }
        public virtual CriticalControlPoint? Ccp { get; set; }
        public virtual ICollection<HolderStorageValue> Values { get; set; } = new List<HolderStorageValue>();
    }

    public class HolderStorageValue
    {
        [Key]
        public int ValueId { get; set; }

        public int RecordId { get; set; }

        public int ParameterId { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? MeasuredNumericValue { get; set; }

        [StringLength(100)]
        public string? MeasuredTextValue { get; set; }

        public bool? IsInRange { get; set; }

        [StringLength(200)]
        public string? AppliedLimit { get; set; }

        public DateTime MeasuredAt { get; set; }

        public virtual HolderStorageRecord? Record { get; set; }
        public virtual CriticalControlPointParameter? Parameter { get; set; }
    }
}
