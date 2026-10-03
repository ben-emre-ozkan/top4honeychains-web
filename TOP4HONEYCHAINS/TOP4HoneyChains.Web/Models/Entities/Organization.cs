using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // An organization with one or more capabilities (docs/ROLE-MODEL.md section 3, D-22)
    public class Organization
    {
        [Key]
        public int OrgId { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(200)]
        public string DisplayName { get; set; } = null!;

        [StringLength(300)]
        public string? LegalName { get; set; }

        [StringLength(50)]
        public string? TaxNo { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        // OrgStatus.Pending / Verified / Suspended; changed only through the verification workflow
        [StringLength(20)]
        public string Status { get; set; } = OrgStatus.Pending;

        public bool IsPlatform { get; set; }

        // Four-eyes exemption, set only by PlatformAdmin; never applies to lab results (D-27)
        public bool FourEyesExempt { get; set; }

        public int? LegacyWholesalerId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [StringLength(450)]
        public string? VerifiedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        public virtual Wholesaler? LegacyWholesaler { get; set; }
        public virtual ICollection<OrgCapability> Capabilities { get; set; } = new List<OrgCapability>();
        public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
        public virtual ICollection<OrgDocument> Documents { get; set; } = new List<OrgDocument>();
        public virtual ICollection<OrgVerificationEvent> VerificationEvents { get; set; } = new List<OrgVerificationEvent>();
    }

    public class Capability
    {
        [Key, StringLength(30)]
        public string Code { get; set; } = null!;

        [StringLength(300)]
        public string Description { get; set; } = null!;

        public int SortOrder { get; set; }
    }

    public class OrgCapability
    {
        public int OrgId { get; set; }

        [StringLength(30)]
        public string Capability { get; set; } = null!;

        public bool IsAccredited { get; set; }

        [StringLength(100)]
        public string? AccreditationNumber { get; set; }

        public DateOnly? AccreditationValidUntil { get; set; }

        public DateTime AddedAt { get; set; }

        [StringLength(450)]
        public string? AddedByUserId { get; set; }

        public virtual Organization? Org { get; set; }
    }

    public class OrgDocument
    {
        [Key]
        public int DocumentId { get; set; }

        public int OrgId { get; set; }

        [StringLength(30)]
        public string Kind { get; set; } = null!;

        [StringLength(255)]
        public string OriginalFileName { get; set; } = null!;

        [StringLength(260)]
        public string StoredFileName { get; set; } = null!;

        [StringLength(64)]
        public string Sha256 { get; set; } = null!;

        [StringLength(450)]
        public string UploadedByUserId { get; set; } = null!;

        public DateTime UploadedAt { get; set; }

        public virtual Organization? Org { get; set; }
    }

    public class OrgVerificationEvent
    {
        [Key]
        public int EventId { get; set; }

        public int OrgId { get; set; }

        [StringLength(20)]
        public string FromStatus { get; set; } = null!;

        [StringLength(20)]
        public string ToStatus { get; set; } = null!;

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(450)]
        public string ByUserId { get; set; } = null!;

        public DateTime At { get; set; }

        public virtual Organization? Org { get; set; }
    }

    public class OrgRoleDefinition
    {
        [Key, StringLength(30)]
        public string Code { get; set; } = null!;

        [StringLength(300)]
        public string Description { get; set; } = null!;

        public int SortOrder { get; set; }
    }

    // A user's membership in an organization with one org role (ROLE-MODEL section 4)
    public class Membership
    {
        [Key]
        public int MembershipId { get; set; }

        [StringLength(450)]
        public string UserId { get; set; } = null!;

        public int OrgId { get; set; }

        [StringLength(30)]
        public string OrgRole { get; set; } = null!;

        [StringLength(20)]
        public string Status { get; set; } = MembershipStatus.Active;

        [StringLength(450)]
        public string? GrantedByUserId { get; set; }

        public DateTime GrantedAt { get; set; }

        [StringLength(450)]
        public string? RevokedByUserId { get; set; }

        public DateTime? RevokedAt { get; set; }

        public virtual Organization? Org { get; set; }
    }

    // A beekeeper is a record linked to an association, without an account (D-23)
    public class BeekeeperAssociation
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string BeekeeperId { get; set; } = null!;

        public int AssociationOrgId { get; set; }

        public DateOnly ValidFrom { get; set; }

        public DateOnly? ValidTo { get; set; }

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Beekeeper? Beekeeper { get; set; }
        public virtual Organization? AssociationOrg { get; set; }
    }

    // The beekeeper's national registry (AKS) enterprise number and its verification state (D-29).
    // A separate table, not Beekeepers columns, because the scaffolded beekeeper Edit binds the whole row.
    public class BeekeeperRegistry
    {
        [Key, StringLength(450)]
        public string BeekeeperId { get; set; } = null!;

        [StringLength(14)]
        public string? RegistryNumber { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = RegistryStatus.Unverified;

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [StringLength(450)]
        public string? VerifiedByUserId { get; set; }

        public virtual Beekeeper? Beekeeper { get; set; }
    }

    public class RegistryVerification
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string BeekeeperId { get; set; } = null!;

        [StringLength(14)]
        public string EnterpriseNumber { get; set; } = null!;

        [StringLength(20)]
        public string Method { get; set; } = null!;

        [StringLength(20)]
        public string Status { get; set; } = RegistryStatus.Pending;

        [StringLength(255)]
        public string? OriginalFileName { get; set; }

        [StringLength(260)]
        public string? StoredFileName { get; set; }

        [StringLength(64)]
        public string? Sha256 { get; set; }

        [StringLength(450)]
        public string RequestedByUserId { get; set; } = null!;

        public DateTime RequestedAt { get; set; }

        [StringLength(450)]
        public string? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        public virtual Beekeeper? Beekeeper { get; set; }
    }

    // Append-only, hash-chained audit row (D-28). Written only by Services/Audit/AuditChain with raw SQL;
    // EF reads it but never inserts, updates or deletes it.
    public class AuditEntry
    {
        [Key]
        public long AuditId { get; set; }
        public DateTime At { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public int? OrgId { get; set; }
        public string Action { get; set; } = null!;
        public string Entity { get; set; } = null!;
        public string? EntityKey { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Flags { get; set; }
        public string? ClientIp { get; set; }
        public string PrevHash { get; set; } = null!;
        public string Hash { get; set; } = null!;
    }

    public static class OrgStatus
    {
        public const string Pending = "Pending";
        public const string Verified = "Verified";
        public const string Suspended = "Suspended";
    }

    public static class MembershipStatus
    {
        public const string Active = "Active";
        public const string Revoked = "Revoked";
    }

    public static class RegistryStatus
    {
        public const string Unverified = "Unverified";
        public const string Pending = "Pending";
        public const string Verified = "Verified";
        public const string Rejected = "Rejected";
    }

    // Capability codes (table Capabilities)
    public static class Capabilities
    {
        public const string Beekeeping = "Beekeeping";
        public const string Association = "Association";
        public const string CentralAssociation = "CentralAssociation";
        public const string Processing = "Processing";
        public const string Packing = "Packing";
        public const string Laboratory = "Laboratory";
        public const string Wholesale = "Wholesale";
        public const string Retail = "Retail";
        public const string PublicAuthority = "PublicAuthority";
        public const string PlatformAdmin = "PlatformAdmin";
    }

    // Org role codes (table OrgRoles)
    public static class OrgRoles
    {
        public const string OrgAdmin = "OrgAdmin";
        public const string Staff = "Staff";
        public const string FieldInspector = "FieldInspector";
        public const string SampleAcceptanceOfficer = "SampleAcceptanceOfficer";
        public const string QualityApprover = "QualityApprover";
        public const string Analyst = "Analyst";
        public const string LabManager = "LabManager";

        public static readonly string[] All = { OrgAdmin, Staff, FieldInspector, SampleAcceptanceOfficer, QualityApprover, Analyst, LabManager };

        // P1: fixed sets; P2 replaces them with the per-CCP responsibility configuration (D-25)
        public static readonly string[] CcpEnter = { Staff, FieldInspector, SampleAcceptanceOfficer };
        public static readonly string[] CcpApprove = { QualityApprover };
    }

    public static class OrgDocumentKinds
    {
        public static readonly string[] All = { "Licence", "Accreditation", "TaxCertificate", "RegistryProof", "Other" };
    }
}
