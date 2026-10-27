using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P3 (D78-SPEC 4.8, D-82): the Compliance section of My organization, for the organization's Compliance Officer (also
    // the platform organization's for platform registrations). Read-only views plus the CO's own records: sampling-quality reviews
    // and data-subject requests. Evidence (signed photos) is accepted where it is shown (registration and delivery pages, My work).
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class ComplianceController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly ComplianceService _compliance;
        private Permissions Perm => HttpContext.RequestServices.GetRequiredService<Permissions>();

        public ComplianceController(TOP4HoneyChainsContext context, ICurrentOrg current, ComplianceService compliance)
        {
            _context = context;
            _current = current;
            _compliance = compliance;
        }

        // the Compliance Officer of the active organization (a fixed rule, like the four-eyes rule)
        private async Task<ActiveMembership?> OfficerAsync() =>
            !_current.IsPlatformAdmin && await _current.GetAsync() is { } m && m.OrgRole == OrgRoles.ComplianceOfficer ? m : null;

        public async Task<IActionResult> Index()
        {
            var m = await OfficerAsync();
            if (m == null)
                return NotFound();
            ViewBag.Org = await _context.Organizations.AsNoTracking().Include(o => o.Documents).FirstAsync(o => o.OrgId == m.OrgId);
            ViewBag.Quality = await _compliance.QualityItemsAsync(m.OrgId);
            ViewBag.Requests = await _compliance.RequestsAsync(m.OrgId);
            ViewBag.Flags = await _compliance.ApprovalFlagsAsync(m.OrgId);
            ViewBag.Rights = PermissionAreas.Functions.Select(f => (f.Area, f.Actions.Select(a => (a, Perm.Roles(f.Area, a))).ToList())).ToList();
            ViewBag.Audit = Perm.Allows(PermissionAreas.OrgAudit, PermissionAreas.Read, m.OrgRole) ? await _compliance.AuditAsync(m.OrgId) : null;
            ViewBag.PendingEvidence = await _context.LotIntakes.CountAsync(i => i.ReceivingOrgId == m.OrgId && i.EvidenceStatus == EvidenceStatus.PendingReview)
                                      + await _context.BeekeeperRegistrations.CountAsync(r => r.RegisteringOrgId == m.OrgId && r.EvidenceStatus == EvidenceStatus.PendingReview);
            ViewBag.UserId = _current.UserId;
            ViewBag.StaleDays = _compliance.Options.StaleApprovalDays;
            ViewBag.Controller = "Compliance";
            ViewBag.Action = "Index";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(string? subjectType, int subjectId, string? outcome, string? finding, string? action)
        {
            var m = await OfficerAsync();
            if (m == null)
                return NotFound();
            var error = await _compliance.ReviewAsync(m.OrgId, _current.UserId!, subjectType, subjectId, outcome, finding, action);
            TempData[error == null ? "Success" : "Error"] = error ?? "Review recorded.";
            return Redirect("/Compliance#quality");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordRequest(string? subjectName, string? beekeeperNumber, string? kind, string? channel, DateOnly? receivedOn, string? details, string? consentPurpose)
        {
            var m = await OfficerAsync();
            if (m == null)
                return NotFound();
            var error = await _compliance.RecordRequestAsync(m.OrgId, _current.UserId!, subjectName, beekeeperNumber, kind, channel, receivedOn, details, consentPurpose);
            TempData[error == null ? "Success" : "Error"] = error ?? "Request recorded; the answer is due within 30 days.";
            return Redirect("/Compliance#requests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseRequest(int requestId, string? status, string? outcome)
        {
            var m = await OfficerAsync();
            if (m == null)
                return NotFound();
            var error = await _compliance.CloseRequestAsync(m.OrgId, _current.UserId!, requestId, status, outcome);
            TempData[error == null ? "Success" : "Error"] = error ?? "Request closed.";
            return Redirect("/Compliance#requests");
        }
    }
}
