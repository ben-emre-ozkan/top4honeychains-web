using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // Role chip of the top bar and My work (D-66): the active membership and the others to switch to
    public record RoleChipModel(ActiveMembership Active, IReadOnlyList<MembershipOption> Memberships);
}
