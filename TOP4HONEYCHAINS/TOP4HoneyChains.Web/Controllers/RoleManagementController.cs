using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RoleManagementController : Controller
    {
        // D-75: global roles the code relies on: the platform administrator (every admin page) and Beekeeper (beekeeper
        // accounts without the organization model). Renaming or deleting them would lock administrators out.
        public static readonly IReadOnlySet<string> Protected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { Services.OrgModel.CurrentOrg.AdminRole, "Beekeeper" };

        public static bool IsProtected(string? name) => name != null && Protected.Contains(name);

        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManagementController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();

            ViewBag.Controller = "Role Management";
            ViewBag.Action = "Roles";

            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                TempData["success"] = "Role created successfully.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string roleName)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role != null && (IsProtected(role.Name) || IsProtected(roleName)))
            {
                TempData["error"] = "This role is used by the platform and cannot be renamed or deleted.";
                return RedirectToAction("Index");
            }
            if (role != null && !string.IsNullOrWhiteSpace(roleName))
            {
                role.Name = roleName;
                await _roleManager.UpdateAsync(role);
                TempData["success"] = "Role updated successfully.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role != null && IsProtected(role.Name))
            {
                TempData["error"] = "This role is used by the platform and cannot be renamed or deleted.";
                return RedirectToAction("Index");
            }
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Role deleted successfully.";
            }

            return RedirectToAction("Index");
        }
    }
}
