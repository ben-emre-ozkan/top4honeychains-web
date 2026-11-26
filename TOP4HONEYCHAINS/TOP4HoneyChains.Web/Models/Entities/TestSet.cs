using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-112 (migration 037): an account of the live site's test set (create-test-set). AspNetUsers belongs to ASP.NET Identity, so
    // the flag of an account is its row here. SetNo: the tester's set of a consumer account (null for the shared cast).
    public class TestSetAccount
    {
        [Key]
        [StringLength(450)]
        public string UserId { get; set; } = null!;

        public int? SetNo { get; set; }

        [StringLength(20)]
        public string Kind { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }

    public static class TestSetAccountKind
    {
        public const string Operator = "Operator";
        public const string Member = "Member";
        public const string Beekeeper = "Beekeeper";
        public const string Consumer = "Consumer";
    }
}
