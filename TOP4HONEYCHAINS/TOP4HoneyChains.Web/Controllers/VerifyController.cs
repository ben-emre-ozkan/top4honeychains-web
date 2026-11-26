using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Public "verify report" page (D-42): anyone enters a lab report code and sees the report's SHA-256
    // hash and approval status. No personal data: no lot, beekeeper, apiary or user is shown.
    // Public pages (D-47)
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [RequireOrgModel]
    [Route("verify-report")]
    public class VerifyController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public VerifyController(TOP4HoneyChainsContext context) => _context = context;

        public record ReportInfo(string Code, string Status, string? ReportNumber, DateOnly? IssuedOn, string Laboratory,
            bool Accredited, string? AccreditationNumber, bool InHouse, string? Sha256, DateTime? ApprovedAt);

        [HttpGet("")]
        public async Task<IActionResult> Index(string? code)
        {
            code = code?.Trim().ToUpperInvariant();
            ViewBag.Code = code;
            ViewBag.Searched = !string.IsNullOrEmpty(code);
            if (string.IsNullOrEmpty(code) || code.Length > 20)
                return View((ReportInfo?)null);
            var r = await _context.LabReports.AsNoTracking().Include(x => x.LabOrg)
                .FirstOrDefaultAsync(x => x.ReportCode == code);
            // Only submitted or approved reports are public; a draft is still being written
            // C4 (D-84): only an approved report is confirmed publicly; a draft or submitted one is "not found"
            if (r == null || r.Status != LabReportStatus.Approved)
                return View((ReportInfo?)null);
            // D-112: a report of the test set's laboratory shows the TEST banner
            if (r.LabOrg!.IsTestData)
                TestData.MarkPage(HttpContext);
            return View(new ReportInfo(r.ReportCode, r.Status, r.ReportNumber, r.IssuedOn, r.LabOrg!.DisplayName,
                r.IsAccredited, r.AccreditationNumber, r.InHouse, r.Sha256, r.ApprovedAt));
        }
    }
}
