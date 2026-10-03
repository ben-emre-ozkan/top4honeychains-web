using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.ViewModels.Monitoring;
using TOP4HoneyChains.Web.Models.ViewModels.Timeline;
using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RawHoneyProductionsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly CcpIntegrityService _ccp;

        public RawHoneyProductionsController(TOP4HoneyChainsContext context, CcpIntegrityService ccp)
        {
            _context = context;
            _ccp = ccp;
        }

        // Lots used in a finalized batch are locked (D-14): show why and go back to the apiary's lots
        private async Task<IActionResult?> RedirectIfLockedAsync(int rawHoneyProductionId)
        {
            var reason = await _ccp.GetRawLotLockReasonAsync(rawHoneyProductionId);
            if (reason == null)
                return null;

            var apiaryId = await _context.RawHoneyProductions
                .Where(r => r.RawHoneyProductionId == rawHoneyProductionId)
                .Select(r => r.RawHoneyProductionApiaryId)
                .FirstAsync();
            TempData["Error"] = reason;
            return RedirectToAction(nameof(RawHoneyProductionByApiary), new { id = apiaryId });
        }

        public async Task<IActionResult> RawHoneyProductionByApiary(int id)
        {
            var apiary = await _context.Apiaries
                .Include(a => a.ApiaryBeekeeper)
                .FirstOrDefaultAsync(a => a.ApiaryId == id);

            if (apiary == null)
                return NotFound();

            var productions = await _context.RawHoneyProductions
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyProductionPeriod)
                .Include(r => r.RawHoneyProductionWholesaler)
                .Include(r => r.RawHoneyCCPAssignments)
                .ThenInclude(a => a.CCP)
                .Where(r => r.RawHoneyProductionApiaryId == id)
                .ToListAsync();

            var logLookup = await _context.CriticalControlPointMonitoringLogs
                .Where(l => l.RawHoneyProductionId != null)
                .Select(l => new { l.RawHoneyProductionId, l.CCPId })
                .Distinct()
                .ToListAsync();

            ViewBag.Apiary = apiary;
            ViewBag.LogLookup = logLookup;
            ViewBag.LockedLotIds = await _ccp.GetLockedRawLotIdsAsync(productions.Select(p => p.RawHoneyProductionId));


            ViewBag.Controller = "Raw Honey Production";
            ViewBag.Action = "Production by Apiary";

            return View(productions);
        }

        public async Task<IActionResult> Index()
        {
            var productions = await _context.RawHoneyProductions
                .Include(r => r.RawHoneyProductionApiary)
                    .ThenInclude(a => a.ApiaryBeekeeper)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyProductionPeriod)
                .Include(r => r.RawHoneyProductionWholesaler)
                .ToListAsync();


            ViewBag.Controller = "Raw Honey Production";
            ViewBag.Action = "Index";

            return View(productions);
        }

        public async Task<IActionResult> Create(int apiaryId)
        {

            var apiary = await _context.Apiaries
                .Include(a => a.ApiaryBeekeeper)
                .FirstOrDefaultAsync(a => a.ApiaryId == apiaryId);

            if (apiary == null)
                return NotFound();

            ViewBag.Apiary = apiary;

            // Apiary dropdown yok → sabit gelecek
            ViewBag.ApiaryId = apiaryId;

            // HoneyType select
            ViewData["RawHoneyProductionHoneyType"] =
                new SelectList(_context.HoneyTypes, "HoneyTypeId", "HoneyTypeTitle");

            // Period select
            ViewData["RawHoneyProductionPeriodId"] =
                new SelectList(_context.RawHoneyProductionPeriods, "RawHoneyProductionPeriodId", "RawHoneyProductionPeriodTitle");

            ViewData["RawHoneyProductionWholesalerId"] = new SelectList(
           _context.Wholesalers
               .Where(w => w.WholesalerIsActive)
               .OrderBy(w => w.WholesalerTitle),
           "WholesalerId",
           "WholesalerTitle"
       );


            ViewBag.Controller = "Raw Honey Production";
            ViewBag.Action = "Create";

            return View(new RawHoneyProduction
            {
                RawHoneyProductionApiaryId = apiaryId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RawHoneyProduction rawHoneyProduction)
        {
            // 🔎 Business Rule Kontrolü
            bool exists = await _context.RawHoneyProductions.AnyAsync(r =>
                r.RawHoneyProductionApiaryId == rawHoneyProduction.RawHoneyProductionApiaryId &&
                r.RawHoneyProductionPeriodId == rawHoneyProduction.RawHoneyProductionPeriodId &&
                r.RawHoneyProductionHoneyType == rawHoneyProduction.RawHoneyProductionHoneyType);

            if (exists)
            {
                ModelState.AddModelError(string.Empty,
                    "This apiary already has a production record for the selected period and honey type.");
            }


            if (ModelState.IsValid)
            {
                _context.Add(rawHoneyProduction);
                await _context.SaveChangesAsync();

                return RedirectToAction("RawHoneyProductionByApiary",
                        new { id = rawHoneyProduction.RawHoneyProductionApiaryId });
            }

            var apiary = await _context.Apiaries
                .Include(a => a.ApiaryBeekeeper)
                .FirstOrDefaultAsync(a => a.ApiaryId == rawHoneyProduction.RawHoneyProductionApiaryId);

            ViewBag.Apiary = apiary;

            ViewBag.ApiaryId = rawHoneyProduction.RawHoneyProductionApiaryId;

            ViewData["RawHoneyProductionHoneyType"] =
                new SelectList(_context.HoneyTypes, "HoneyTypeId", "HoneyTypeTitle", rawHoneyProduction.RawHoneyProductionHoneyType);

            ViewData["RawHoneyProductionPeriodId"] =
                new SelectList(_context.RawHoneyProductionPeriods, "RawHoneyProductionPeriodId", "RawHoneyProductionPeriodTitle", rawHoneyProduction.RawHoneyProductionPeriodId);

            ViewData["RawHoneyProductionWholesalerId"] =
       new SelectList(
           _context.Wholesalers.Where(w => w.WholesalerIsActive),
           "WholesalerId",
           "WholesalerTitle",
           rawHoneyProduction.RawHoneyProductionWholesalerId
       );



            ViewBag.Controller = "Raw Honey Production";
            ViewBag.Action = "Create";

            return View(rawHoneyProduction);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rawHoneyProduction = await _context.RawHoneyProductions
                .Include(r => r.RawHoneyProductionApiary)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyProductionPeriod)
                .Include(r => r.RawHoneyProductionWholesaler)
                .FirstOrDefaultAsync(r => r.RawHoneyProductionId == id);

            if (rawHoneyProduction == null)
            {
                return NotFound();
            }

            if (await RedirectIfLockedAsync(id.Value) is IActionResult locked)
                return locked;

            var apiary = await _context.Apiaries
                .Include(a => a.ApiaryBeekeeper)
                .FirstOrDefaultAsync(a => a.ApiaryId == rawHoneyProduction.RawHoneyProductionApiaryId);

            ViewBag.Apiary = apiary;

            ViewData["RawHoneyProductionApiaryId"] = new SelectList(_context.Apiaries, "ApiaryId", "ApiaryTitle", rawHoneyProduction.RawHoneyProductionApiaryId);
            ViewData["RawHoneyProductionHoneyType"] = new SelectList(_context.HoneyTypes, "HoneyTypeId", "HoneyTypeTitle", rawHoneyProduction.RawHoneyProductionHoneyType);
            ViewData["RawHoneyProductionPeriodId"] = new SelectList(_context.RawHoneyProductionPeriods, "RawHoneyProductionPeriodId", "RawHoneyProductionPeriodTitle", rawHoneyProduction.RawHoneyProductionPeriodId);
            ViewData["RawHoneyProductionWholesalerId"] =
        new SelectList(
            _context.Wholesalers.Where(w => w.WholesalerIsActive),
            "WholesalerId",
            "WholesalerTitle",
            rawHoneyProduction.RawHoneyProductionWholesalerId
        );


            ViewBag.Controller = "Raw Honey Production";
            ViewBag.Action = "Edit";

            return View(rawHoneyProduction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RawHoneyProduction rawHoneyProduction)
        {
            if (id != rawHoneyProduction.RawHoneyProductionId)
            {
                return NotFound();
            }

            if (!RawHoneyProductionExists(id))
                return NotFound();

            if (await RedirectIfLockedAsync(id) is IActionResult locked)
                return locked;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rawHoneyProduction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RawHoneyProductionExists(rawHoneyProduction.RawHoneyProductionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("RawHoneyProductionByApiary",
                        new { id = rawHoneyProduction.RawHoneyProductionApiaryId });

            }
            ViewData["RawHoneyProductionApiaryId"] = new SelectList(_context.Apiaries, "ApiaryId", "ApiaryTitle", rawHoneyProduction.RawHoneyProductionApiaryId);
            ViewData["RawHoneyProductionHoneyType"] = new SelectList(_context.HoneyTypes, "HoneyTypeId", "HoneyTypeTitle", rawHoneyProduction.RawHoneyProductionHoneyType);
            ViewData["RawHoneyProductionPeriodId"] = new SelectList(_context.RawHoneyProductionPeriods, "RawHoneyProductionPeriodId", "RawHoneyProductionPeriodTitle", rawHoneyProduction.RawHoneyProductionPeriodId);
            ViewData["RawHoneyProductionWholesalerId"] = new SelectList(
                _context.Wholesalers.Where(w => w.WholesalerIsActive),
                "WholesalerId", "WholesalerTitle", rawHoneyProduction.RawHoneyProductionWholesalerId);


            ViewBag.Controller = "Raw Honey Production";
            ViewBag.Action = "Edit";

            return View(rawHoneyProduction);
        }

        // GET: RawHoneyProductions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rawHoneyProduction = await _context.RawHoneyProductions
                .Include(r => r.RawHoneyProductionApiary)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyProductionPeriod)
                .FirstOrDefaultAsync(m => m.RawHoneyProductionId == id);
            if (rawHoneyProduction == null)
            {
                return NotFound();
            }

            if (await RedirectIfLockedAsync(id.Value) is IActionResult locked)
                return locked;

            ViewBag.Controller = "Raw Honey Production";
            ViewBag.Action = "Delete";

            return View(rawHoneyProduction);
        }

        // POST: RawHoneyProductions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (RawHoneyProductionExists(id) && await RedirectIfLockedAsync(id) is IActionResult locked)
                return locked;

            var rawHoneyProduction = await _context.RawHoneyProductions.FindAsync(id);
            if (rawHoneyProduction != null)
            {
                _context.RawHoneyProductions.Remove(rawHoneyProduction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("RawHoneyProductionByApiary",
                        new { id = rawHoneyProduction?.RawHoneyProductionApiaryId });
        }

        private bool RawHoneyProductionExists(int id)
        {
            return _context.RawHoneyProductions.Any(e => e.RawHoneyProductionId == id);
        }



        public async Task<IActionResult> ManageCCP(int id)
        {
            var production = await _context.RawHoneyProductions
                .Include(r => r.RawHoneyProductionApiary)
                .FirstOrDefaultAsync(r => r.RawHoneyProductionId == id);

            if (production == null)
                return NotFound();

            var allCCPs = await _context.CriticalControlPoints.ToListAsync();

            var assignedCCPs = await _context.RawHoneyCCPAssignments
                .Where(x => x.RawHoneyProductionId == id)
                .Select(x => x.RawHoneyCCPAssignmentCCP)
                .ToListAsync();

            var vm = new ManageRawHoneyCCPViewModel
            {
                RawHoneyProductionId = id,
                ApiaryId = production.RawHoneyProductionApiaryId ?? 0,
                Production = production,
                // Only CCPs that apply to this kind of record (D-17), plus any assigned earlier
                AllCCPs = await _context.CriticalControlPoints
                    .Where(c => c.AppliesTo == CcpAppliesTo.RawLot || assignedCCPs.Contains(c.Id))
                    .OrderBy(c => c.Id)
                    .ToListAsync(),
                SelectedCCPIds = assignedCCPs,
                LockReason = await _ccp.GetRawLotLockReasonAsync(id)
            };

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageCCP(ManageRawHoneyCCPViewModel model)
        {
            if (!RawHoneyProductionExists(model.RawHoneyProductionId))
                return NotFound();

            if (await RedirectIfLockedAsync(model.RawHoneyProductionId) is IActionResult locked)
                return locked;

            var existingAssignments = await _context.RawHoneyCCPAssignments
                .Where(x => x.RawHoneyProductionId == model.RawHoneyProductionId)
                .Include(x => x.CCP)
                .ToListAsync();

            var existingIds = existingAssignments
                .Select(x => x.RawHoneyCCPAssignmentCCP)
                .ToList();

            var selectedIds = model.SelectedCCPIds ?? new List<int>();

            // Only CCPs that apply to raw lots can be added (D-17)
            var applicableIds = await _context.CriticalControlPoints
                .Where(c => c.AppliesTo == CcpAppliesTo.RawLot)
                .Select(c => c.Id)
                .ToListAsync();
            if (selectedIds.Any(id => !existingIds.Contains(id) && !applicableIds.Contains(id)))
            {
                TempData["Error"] = "Only CCPs that apply to raw honey lots can be assigned. Nothing was changed.";
                return RedirectToAction(nameof(ManageCCP), new { id = model.RawHoneyProductionId });
            }

            // -------------------------
            // 1️⃣ EKLENECEKLER
            // -------------------------

            var toAdd = selectedIds
                .Where(id => !existingIds.Contains(id))
                .ToList();

            foreach (var ccpId in toAdd)
            {
                _context.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment
                {
                    RawHoneyProductionId = model.RawHoneyProductionId,
                    RawHoneyCCPAssignmentCCP = ccpId,
                    RawHoneyCCPAssignmentStatus = 1,
                    RawHoneyCCPAssignmentStartDate = DateTime.Now
                });
            }

            // -------------------------
            // 2️⃣ SİLİNECEKLER
            // -------------------------

            var toRemove = existingAssignments
                .Where(x => !selectedIds.Contains(x.RawHoneyCCPAssignmentCCP))
                .ToList();

            // Never lose recorded values (D-14): a CCP with values or a "Not applicable" record stays
            var blocked = new List<string>();
            foreach (var assignment in toRemove)
            {
                var hasMonitoringLog = await _context.CriticalControlPointMonitoringLogs
                    .AnyAsync(log =>
                        log.RawHoneyProductionId == model.RawHoneyProductionId &&
                        log.CCPId == assignment.RawHoneyCCPAssignmentCCP);

                if (hasMonitoringLog || assignment.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.NotApplicable)
                    blocked.Add(assignment.CCP?.Code ?? assignment.RawHoneyCCPAssignmentCCP.ToString());
            }

            if (blocked.Count > 0)
            {
                TempData["Error"] =
                    $"{string.Join(", ", blocked)} cannot be removed because values or a \"Not applicable\" record exist for this lot. Nothing was changed.";

                return RedirectToAction(nameof(ManageCCP),
                    new { id = model.RawHoneyProductionId });
            }

            _context.RawHoneyCCPAssignments.RemoveRange(toRemove);

            await _context.SaveChangesAsync();

            var production = await _context.RawHoneyProductions
                .FirstOrDefaultAsync(r => r.RawHoneyProductionId == model.RawHoneyProductionId);

            return RedirectToAction(nameof(RawHoneyProductionByApiary),
                new { id = production.RawHoneyProductionApiaryId });
        }



        public async Task<IActionResult> CCPOverview(int id)
        {
            var production = await _context.RawHoneyProductions
                .Include(r => r.RawHoneyProductionApiary)
                    .ThenInclude(a => a.ApiaryBeekeeper)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyProductionWholesaler)
                .FirstOrDefaultAsync(r => r.RawHoneyProductionId == id);

            if (production == null)
                return NotFound();

            var timeline = await _context.RawHoneyCCPAssignments
                .Include(x => x.CCP)
                .Include(x => x.AssignmentStatus)
                .Where(x => x.RawHoneyProductionId == id)
                .OrderBy(x => x.CCP.Id)
                .Select(x => new CCPTimelineItem
                {
                    CCPCode = x.CCP.Code,
                    CCPTitle = x.CCP.Title,
                    StatusName = x.AssignmentStatus.AssignmentStatusName,
                    StartDate = x.RawHoneyCCPAssignmentStartDate,
                    EndDate = x.RawHoneyCCPAssignmentEndDate
                })
                .ToListAsync();

            var model = new CCPOverviewViewModel
            {
                RawHoneyProductionId = id,
                BeekeeperName = production.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperFirstName
                                + " " +
                                production.RawHoneyProductionApiary.ApiaryBeekeeper.BeekeeperLastName,
                
                ApiaryName = production.RawHoneyProductionApiary.ApiaryTitle,

                WholesalerName = production.RawHoneyProductionWholesaler != null
        ? production.RawHoneyProductionWholesaler.WholesalerTitle
        : "Not Assigned",

                HoneyType = production.RawHoneyProductionHoneyTypeNavigation.HoneyTypeTitle,
                ProductionDate = production.RawHoneyProductionDate.HasValue
    ? production.RawHoneyProductionDate.Value.ToDateTime(TimeOnly.MinValue)
    : (DateTime?)null,
                Amount = production.RawHoneyProductionAmount,
                Timeline = timeline
            };


            var total = timeline.Count;
            var completed = timeline.Count(x => x.StatusName == "Completed");

            ViewBag.ProgressPercent = total == 0 ? 0 : (completed * 100 / total);

            return View(model);
        }


    }
}
