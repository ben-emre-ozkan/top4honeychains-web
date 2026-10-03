using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Read-only view of the append-only audit log and its hash-chain check (D-28)
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class AuditController : Controller
    {
        private const int PageSize = 50;
        private readonly TOP4HoneyChainsContext _context;

        public AuditController(TOP4HoneyChainsContext context) => _context = context;

        public async Task<IActionResult> Index(int page = 1, string? entity = null, string? user = null, string? act = null, string? flag = null)
        {
            var q = _context.AuditEntries.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(entity))
                q = q.Where(a => a.Entity == entity);
            if (!string.IsNullOrWhiteSpace(user))
                q = q.Where(a => a.UserName == user);
            if (!string.IsNullOrWhiteSpace(act))
                q = q.Where(a => a.Action == act);
            if (!string.IsNullOrWhiteSpace(flag))
                q = q.Where(a => a.Flags != null && a.Flags.Contains(flag));

            page = Math.Max(1, page);
            ViewBag.Total = await q.CountAsync();
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Filter = new { entity, user, act, flag };
            ViewBag.Entities = await _context.AuditEntries.Select(a => a.Entity).Distinct().OrderBy(e => e).ToListAsync();
            ViewBag.Controller = "Audit log";
            ViewBag.Action = "Index";
            return View(await q.OrderByDescending(a => a.AuditId).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify()
        {
            var result = await AuditChain.VerifyAsync(_context);
            TempData[result.Ok ? "Success" : "Error"] = "Hash chain: " + result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
