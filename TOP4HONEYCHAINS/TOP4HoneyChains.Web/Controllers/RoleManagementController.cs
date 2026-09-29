using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RoleManagementController : Controller
    {
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
        public async Task<IActionResult> Edit(string id, string roleName)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role != null)
            {
                role.Name = roleName;
                await _roleManager.UpdateAsync(role);
                TempData["success"] = "Role updated successfully.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Role deleted successfully.";
            }

            return RedirectToAction("Index");
        }
    }
}
