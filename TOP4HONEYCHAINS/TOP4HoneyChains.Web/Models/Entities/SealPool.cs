using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-97 (migration 033): a batch of the seal pool of an organization: seal labels issued by the platform (TS-nnnnnn, printed in
    // advance on tamper-evident VOID label stock) or a range of purchased numbered plastic seals
    public class SealBatch
    {
        [Key]
        public int BatchId { get; set; }

        public int OrgId { get; set; }

        [StringLength(20)]
        public string Kind { get; set; } = SealKind.Label;

        [StringLength(50)]
        public string FirstCode { get; set; } = null!;

        [StringLength(50)]
        public string LastCode { get; set; } = null!;

        public int CodeCount { get; set; }

        [StringLength(200)]
        public string? Note { get; set; }

        [StringLength(450)]
        public string IssuedByUserId { get; set; } = null!;

        public DateTime IssuedAt { get; set; }

        public virtual ICollection<SealCode> Codes { get; set; } = new List<SealCode>();
    }

    // One code of the pool, used once: Issued -> Used (on a container or a jar) -> Broken (opened)
    public class SealCode
    {
        [Key]
        public int SealCodeId { get; set; }

        public int BatchId { get; set; }

        public int OrgId { get; set; }

        [StringLength(50)]
        public string Code { get; set; } = null!;

        [StringLength(20)]
        public string Kind { get; set; } = SealKind.Label;

        [StringLength(20)]
        public string Status { get; set; } = SealStatus.Issued;

        [StringLength(20)]
        public string? UsedAs { get; set; }

        public int? ContainerId { get; set; }

        public int? SampleId { get; set; }

        [StringLength(450)]
        public string? UsedByUserId { get; set; }

        public DateTime? UsedAt { get; set; }

        public DateTime? BrokenAt { get; set; }

        public virtual SealBatch? Batch { get; set; }
        public virtual Container? Container { get; set; }
        public virtual Sample? Sample { get; set; }
    }

    // D-97: an organization's own permanent container (the association's container), registered once with code TK-nnnn
    public class OrgContainer
    {
        [Key]
        public int OrgContainerId { get; set; }

        public int OrgId { get; set; }

        [StringLength(30)]
        public string Code { get; set; } = null!;

        [StringLength(20)]
        public string Kind { get; set; } = ContainerKind.Tin;

        [StringLength(100)]
        public string? Name { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CapacityKg { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TareKg { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(450)]
        public string CreatedByUserId { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public virtual Organization? Org { get; set; }
    }

    public static class SealKind
    {
        // a numbered seal label with a QR code on VOID label stock, across lid and body (or cap and jar)
        public const string Label = "Label";
        // a purchased numbered plastic seal through the eyelet that ties the lid to the body (D-98: only the association's containers
        // have one, so a plastic seal closes only those)
        public const string Plastic = "Plastic";
        public static readonly string[] All = { Label, Plastic };
    }

    public static class SealStatus
    {
        public const string Issued = "Issued";
        public const string Used = "Used";
        public const string Broken = "Broken";
        public static readonly string[] All = { Issued, Used, Broken };
    }

    public static class SealUse
    {
        public const string Origin = "Origin";
        public const string Closing = "Closing";
        public const string Jar = "Jar";
        public const string CounterJar = "CounterJar";
    }

    // D-97: how a container of a delivery is closed
    public static class ClosureKind
    {
        // the beekeeper's own container with a platform seal label across lid and body (default; works on any container)
        public const string SealLabel = "SealLabel";
        // the beekeeper's own container with a numbered plastic seal: recorded by D-97 deliveries only. D-98: no longer offered for new
        // deliveries (a beekeeper's container has no eyelet tying the lid to the body); old deliveries keep it and stay verified.
        public const string PlasticSeal = "PlasticSeal";
        // the association's own permanent container (registered once, code TK-nnnn); closed with a seal label or a plastic seal
        public const string OrgContainer = "OrgContainer";
        public static readonly string[] All = { SealLabel, PlasticSeal, OrgContainer };
        // D-98: the closures a new delivery may choose
        public static readonly string[] New = { SealLabel, OrgContainer };
    }

    // D-97: the cap of the sample jars, chosen once per delivery
    public static class JarCap
    {
        // a safety-button (vacuum) cap: the button stays down only while there is a vacuum in the jar
        public const string SafetyButton = "SafetyButton";
        // a tamper-ring cap: the ring breaks when the cap is opened
        public const string TamperRing = "TamperRing";
        public static readonly string[] All = { SafetyButton, TamperRing };
    }

    // D-98 (migration 036): a seal check when a container is emptied into a batch (append-only). Emptied: the scanned code is the
    // container's closing seal and it is intact; Mismatch / FoundBroken: the step stopped, the container stays sealed.
    public class ContainerEmptying
    {
        [Key]
        public int EmptyingId { get; set; }

        public int ContainerId { get; set; }

        public int FinalHoneyProductionId { get; set; }

        [StringLength(20)]
        public string Outcome { get; set; } = EmptyingOutcome.Emptied;

        [StringLength(50)]
        public string ExpectedSeal { get; set; } = null!;

        [StringLength(50)]
        public string? ScannedSeal { get; set; }

        [StringLength(450)]
        public string UserId { get; set; } = null!;

        public DateTime At { get; set; }

        [StringLength(300)]
        public string? Note { get; set; }

        public virtual Container? Container { get; set; }
        public virtual FinalHoneyProduction? Batch { get; set; }
    }

    public static class EmptyingOutcome
    {
        public const string Emptied = "Emptied";
        public const string Mismatch = "Mismatch";
        public const string FoundBroken = "FoundBroken";
        public static readonly string[] All = { Emptied, Mismatch, FoundBroken };
    }
}
