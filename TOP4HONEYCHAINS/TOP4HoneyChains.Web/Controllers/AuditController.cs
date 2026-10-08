using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Read-only view of the append-only audit log and its hash-chain check (D-28)
    // Available with or without the organization model: the audit log is on from go-live (D-34)
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class AuditController : Controller
    {
        private const int PageSize = 50;
        private readonly TOP4HoneyChainsContext _context;
        private readonly ApplicationDbContext _identity;
        private readonly AuditReadable _readable;

        public AuditController(TOP4HoneyChainsContext context, ApplicationDbContext identity, AuditReadable readable)
        {
            _context = context;
            _identity = identity;
            _readable = readable;
        }

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
            var entries = await q.OrderByDescending(a => a.AuditId).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

            // D-73: readable rows; user and organization ids of the page are resolved to names in one query each
            var (userIds, orgIds) = AuditReadable.References(entries);
            var users = await _identity.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Id);
            var orgs = await _context.Organizations.AsNoTracking().Where(o => orgIds.Contains(o.OrgId)).ToDictionaryAsync(o => o.OrgId, o => o.DisplayName);
            var statuses = await _context.AssignmentStatuses.AsNoTracking().ToDictionaryAsync(s => s.AssignmentStatusId, s => s.AssignmentStatusName);
            var ccps = await _context.CriticalControlPoints.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Code);
            var parameters = await _context.CriticalControlPointParameters.AsNoTracking().ToDictionaryAsync(p => p.ParameterId, p => p.ParameterName ?? "#" + p.ParameterId);
            return View(_readable.Build(entries, new AuditLookups(users, orgs, statuses, ccps, parameters)));
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
