using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // Who is responsible for a CCP (D-25): the organization that owns the lot/batch, or the association of
    // the lot's beekeeper; which capability it needs, which org roles enter and which approve
    public class CcpResponsibility
    {
        [Key]
        public int CcpId { get; set; }

        [StringLength(30)]
        public string OwnerRule { get; set; } = CcpOwnerRules.Owner;

        public bool RequiresApproval { get; set; } = true;

        public DateTime UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedByUserId { get; set; }

        public virtual CriticalControlPoint? Ccp { get; set; }
        public virtual ICollection<CcpResponsibilityCapability> Capabilities { get; set; } = new List<CcpResponsibilityCapability>();
        public virtual ICollection<CcpResponsibilityRole> Roles { get; set; } = new List<CcpResponsibilityRole>();
    }

    public class CcpResponsibilityCapability
    {
        public int CcpId { get; set; }

        [StringLength(30)]
        public string Capability { get; set; } = null!;
    }

    public class CcpResponsibilityRole
    {
        public int CcpId { get; set; }

        // CcpActions.Enter or CcpActions.Approve
        [StringLength(10)]
        public string Action { get; set; } = null!;

        [StringLength(30)]
        public string OrgRole { get; set; } = null!;
    }

    // A correction of a recorded CCP value, applied only after a second person approves it (D-35, D-27)
    public class CcpCorrection
    {
        [Key]
        public int CorrectionId { get; set; }

        [StringLength(10)]
        public string OwnerKind { get; set; } = null!;

        public int OwnerId { get; set; }

        public int CcpId { get; set; }

        public int ParameterId { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? OldNumericValue { get; set; }

        [StringLength(100)]
        public string? OldTextValue { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? NewNumericValue { get; set; }

        [StringLength(100)]
        public string? NewTextValue { get; set; }

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

        public virtual CriticalControlPoint? Ccp { get; set; }
        public virtual CriticalControlPointParameter? Parameter { get; set; }
    }

    public static class CcpOwnerRules
    {
        public const string Owner = "Owner";
        public const string BeekeeperAssociation = "BeekeeperAssociation";
        public static readonly string[] All = { Owner, BeekeeperAssociation };
    }

    public static class CcpActions
    {
        public const string Enter = "Enter";
        public const string Approve = "Approve";
    }

    public static class CorrectionStatus
    {
        public const string Pending = "Pending";
        public const string Applied = "Applied";
        public const string Rejected = "Rejected";
    }
}
