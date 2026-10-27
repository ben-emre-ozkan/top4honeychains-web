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
}
