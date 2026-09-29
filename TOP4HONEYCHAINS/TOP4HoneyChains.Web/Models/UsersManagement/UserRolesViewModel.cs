namespace TOP4HoneyChains.Web.Models.UsersManagement
{
    public class UserRolesViewModel
    {
        public string? UserId { get; set; }

        public string? UserName { get; set; }

        public List<RoleSelection> Roles { get; set; } = new List<RoleSelection>();
    }
}
