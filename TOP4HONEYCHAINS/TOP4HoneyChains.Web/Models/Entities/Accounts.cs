using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-78 P1 (D78-SPEC 3.7, migration 021): the sign-in account of a beekeeper (one each). Written only when a registration
    // officer issues the sign-in; the legacy implicit link (BeekeeperId = AspNetUsers.Id) is not used for access.
    public class BeekeeperAccount
    {
        [Key, StringLength(450)]
        public string BeekeeperId { get; set; } = null!;

        [StringLength(450)]
        public string UserId { get; set; } = null!;

        public DateTime LinkedAt { get; set; }

        [StringLength(450)]
        public string? LinkedByUserId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = BeekeeperAccountStatus.Active;

        public virtual Beekeeper? Beekeeper { get; set; }
    }

    public static class BeekeeperAccountStatus
    {
        public const string Active = "Active";
        public const string Suspended = "Suspended";
    }

    // One attestation of a beekeeper by a registration officer (or a platform administrator). Append-only except the review.
    public class BeekeeperRegistration
    {
        [Key]
        public int RegistrationId { get; set; }

        [StringLength(450)]
        public string BeekeeperId { get; set; } = null!;

        public int RegisteringOrgId { get; set; }

        [StringLength(450)]
        public string OfficerUserId { get; set; } = null!;

        public DateTime At { get; set; }

        [StringLength(20)]
        public string Kind { get; set; } = RegistrationKind.New;

        [StringLength(20)]
        public string? RegistryNumber { get; set; }

        public bool RegistryFormatOk { get; set; }

        public bool? RegistryProvinceMatches { get; set; }

        [StringLength(300)]
        public string? ProvinceMismatchReason { get; set; }

        [StringLength(64)]
        public string? PhoneE164Hash { get; set; }

        [StringLength(20)]
        public string Method { get; set; } = null!;

        public long? CodeId { get; set; }

        [StringLength(300)]
        public string? FallbackReason { get; set; }

        [StringLength(100)]
        public string? EvidenceStoredFileName { get; set; }

        [StringLength(260)]
        public string? EvidenceOriginalFileName { get; set; }

        [StringLength(64)]
        public string? EvidenceSha256 { get; set; }

        public bool OfficerAttestation { get; set; }

        public int? AttestationNoticeId { get; set; }

        [StringLength(20)]
        public string EvidenceStatus { get; set; } = Entities.EvidenceStatus.Accepted;

        [StringLength(450)]
        public string? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? ReviewReason { get; set; }

        public virtual Beekeeper? Beekeeper { get; set; }
        public virtual Organization? RegisteringOrg { get; set; }
    }

    public static class RegistrationKind
    {
        public const string New = "New";
        public const string Reverification = "Reverification";
        public const string IdentityChange = "IdentityChange";
    }

    public static class EvidenceStatus
    {
        public const string Accepted = "Accepted";
        public const string PendingReview = "PendingReview";
        public const string Rejected = "Rejected";
    }

    // Sign-in details issued to an account; while the latest issue has no PasswordChangedAt the user must change the password
    public class UserCredentialIssue
    {
        [Key]
        public int IssueId { get; set; }

        [StringLength(450)]
        public string UserId { get; set; } = null!;

        [StringLength(450)]
        public string? IssuedByUserId { get; set; }

        public DateTime IssuedAt { get; set; }

        [StringLength(20)]
        public string Channel { get; set; } = CredentialChannel.Sms;

        public long? SmsId { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? PasswordChangedAt { get; set; }
    }

    public static class CredentialChannel
    {
        public const string Sms = "Sms";
        public const string Manual = "Manual";
    }

    // Settings of an account kept outside the Identity tables (D78-SPEC 3.7)
    public class UserSetting
    {
        [Key, StringLength(450)]
        public string UserId { get; set; } = null!;

        [StringLength(5)]
        public string? Language { get; set; }

        public DateTime? PhoneVerifiedAt { get; set; }

        public long? PhoneCodeId { get; set; }

        public bool ShowAnimations { get; set; } = true;

        // D-78 P5 (027, D-84): the roles whose first-use tour the user has seen, comma-separated
        [StringLength(400)]
        public string? ToursSeen { get; set; }
    }
}
