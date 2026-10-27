using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P1 (D78-SPEC 2.1, 2.5): where the new roles may be held
    public static class MembershipRules
    {
        public const string RegistrationNeedsCapability =
            "The beekeeper registration officer role needs an organization with the Beekeeper registration capability.";

        // an error when the role may not be given in an organization with these capabilities, otherwise null
        public static string? Check(string orgRole, IEnumerable<string> capabilities) =>
            orgRole == OrgRoles.RegistrationOfficer && !capabilities.Contains(Capabilities.BeekeeperRegistration)
                ? RegistrationNeedsCapability
                : null;

        // a verified organization without an active Compliance Officer gets a warning (required from P3, O-9)
        public static bool MissingComplianceOfficer(string orgStatus, IEnumerable<Membership> members) =>
            orgStatus == OrgStatus.Verified
            && !members.Any(m => m.OrgRole == OrgRoles.ComplianceOfficer && m.Status == MembershipStatus.Active);
    }
}
