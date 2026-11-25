using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-78 P2 (D78-SPEC 3.2, migration 023): one physical container of a delivery. Never changed after insert; the lot amount is
    // the sum of the net weights.
    public class Container
    {
        [Key]
        public int ContainerId { get; set; }

        public int IntakeId { get; set; }

        public int RawHoneyProductionId { get; set; }

        public int Seq { get; set; }

        // L{lot}-C{n}, printed on the container label with a QR code
        [StringLength(30)]
        public string ContainerCode { get; set; } = null!;

        [StringLength(20)]
        public string Kind { get; set; } = ContainerKind.Tin;

        [Column(TypeName = "decimal(10,2)")]
        public decimal NetWeightKg { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? GrossWeightKg { get; set; }

        // the seal applied before delivery; null when the container arrived unsealed
        [StringLength(50)]
        public string? OriginSealNumber { get; set; }

        public bool? OriginSealIntact { get; set; }

        [StringLength(450)]
        public string CreatedByUserId { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        // D-97 (migration 033): how the container was closed (ClosureKind; null = recorded before D-97) and, for the association's
        // own container, which one
        [StringLength(20)]
        public string? Closure { get; set; }

        public int? OrgContainerId { get; set; }

        public virtual OrgContainer? OrgContainer { get; set; }

        public virtual LotIntake? Intake { get; set; }
        public virtual ICollection<ContainerSealEvent> SealEvents { get; set; } = new List<ContainerSealEvent>();
        public virtual ICollection<Sample> Samples { get; set; } = new List<Sample>();
    }

    // Append-only seal history of a container
    public class ContainerSealEvent
    {
        [Key]
        public int EventId { get; set; }

        public int ContainerId { get; set; }

        [StringLength(20)]
        public string Event { get; set; } = null!;

        [StringLength(50)]
        public string SealNumber { get; set; } = null!;

        public bool? Intact { get; set; }

        [StringLength(450)]
        public string UserId { get; set; } = null!;

        public DateTime At { get; set; }

        [StringLength(300)]
        public string? Note { get; set; }

        [StringLength(100)]
        public string? PhotoStoredFileName { get; set; }

        [StringLength(64)]
        public string? PhotoSha256 { get; set; }

        public virtual Container? Container { get; set; }
    }

    public static class ContainerKind
    {
        public const string Tin = "Tin";
        public const string Drum = "Drum";
        public const string Bucket = "Bucket";
        public const string Ibc = "Ibc";
        public const string Other = "Other";
        public static readonly string[] All = { Tin, Drum, Bucket, Ibc, Other };
    }

    public static class SealEvent
    {
        // the origin seal checked at intake
        public const string Checked = "Checked";
        // a plant seal applied
        public const string Applied = "Applied";
        // opened for sampling or processing
        public const string Broken = "Broken";
        public const string Replaced = "Replaced";
    }

    public static class ConfirmationMethod
    {
        public const string SmsCode = "SmsCode";
        public const string SignedPhoto = "SignedPhoto";
    }
}
