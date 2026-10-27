using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-78 P3 (D78-SPEC 3.8, migration 024): the beekeeper's asking price of a lot. Append-only; the current price is the latest row.
    public class LotPrice
    {
        [Key]
        public int PriceId { get; set; }

        public int RawHoneyProductionId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerKg { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = null!;

        [StringLength(20)]
        public string Basis { get; set; } = PriceBasis.Asking;

        [StringLength(200)]
        public string? Note { get; set; }

        [StringLength(450)]
        public string EnteredByUserId { get; set; } = null!;

        public DateTime EnteredAt { get; set; }

        [StringLength(45)]
        public string? ClientIp { get; set; }
    }

    public static class PriceBasis
    {
        public const string Asking = "Asking";
        public const string Agreed = "Agreed";
    }

    // D-78 P3 (D-82, migration 024): the two-admin correction of a delivery recorded with containers. The old and the new
    // confirmed content stay in the row; only a pending row changes, and only by its decision.
    public class DeliveryCorrection
    {
        [Key]
        public int CorrectionId { get; set; }

        public int IntakeId { get; set; }

        public int RawHoneyProductionId { get; set; }

        public string OldContent { get; set; } = null!;

        [StringLength(64)]
        public string OldContentSha256 { get; set; } = null!;

        public string NewContent { get; set; } = null!;

        [StringLength(64)]
        public string NewContentSha256 { get; set; } = null!;

        [StringLength(500)]
        public string Reason { get; set; } = null!;

        [StringLength(20)]
        public string Status { get; set; } = CorrectionStatus.Pending;

        [StringLength(450)]
        public string RequestedByUserId { get; set; } = null!;

        public DateTime RequestedAt { get; set; }

        [StringLength(450)]
        public string? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        [StringLength(500)]
        public string? DecisionNote { get; set; }

        public long? InfoSmsId { get; set; }

        public virtual LotIntake? Intake { get; set; }
    }

    // D-78 P3 (D78-SPEC 4.8, migration 025): a request of a data subject with its 30-day KVKK deadline
    public class DataSubjectRequest
    {
        [Key]
        public int RequestId { get; set; }

        public int OrgId { get; set; }

        [StringLength(450)]
        public string? BeekeeperId { get; set; }

        [StringLength(200)]
        public string SubjectName { get; set; } = null!;

        [StringLength(30)]
        public string Kind { get; set; } = null!;

        [StringLength(50)]
        public string Channel { get; set; } = null!;

        public DateOnly ReceivedOn { get; set; }

        public DateOnly DueOn { get; set; }

        [StringLength(1000)]
        public string Details { get; set; } = null!;

        [StringLength(20)]
        public string Status { get; set; } = DataSubjectRequestStatus.Open;

        [StringLength(1000)]
        public string? Outcome { get; set; }

        [StringLength(450)]
        public string RecordedByUserId { get; set; } = null!;

        public DateTime RecordedAt { get; set; }

        [StringLength(450)]
        public string? ClosedByUserId { get; set; }

        public DateTime? ClosedAt { get; set; }
    }

    public static class DataSubjectRequestKind
    {
        public const string Access = "Access";
        public const string Correction = "Correction";
        public const string Deletion = "Deletion";
        public const string Objection = "Objection";
        public const string ConsentWithdrawal = "ConsentWithdrawal";
        public static readonly string[] All = { Access, Correction, Deletion, Objection, ConsentWithdrawal };
    }

    public static class DataSubjectRequestStatus
    {
        public const string Open = "Open";
        public const string Answered = "Answered";
        public const string Refused = "Refused";
    }

    // D-78 P3 (D78-SPEC 4.8, migration 025): the Compliance Officer's append-only review of a sampling-quality item
    public class ComplianceReview
    {
        [Key]
        public int ReviewId { get; set; }

        public int OrgId { get; set; }

        [StringLength(20)]
        public string SubjectType { get; set; } = null!;

        public int SubjectId { get; set; }

        [StringLength(20)]
        public string Outcome { get; set; } = null!;

        [StringLength(1000)]
        public string Finding { get; set; } = null!;

        [StringLength(1000)]
        public string? Action { get; set; }

        [StringLength(450)]
        public string ReviewedByUserId { get; set; } = null!;

        public DateTime ReviewedAt { get; set; }
    }

    public static class ReviewSubject
    {
        public const string Intake = "Intake";
        public const string Sample = "Sample";
        public const string SealEvent = "SealEvent";
    }

    public static class ReviewOutcome
    {
        public const string InOrder = "InOrder";
        public const string Finding = "Finding";
    }

    public static class AgreementKinds
    {
        public const string DataProcessingAgreement = "DataProcessingAgreement";
        public const string SupplyAgreement = "SupplyAgreement";
        public static readonly string[] All = { DataProcessingAgreement, SupplyAgreement };
    }
}
