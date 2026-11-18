using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-87 (migration 029): a consumer sign-in account. No role, no membership; organizations only ever see the pseudonym.
    public class ConsumerAccount
    {
        [Key, StringLength(450)]
        public string UserId { get; set; } = null!;

        [StringLength(12)]
        public string Pseudonym { get; set; } = null!;

        [StringLength(5)]
        public string Language { get; set; } = "tr";

        public DateTime AgeConfirmedAt { get; set; }

        public int? NoticeId { get; set; }

        [StringLength(40)]
        public string? NoticeVersion { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastSignInAt { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = ConsumerAccountStatus.Active;
    }

    public static class ConsumerAccountStatus
    {
        public const string Active = "Active";
        public const string Locked = "Locked";
    }

    // a honey the consumer saved explicitly
    public class ConsumerSavedBatch
    {
        [StringLength(450)]
        public string ConsumerUserId { get; set; } = null!;

        public int FinalHoneyProductionId { get; set; }

        public DateTime SavedAt { get; set; }

        public virtual FinalHoneyProduction? Batch { get; set; }
    }

    // feedback to the organization that owns the batch, with its answer
    public class ConsumerFeedback
    {
        public int FeedbackId { get; set; }

        [StringLength(450)]
        public string? ConsumerUserId { get; set; }

        [StringLength(12)]
        public string Pseudonym { get; set; } = null!;

        public int FinalHoneyProductionId { get; set; }

        public int OrgId { get; set; }

        [StringLength(30)]
        public string Category { get; set; } = null!;

        [StringLength(1000)]
        public string Message { get; set; } = null!;

        [StringLength(5)]
        public string? Language { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = ConsumerFeedbackStatus.New;

        public DateTime? ReadAt { get; set; }

        [StringLength(450)]
        public string? ReadByUserId { get; set; }

        [StringLength(1000)]
        public string? Answer { get; set; }

        public DateTime? AnsweredAt { get; set; }

        [StringLength(450)]
        public string? AnsweredByUserId { get; set; }

        public DateTime? ClosedAt { get; set; }

        [StringLength(450)]
        public string? ClosedByUserId { get; set; }

        public virtual FinalHoneyProduction? Batch { get; set; }
        public virtual Organization? Org { get; set; }
    }

    public static class ConsumerFeedbackStatus
    {
        public const string New = "New";
        public const string Read = "Read";
        public const string Answered = "Answered";
        public const string Closed = "Closed";
        public static readonly string[] All = { New, Read, Answered, Closed };
    }

    public static class ConsumerFeedbackCategory
    {
        public const string TasteSmell = "TasteSmell";
        public const string TextureCrystallization = "TextureCrystallization";
        public const string PackagingLabel = "PackagingLabel";
        public const string SuspectedFake = "SuspectedFake";
        public const string Other = "Other";
        public static readonly string[] All = { TasteSmell, TextureCrystallization, PackagingLabel, SuspectedFake, Other };
    }
}
