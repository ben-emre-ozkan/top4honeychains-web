using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-88: what the platform administrator still needs of the old data-entry menus, read-only: the lots and batches of every
    // organization with their owner, progress and links to the read-only pages (details, control sheet, flow, Honey Story).
    // Entering and approving stay with the organizations; corrections go through correction records (D-35).
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class RecordsController : Controller
    {
        public const int Max = 200;

        public record BatchRow(int Id, string? Title, string? Owner, DateTime? Date, decimal? Kg, bool Finalized, Guid Qr, int Done, int Ccps);
        public record LotRow(int Id, string? Owner, int? HoneyTypeId, string? HoneyType, DateOnly? Date, decimal? Kg, int Done, int Ccps, int Batches);
        public record OrgOption(int OrgId, string Name);

        private readonly TOP4HoneyChainsContext _context;

        public RecordsController(TOP4HoneyChainsContext context) => _context = context;

        public async Task<IActionResult> Index(string? tab, int? org)
        {
            tab = tab == "lots" ? "lots" : "batches";
            ViewBag.Tab = tab;
            ViewBag.Org = org;
            ViewBag.Orgs = await _context.Organizations.AsNoTracking().Where(o => !o.IsPlatform)
                .Where(o => _context.RawHoneyProductions.Any(r => r.OwnerOrgId == o.OrgId) || _context.FinalHoneyProductions.Any(f => f.OwnerOrgId == o.OrgId))
                .OrderBy(o => o.DisplayName).Select(o => new OrgOption(o.OrgId, o.DisplayName)).ToListAsync();

            if (tab == "batches")
            {
                var batches = _context.FinalHoneyProductions.AsNoTracking();
                if (org != null)
                    batches = batches.Where(f => f.OwnerOrgId == org);
                ViewBag.Batches = await batches.OrderByDescending(f => f.FinalHoneyProductionId).Take(Max)
                    .Select(f => new BatchRow(f.FinalHoneyProductionId, f.FinalHoneyProductionTitle,
                        _context.Organizations.Where(o => o.OrgId == f.OwnerOrgId).Select(o => o.DisplayName).FirstOrDefault(),
                        f.FinalHoneyProductionDate, f.FinalHoneyProductionTotalAmount, f.FinalHoneyProductionIsFinalized, f.FinalHoneyProductionQR,
                        f.FinalHoneyCCPAssignments.Count(a => (a.FinalHoneyCCPAssignmentStatus == AssignmentStatusIds.Completed || a.FinalHoneyCCPAssignmentStatus == AssignmentStatusIds.NotApplicable) && a.ApprovedAt != null),
                        f.FinalHoneyCCPAssignments.Count()))
                    .ToListAsync();
            }
            else
            {
                var lots = _context.RawHoneyProductions.AsNoTracking();
                if (org != null)
                    lots = lots.Where(r => r.OwnerOrgId == org);
                ViewBag.Lots = await lots.OrderByDescending(r => r.RawHoneyProductionId).Take(Max)
                    .Select(r => new LotRow(r.RawHoneyProductionId,
                        _context.Organizations.Where(o => o.OrgId == r.OwnerOrgId).Select(o => o.DisplayName).FirstOrDefault(),
                        r.RawHoneyProductionHoneyType, r.RawHoneyProductionHoneyTypeNavigation!.HoneyTypeTitle, r.RawHoneyProductionDate, r.RawHoneyProductionAmount,
                        r.RawHoneyCCPAssignments.Count(a => (a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.Completed || a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.NotApplicable) && a.ApprovedAt != null),
                        r.RawHoneyCCPAssignments.Count(), r.FinalHoneyProductionDetails.Count()))
                    .ToListAsync();
            }
            ViewBag.Controller = "Records";
            return View();
        }
    }

    // D-88: one page with the reference lists the platform administrator maintains (Set-up → Reference data)
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class ReferenceDataController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Controller = "ReferenceData";
            return View();
        }
    }
}
