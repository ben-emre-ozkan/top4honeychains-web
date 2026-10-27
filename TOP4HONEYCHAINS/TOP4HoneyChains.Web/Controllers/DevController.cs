using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P4 (D-83): the scene gallery for review in Development only (/Dev/Scenes). Every other environment answers 404.
    [AllowAnonymous]
    public class DevController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public DevController(IWebHostEnvironment env) => _env = env;

        public IActionResult Scenes()
        {
            if (!_env.IsDevelopment())
                return NotFound();
            return View();
        }
    }
}
