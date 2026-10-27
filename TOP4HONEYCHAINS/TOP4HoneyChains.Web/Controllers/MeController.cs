using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P1 (D78-SPEC 2.4, O-15): the beekeeper area. P1 has only this start page (password, notices, sign-out); the
    // portal screens come in P3. Only an active beekeeper account while Features:BeekeeperAccounts is on; anyone else gets 404.
    [Authorize(Policy = AuthPolicies.SignedIn)]
    public class MeController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly UserManager<ApplicationUser> _users;
        private readonly FeatureFlags _flags;

        public MeController(TOP4HoneyChainsContext context, UserManager<ApplicationUser> users, IOptions<FeatureFlags> flags)
        {
            _context = context;
            _users = users;
            _flags = flags.Value;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _users.GetUserId(User);
            var account = !_flags.BeekeeperAccounts || userId == null ? null : await _context.BeekeeperAccounts.AsNoTracking()
                .Include(a => a.Beekeeper)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Status == BeekeeperAccountStatus.Active);
            if (account == null)
                return NotFound();
            return View(account.Beekeeper);
        }
    }
}
