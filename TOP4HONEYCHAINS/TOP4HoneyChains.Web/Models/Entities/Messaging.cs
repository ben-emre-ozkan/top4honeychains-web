using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-78 P1 (D78-SPEC 3.4, migration 019): one SMS sent through the provider abstraction. The full number is never stored
    // here; with a real provider codes and passwords in BodyStored are masked. Append-only except the delivery status.
    public class SmsMessage
    {
        [Key]
        public long SmsId { get; set; }

        [StringLength(30)]
        public string Purpose { get; set; } = null!;

        [StringLength(30)]
        public string? SubjectType { get; set; }

        [StringLength(64)]
        public string? SubjectKey { get; set; }

        [StringLength(20)]
        public string ToPhoneMasked { get; set; } = null!;

        [StringLength(64)]
        public string ToPhoneHash { get; set; } = null!;

        [StringLength(5)]
        public string Language { get; set; } = null!;

        [StringLength(800)]
        public string BodyStored { get; set; } = null!;

        public byte Segments { get; set; }

        [StringLength(30)]
        public string Provider { get; set; } = null!;

        [StringLength(64)]
        public string? ProviderMessageId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = SmsStatus.Queued;

        public DateTime? StatusAt { get; set; }

        [StringLength(400)]
        public string? Error { get; set; }

        [StringLength(450)]
        public string? SentByUserId { get; set; }

        public int? OrgId { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    // A 6-digit one-time code: only its HMAC is stored, bound to a purpose, a subject and the hash of the confirmed content
    public class OneTimeCode
    {
        [Key]
        public long CodeId { get; set; }

        public long SmsId { get; set; }

        [StringLength(30)]
        public string Purpose { get; set; } = null!;

        [StringLength(30)]
        public string SubjectType { get; set; } = null!;

        [StringLength(64)]
        public string SubjectKey { get; set; } = null!;

        [StringLength(64)]
        public string ContentSha256 { get; set; } = null!;

        [StringLength(64)]
        public string CodeHash { get; set; } = null!;

        [MaxLength(16)]
        public byte[] Salt { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public byte Attempts { get; set; }

        public DateTime? LastAttemptAt { get; set; }

        public DateTime? UsedAt { get; set; }

        [StringLength(450)]
        public string? UsedByUserId { get; set; }

        public DateTime? VoidedAt { get; set; }

        [StringLength(30)]
        public string? VoidReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual SmsMessage? Sms { get; set; }
    }

    public static class SmsPurpose
    {
        public const string Registration = "Registration";
        public const string Credentials = "Credentials";
        public const string IntakeConfirm = "IntakeConfirm";
        public const string PasswordReset = "PasswordReset";
        public const string NoticeConfirm = "NoticeConfirm";
        public const string PhoneVerify = "PhoneVerify";
        public const string Info = "Info";
        // D-78 P3 (M6): a confirmed correction of a delivery, for the beekeeper's information
        public const string DeliveryCorrected = "DeliveryCorrected";
    }

    public static class SmsStatus
    {
        public const string Queued = "Queued";
        public const string Sent = "Sent";
        public const string Delivered = "Delivered";
        public const string Failed = "Failed";
        public const string Expired = "Expired";
        public const string Fake = "Fake";
        public const string Logged = "Logged";
    }

    public static class CodeVoidReason
    {
        public const string Resent = "Resent";
        public const string TooManyAttempts = "TooManyAttempts";
        public const string ContentChanged = "ContentChanged";
        public const string Expired = "Expired";
    }

    // D-78 P1 (D78-SPEC 3.5, migration 020): a versioned text (privacy notice, consent text, attestation) in one language.
    // Never changed after publishing; a new version retires the previous one (RetiredAt).
    public class Notice
    {
        [Key]
        public int NoticeId { get; set; }

        [StringLength(30)]
        public string Kind { get; set; } = null!;

        [StringLength(20)]
        public string Audience { get; set; } = null!;

        // null = the platform's text; otherwise the organization as data controller
        public int? OrgId { get; set; }

        [StringLength(30)]
        public string? Purpose { get; set; }

        [StringLength(40)]
        public string Version { get; set; } = null!;

        [StringLength(5)]
        public string Language { get; set; } = null!;

        public string Body { get; set; } = null!;

        [StringLength(64)]
        public string BodySha256 { get; set; } = null!;

        // a draft awaiting review (O-14); shown with a DRAFT mark
        public bool IsDraft { get; set; }

        public DateTime PublishedAt { get; set; }

        [StringLength(450)]
        public string? PublishedByUserId { get; set; }

        public DateTime? RetiredAt { get; set; }

        public virtual Organization? Org { get; set; }

        // SHA-256 of the text as SQL Server stores it (nvarchar = UTF-16LE), lower-case hex, as computed by migration 020
        public static string Hash(string body) =>
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.Unicode.GetBytes(body))).ToLowerInvariant();
    }

    // Append-only proof that a user or a beekeeper accepted one version of a notice
    public class NoticeAcceptance
    {
        [Key]
        public long AcceptanceId { get; set; }

        public int NoticeId { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(450)]
        public string? BeekeeperId { get; set; }

        public int? OrgId { get; set; }

        [StringLength(20)]
        public string Method { get; set; } = null!;

        public long? CodeId { get; set; }

        [StringLength(100)]
        public string? EvidenceStoredFileName { get; set; }

        [StringLength(64)]
        public string? EvidenceSha256 { get; set; }

        [StringLength(450)]
        public string RecordedByUserId { get; set; } = null!;

        public DateTime At { get; set; }

        [StringLength(45)]
        public string? ClientIp { get; set; }

        public virtual Notice? Notice { get; set; }
    }

    public static class NoticeKind
    {
        public const string PrivacyNotice = "PrivacyNotice";
        public const string ConsentText = "ConsentText";
        public const string TermsOfUse = "TermsOfUse";
        public const string Attestation = "Attestation";
        public static readonly string[] All = { PrivacyNotice, ConsentText, TermsOfUse, Attestation };
    }

    public static class NoticeAudience
    {
        public const string Beekeeper = "Beekeeper";
        public const string Member = "Member";
        public const string All = "All";
        public static readonly string[] Values = { Beekeeper, Member, All };
    }

    public static class AcceptanceMethod
    {
        public const string Click = "Click";
        public const string SmsCode = "SmsCode";
        public const string SignedPhoto = "SignedPhoto";
        public const string Paper = "Paper";
    }
}
