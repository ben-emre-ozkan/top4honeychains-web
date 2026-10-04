using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // P4 (ROLE-MODEL section 7): a sample of a raw lot, sealed at intake and sent to a laboratory
    public class Sample
    {
        [Key]
        public int SampleId { get; set; }

        [StringLength(30)]
        public string SampleCode { get; set; } = null!;

        public int RawHoneyProductionId { get; set; }

        public int TakingOrgId { get; set; }

        public int LabOrgId { get; set; }

        [StringLength(50)]
        public string SealNumber { get; set; } = null!;

        [StringLength(50)]
        public string CounterSealNumber { get; set; } = null!;

        [StringLength(200)]
        public string CounterLocation { get; set; } = null!;

        public DateOnly RetainUntil { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = SampleStatus.Sent;

        [StringLength(450)]
        public string TakenByUserId { get; set; } = null!;

        public DateTime TakenAt { get; set; }

        [StringLength(450)]
        public string? ReceivedByUserId { get; set; }

        public DateTime? ReceivedAt { get; set; }

        public bool? SealIntact { get; set; }

        [StringLength(500)]
        public string? RejectReason { get; set; }

        public virtual RawHoneyProduction? RawHoneyProduction { get; set; }
        public virtual Organization? TakingOrg { get; set; }
        public virtual Organization? LabOrg { get; set; }
        public virtual LabReport? Report { get; set; }
        public virtual ICollection<SampleCustodyEvent> Events { get; set; } = new List<SampleCustodyEvent>();
    }

    public static class SampleStatus
    {
        public const string Sent = "Sent";
        public const string Received = "Received";
        public const string Rejected = "Rejected";
    }

    public class SampleCustodyEvent
    {
        [Key]
        public int EventId { get; set; }

        public int SampleId { get; set; }

        [StringLength(20)]
        public string Event { get; set; } = null!;

        public int? FromOrgId { get; set; }

        public int? ToOrgId { get; set; }

        [StringLength(450)]
        public string UserId { get; set; } = null!;

        public DateTime At { get; set; }

        public bool? SealIntact { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public virtual Sample? Sample { get; set; }
    }

    // The lab report of a sample: results per CCP parameter and the report PDF with its SHA-256.
    // Draft -> Submitted (Analyst) -> Approved (LabManager, another person, no exemption: D-27)
    public class LabReport
    {
        [Key]
        public int ReportId { get; set; }

        public int SampleId { get; set; }

        public int LabOrgId { get; set; }

        // Public code for the "verify report" page (random, not guessable)
        [StringLength(20)]
        public string ReportCode { get; set; } = null!;

        [StringLength(50)]
        public string? ReportNumber { get; set; }

        public DateOnly? IssuedOn { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = LabReportStatus.Draft;

        [StringLength(100)]
        public string? StoredFileName { get; set; }

        [StringLength(260)]
        public string? OriginalFileName { get; set; }

        [StringLength(64)]
        public string? Sha256 { get; set; }

        [StringLength(450)]
        public string EnteredByUserId { get; set; } = null!;

        public DateTime? SubmittedAt { get; set; }

        [StringLength(450)]
        public string? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(500)]
        public string? ReturnReason { get; set; }

        // Snapshots taken at approval
        public bool IsAccredited { get; set; }

        [StringLength(50)]
        public string? AccreditationNumber { get; set; }

        public bool InHouse { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Sample? Sample { get; set; }
        public virtual Organization? LabOrg { get; set; }
        public virtual ICollection<LabResult> Results { get; set; } = new List<LabResult>();
    }

    public static class LabReportStatus
    {
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
        public const string Approved = "Approved";
    }

    public class LabResult
    {
        [Key]
        public int ResultId { get; set; }

        public int ReportId { get; set; }

        public int ParameterId { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? MeasuredNumericValue { get; set; }

        [StringLength(100)]
        public string? MeasuredTextValue { get; set; }

        public bool? IsInRange { get; set; }

        [StringLength(200)]
        public string? AppliedLimit { get; set; }

        public virtual LabReport? Report { get; set; }
        public virtual CriticalControlPointParameter? Parameter { get; set; }
    }
}
