using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-56: one right of an organization role (Enter / Approve / Read) in an area (CCP read, intake, samples, …).
    // Who enters and approves each CCP stays in the CCP responsibility matrix.
    public class RolePermission
    {
        [StringLength(30)]
        public string Area { get; set; } = null!;

        [StringLength(20)]
        public string AreaKey { get; set; } = "";

        [StringLength(10)]
        public string Action { get; set; } = null!;

        [StringLength(40)]
        public string OrgRole { get; set; } = null!;

        public DateTime UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedByUserId { get; set; }
    }

    public static class PermissionAreas
    {
        public const string Ccp = "Ccp";
        public const string Intake = "Intake";
        public const string Samples = "Samples";
        public const string LabResults = "LabResults";
        public const string Shipments = "Shipments";
        public const string Storefront = "Storefront";
        public const string Recall = "Recall";
        public const string Finalize = "Finalize";
        // D-78 (migration 018): register beekeepers; accept or reject fallback evidence (signed photo)
        public const string Registration = "Registration";
        public const string Evidence = "Evidence";

        public const string Enter = "Enter";
        public const string Approve = "Approve";
        public const string Read = "Read";

        // the actions each area offers on the permission screen
        public static readonly (string Area, string[] Actions)[] Functions =
        {
            (Intake, new[] { Enter, Read }),
            (Samples, new[] { Enter, Approve, Read }),
            (LabResults, new[] { Enter, Approve, Read }),
            (Shipments, new[] { Enter, Approve, Read }),
            (Storefront, new[] { Enter, Read }),
            (Recall, new[] { Read }),
            (Finalize, new[] { Enter, Approve }),
            (Registration, new[] { Enter, Read }),
            (Evidence, new[] { Approve })
        };
    }
}
