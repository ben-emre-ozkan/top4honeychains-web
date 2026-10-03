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
using TOP4HoneyChains.Web.Models.ViewModels.FinalProduct;
using TOP4HoneyChains.Web.Models.ViewModels.Monitoring;
using TOP4HoneyChains.Web.Models.ViewModels.Timeline;
using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class FinalHoneyProductionsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly CcpIntegrityService _ccp;

        public FinalHoneyProductionsController(TOP4HoneyChainsContext context, CcpIntegrityService ccp)
        {
            _context = context;
            _ccp = ccp;
        }

        // Finalized batches are locked (D-14): show why and go back to the batch list
        private async Task<IActionResult?> RedirectIfLockedAsync(int batchId)
        {
            var reason = await _ccp.GetBatchLockReasonAsync(batchId);
            if (reason == null)
                return null;

            var wholesalerId = await _context.FinalHoneyProductions
                .Where(f => f.FinalHoneyProductionId == batchId)
                .Select(f => f.FinalHoneyProductionWholesalerId)
                .FirstAsync();
            TempData["Error"] = reason;
            return RedirectToAction("ByWholesaler", new { wholesalerId });
        }

        public async Task<IActionResult> ByWholesaler(int wholesalerId)
        {
            var wholesaler = await _context.Wholesalers
                .FirstOrDefaultAsync(w => w.WholesalerId == wholesalerId);

            if (wholesaler == null)
                return NotFound();

            var batches = await _context.FinalHoneyProductions
                .Include(f => f.FinalHoneyProductionDetails)
                .Include(f => f.FinalHoneyCCPAssignments)
                .ThenInclude(a => a.CCP)
                .Where(f => f.FinalHoneyProductionWholesalerId == wholesalerId)
                .OrderByDescending(f => f.FinalHoneyProductionDate)
                .ToListAsync();

            ViewBag.Wholesaler = wholesaler;


            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Productions by Wholesaler";

            return View(batches);
        }

        public async Task<IActionResult> ManageBatch(int id)
        {
            var batch = await _context.FinalHoneyProductions
                .Include(f => f.FinalHoneyProductionDetails)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == id);

            if (batch == null)
                return NotFound();

            if (batch.FinalHoneyProductionIsFinalized)
            {
                ViewBag.IsLocked = true;
            }

            var wholesaler = await _context.Wholesalers
                .FirstOrDefaultAsync(w => w.WholesalerId == batch.FinalHoneyProductionWholesalerId);

            // 🔐 CCP süreci tamamlanmış raw honey’ler
            // At least one CCP, all Completed or Not applicable (D-05, D-15)
            var rawHoneys = await _ccp.EligibleRawLots(_context.RawHoneyProductions
                .Where(r => r.RawHoneyProductionWholesalerId == batch.FinalHoneyProductionWholesalerId))
                .Include(r => r.RawHoneyProductionApiary)
                    .ThenInclude(a => a.ApiaryBeekeeper)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.FinalHoneyProductionDetails)
                .ToListAsync();

            var model = new ManageBatchViewModel
            {
                Batch = batch,
                Wholesaler = wholesaler
            };

            foreach (var raw in rawHoneys)
            {
                // 🔎 Başka batch'lerde kullanılan miktar
                var alreadyUsed = raw.FinalHoneyProductionDetails
                    .Where(d => d.FinalHoneyProductionId != batch.FinalHoneyProductionId)
                    .Sum(d => d.UsedAmount);

                // 🔎 Bu batch'teki mevcut miktar (edit senaryosu için)
                var currentBatchAmount = batch.FinalHoneyProductionDetails
                    .Where(d => d.RawHoneyProductionId == raw.RawHoneyProductionId)
                    .Sum(d => d.UsedAmount);

                var available = raw.RawHoneyProductionAmount - alreadyUsed;

                model.RawHoneyList.Add(new RawHoneyBatchItemViewModel
                {
                    RawHoney = raw,
                    AlreadyUsed = alreadyUsed,
                    AvailableAmount = (decimal)available,
                    CurrentBatchAmount = currentBatchAmount
                });
            }

            // Lots of the same wholesaler that are not eligible yet, and which CCP checks are missing (D-05)
            var eligibleIds = rawHoneys.Select(r => r.RawHoneyProductionId).ToList();
            var ineligibleRawHoneys = await _context.RawHoneyProductions
                .Where(r => r.RawHoneyProductionWholesalerId == batch.FinalHoneyProductionWholesalerId
                            && !eligibleIds.Contains(r.RawHoneyProductionId))
                .Include(r => r.RawHoneyProductionApiary)
                    .ThenInclude(a => a!.ApiaryBeekeeper)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyCCPAssignments)
                    .ThenInclude(a => a.CCP)
                .Include(r => r.RawHoneyCCPAssignments)
                    .ThenInclude(a => a.AssignmentStatus)
                .OrderBy(r => r.RawHoneyProductionId)
                .ToListAsync();

            foreach (var raw in ineligibleRawHoneys)
            {
                model.IneligibleRawHoneyList.Add(new IneligibleRawHoneyItemViewModel
                {
                    RawHoney = raw,
                    PendingChecks = raw.RawHoneyCCPAssignments
                        .Where(a => a.RawHoneyCCPAssignmentStatus != 3) // 3 = Completed
                        .OrderBy(a => a.CCP?.Code)
                        .Select(a => new PendingCcpCheckViewModel
                        {
                            CcpId = a.RawHoneyCCPAssignmentCCP,
                            Code = a.CCP?.Code ?? a.RawHoneyCCPAssignmentCCP.ToString(),
                            Title = a.CCP?.Title,
                            StatusName = a.AssignmentStatus?.AssignmentStatusName
                                         ?? a.RawHoneyCCPAssignmentStatus.ToString()
                        })
                        .ToList()
                });
            }

            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Manage Batch";

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        // An explicit form name stops MVC from falling back to the empty prefix when the batch has
        // no eligible lots (no usedAmounts[...] fields); it then tried to parse "batchId" as a lot id (HTTP 500)
        public async Task<IActionResult> ManageBatchPost(int batchId, [FromForm(Name = "usedAmounts")] Dictionary<int, decimal>? usedAmounts)
        {
            usedAmounts ??= new Dictionary<int, decimal>();

            var batch = await _context.FinalHoneyProductions
                .Include(f => f.FinalHoneyProductionDetails)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == batchId);

            if (batch == null)
                return NotFound();

            if (batch.FinalHoneyProductionIsFinalized)
            {
                TempData["Error"] = "This batch is locked and cannot be modified.";
                return RedirectToAction("ManageBatch", new { id = batchId });
            }

            foreach (var item in usedAmounts)
            {
                var rawId = item.Key;
                var requestedAmount = item.Value;

                if (requestedAmount <= 0)
                {
                    var existing = batch.FinalHoneyProductionDetails
                        .FirstOrDefault(d => d.RawHoneyProductionId == rawId);

                    if (existing != null)
                        _context.FinalHoneyProductionDetails.Remove(existing);

                    continue;
                }

                var raw = await _context.RawHoneyProductions
                    .Include(r => r.FinalHoneyProductionDetails)
                    .FirstOrDefaultAsync(r =>
                        r.RawHoneyProductionId == rawId &&
                        r.RawHoneyProductionWholesalerId == batch.FinalHoneyProductionWholesalerId);

                if (raw == null)
                    continue;

                // Same rule as the list: a lot without any CCP is not eligible either
                if (!await _ccp.EligibleRawLots(_context.RawHoneyProductions.Where(r => r.RawHoneyProductionId == rawId)).AnyAsync())
                    continue;

                var totalUsed = raw.FinalHoneyProductionDetails
                    .Where(d => d.FinalHoneyProductionId != batchId)
                    .Sum(d => d.UsedAmount);

                var available = raw.RawHoneyProductionAmount - totalUsed;

                if (requestedAmount > available)
                    continue;

                var existingDetail = batch.FinalHoneyProductionDetails
                    .FirstOrDefault(d => d.RawHoneyProductionId == rawId);

                if (existingDetail != null)
                {
                    existingDetail.UsedAmount = requestedAmount;
                }
                else
                {
                    _context.FinalHoneyProductionDetails.Add(new FinalHoneyProductionDetail
                    {
                        FinalHoneyProductionId = batchId,
                        RawHoneyProductionId = rawId,
                        UsedAmount = requestedAmount
                    });
                }
            }

            await _context.SaveChangesAsync();

            batch.FinalHoneyProductionTotalAmount =
                await _context.FinalHoneyProductionDetails
                    .Where(d => d.FinalHoneyProductionId == batchId)
                    .SumAsync(d => d.UsedAmount);

            await _context.SaveChangesAsync();


            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Manage Batch";

            return RedirectToAction("ManageBatch", new { id = batchId });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeBatch(int id)
        {
            var batch = await _context.FinalHoneyProductions
                .Include(f => f.FinalHoneyProductionDetails)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == id);

            if (batch == null)
                return NotFound();

            // 🔒 Zaten finalize edilmiş mi?
            if (batch.FinalHoneyProductionIsFinalized)
                return BadRequest("Batch already finalized.");

            // 🔴 TOTAL KONTROLÜ (EN KRİTİK)
            var total = await _context.FinalHoneyProductionDetails
                .Where(d => d.FinalHoneyProductionId == id)
                .SumAsync(d => (decimal?)d.UsedAmount) ?? 0;

            if (total <= 0)
            {
                TempData["Error"] = "Batch total must be greater than zero before finalizing.";
                return RedirectToAction("ManageBatch", new { id });
            }

            // 🔒 Finalize
            batch.FinalHoneyProductionIsFinalized = true;
            batch.FinalHoneyProductionFinalizedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Batch finalized successfully.";


            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Final Batch";

            return RedirectToAction("ManageBatch", new { id });
        }

        // There is no "unfinalize": a finalized batch is permanent (D-14)

        public async Task<IActionResult> ManageCCP(int id)
        {
            var production = await _context.FinalHoneyProductions
                .Include(f => f.FinalHoneyProductionWholesaler)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == id);

            if (production == null)
                return NotFound();

            var assignedCCPs = await _context.FinalHoneyCCPAssignments
                .Where(x => x.FinalHoneyProductionId == id)
                .Select(x => x.FinalHoneyCCPAssignmentCCP)
                .ToListAsync();

            var vm = new ManageFinalHoneyCCPViewModel
            {
                FinalHoneyProductionId = id,
                Production = production,
                // Only CCPs that apply to this kind of record (D-17), plus any assigned earlier
                AllCCPs = await _context.CriticalControlPoints
                    .Where(c => c.AppliesTo == CcpAppliesTo.Batch || assignedCCPs.Contains(c.Id))
                    .OrderBy(c => c.Id)
                    .ToListAsync(),
                SelectedCCPIds = assignedCCPs,
                LockReason = await _ccp.GetBatchLockReasonAsync(id)
            };


            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Manage CCP";

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageCCP(ManageFinalHoneyCCPViewModel vm)
        {
            var production = await _context.FinalHoneyProductions
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == vm.FinalHoneyProductionId);

            if (production == null)
                return NotFound();

            if (await RedirectIfLockedAsync(vm.FinalHoneyProductionId) is IActionResult locked)
                return locked;

            vm.SelectedCCPIds ??= new List<int>();

            // Only CCPs that apply to batches can be added (D-17)
            var applicableIds = await _context.CriticalControlPoints
                .Where(c => c.AppliesTo == CcpAppliesTo.Batch)
                .Select(c => c.Id)
                .ToListAsync();
            var assignedIds = await _context.FinalHoneyCCPAssignments
                .Where(x => x.FinalHoneyProductionId == vm.FinalHoneyProductionId)
                .Select(x => x.FinalHoneyCCPAssignmentCCP)
                .ToListAsync();
            if (vm.SelectedCCPIds.Any(id => !assignedIds.Contains(id) && !applicableIds.Contains(id)))
            {
                TempData["Error"] = "Only CCPs that apply to final batches can be assigned. Nothing was changed.";
                return RedirectToAction(nameof(ManageCCP), new { id = vm.FinalHoneyProductionId });
            }

            var existingAssignments = await _context.FinalHoneyCCPAssignments
                .Where(x => x.FinalHoneyProductionId == vm.FinalHoneyProductionId)
                .Include(x => x.CCP)
                .ToListAsync();

            var toRemove = existingAssignments
                .Where(x => !vm.SelectedCCPIds.Contains(x.FinalHoneyCCPAssignmentCCP))
                .ToList();

            // Never lose recorded values (D-14): a CCP with values or a "Not applicable" record stays
            var blocked = new List<string>();
            foreach (var assignment in toRemove)
            {
                var hasValues = await _context.CriticalControlPointMonitoringLogs.AnyAsync(l =>
                    l.FinalHoneyProductionId == vm.FinalHoneyProductionId && l.CCPId == assignment.FinalHoneyCCPAssignmentCCP);
                if (hasValues || assignment.FinalHoneyCCPAssignmentStatus == AssignmentStatusIds.NotApplicable)
                    blocked.Add(assignment.CCP?.Code ?? assignment.FinalHoneyCCPAssignmentCCP.ToString());
            }
            if (blocked.Count > 0)
            {
                TempData["Error"] = $"{string.Join(", ", blocked)} cannot be removed because values or a \"Not applicable\" record exist for this batch. Nothing was changed.";
                return RedirectToAction(nameof(ManageCCP), new { id = vm.FinalHoneyProductionId });
            }

            _context.FinalHoneyCCPAssignments.RemoveRange(toRemove);

            // Yeni eklenenleri ekle
            var existingIds = existingAssignments
                .Select(x => x.FinalHoneyCCPAssignmentCCP)
                .ToList();

            var toAdd = vm.SelectedCCPIds
                .Where(id => !existingIds.Contains(id))
                .ToList();

            foreach (var ccpId in toAdd)
            {
                _context.FinalHoneyCCPAssignments.Add(new FinalHoneyCCPAssignment
                {
                    FinalHoneyProductionId = vm.FinalHoneyProductionId,
                    FinalHoneyCCPAssignmentCCP = ccpId,
                    FinalHoneyCCPAssignmentStatus = 1, // Draft
                    FinalHoneyCCPAssignmentStartDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            //ByWholesaler?wholesalerId=1

            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Manage CCP";

            return RedirectToAction("ByWholesaler", new { wholesalerId = production.FinalHoneyProductionWholesalerId });
        }



        public async Task<IActionResult> CCPOverview(int id)
        {
            var batch = await _context.FinalHoneyProductions
                .Include(f => f.FinalHoneyProductionWholesaler)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == id);

            if (batch == null)
                return NotFound();

            var timeline = await _context.FinalHoneyCCPAssignments
                .Include(x => x.CCP)
                .Include(x => x.AssignmentStatus)
                .Where(x => x.FinalHoneyProductionId == id)
                .OrderBy(x => x.CCP.Id)
                .Select(x => new CCPTimelineItem
                {
                    CCPCode = x.CCP.Code,
                    CCPTitle = x.CCP.Title,
                    StatusName = x.AssignmentStatus.AssignmentStatusName,
                    StartDate = x.FinalHoneyCCPAssignmentStartDate,
                    EndDate = x.FinalHoneyCCPAssignmentEndDate
                })
                .ToListAsync();

            var model = new CCPOverviewViewModel
            {
                RawHoneyProductionId = id, // reuse ediyoruz
                BeekeeperName = "Final Batch",
                ApiaryName = "Blended Production",
                WholesalerName = batch.FinalHoneyProductionWholesaler?.WholesalerTitle ?? "Not Assigned",
                HoneyType = "Mixed",
                ProductionDate = batch.FinalHoneyProductionDate,
                Amount = batch.FinalHoneyProductionTotalAmount,
                Timeline = timeline
            };

            var total = timeline.Count;
            var completed = timeline.Count(x => x.StatusName == "Completed" || x.StatusName == "Not applicable");

            ViewBag.ProgressPercent = total == 0 ? 0 : (completed * 100 / total);
            ViewBag.ReturnUrl = Url.Action("Index", "FinalHoneyProductions");


            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "CCP Overview";

            return View("CCPOverview", model);
        }


        public async Task<IActionResult> Create(int wholesalerId)
        {
            var wholesaler = await _context.Wholesalers
                .FirstOrDefaultAsync(w => w.WholesalerId == wholesalerId);

            if (wholesaler == null)
                return NotFound();

            var model = new FinalHoneyProduction
            {
                FinalHoneyProductionWholesalerId = wholesalerId,
                FinalHoneyProductionDate = DateTime.Today,
                FinalHoneyProductionTotalAmount = 0
            };

            ViewBag.Wholesaler = wholesaler;



            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Create";

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FinalHoneyProduction model)
        {
            // ModelState false olsa bile wholesaler'ı tekrar yükle
            var wholesaler = await _context.Wholesalers
                .FirstOrDefaultAsync(w => w.WholesalerId == model.FinalHoneyProductionWholesalerId);

            ViewBag.Wholesaler = wholesaler;

            if (wholesaler == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Controller = "Final Honey Productions";
                ViewBag.Action = "Create";
                return View(model);
            }

            model.FinalHoneyProductionQR = Guid.NewGuid();
            model.FinalHoneyProductionTotalAmount = 0;

            _context.Add(model);
            await _context.SaveChangesAsync();


            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Create";

            return RedirectToAction("ManageBatch",
                new { id = model.FinalHoneyProductionId });
        }






        // GET: FinalHoneyProductions
        public async Task<IActionResult> Index()
        {

            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Index";

            return View(await _context.FinalHoneyProductions.ToListAsync());
        }

        // GET: FinalHoneyProductions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finalHoneyProduction = await _context.FinalHoneyProductions
                .FirstOrDefaultAsync(m => m.FinalHoneyProductionId == id);
            if (finalHoneyProduction == null)
            {
                return NotFound();
            }


            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Details";

            return View(finalHoneyProduction);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finalHoneyProduction = await _context.FinalHoneyProductions.FindAsync(id);
            if (finalHoneyProduction == null)
            {
                return NotFound();
            }

            if (await RedirectIfLockedAsync(id.Value) is IActionResult locked)
                return locked;

            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Edit";

            return View(finalHoneyProduction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FinalHoneyProduction model)
        {
            if (id != model.FinalHoneyProductionId)
                return NotFound();

            var batch = await _context.FinalHoneyProductions
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == id);

            if (batch == null)
                return NotFound();

            if (await RedirectIfLockedAsync(id) is IActionResult locked)
                return locked;

            if (!ModelState.IsValid)
                return View(model);

            // ✅ Sadece metadata güncelle
            batch.FinalHoneyProductionTitle = model.FinalHoneyProductionTitle;
            batch.FinalHoneyProductionDate = model.FinalHoneyProductionDate;

            await _context.SaveChangesAsync();


            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Edit";

            return RedirectToAction("ByWholesaler",
                new { wholesalerId = batch.FinalHoneyProductionWholesalerId });
        }

        // GET: FinalHoneyProductions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finalHoneyProduction = await _context.FinalHoneyProductions
                .FirstOrDefaultAsync(m => m.FinalHoneyProductionId == id);
            if (finalHoneyProduction == null)
            {
                return NotFound();
            }

            if (await RedirectIfLockedAsync(id.Value) is IActionResult locked)
                return locked;

            ViewBag.Controller = "Final Honey Productions";
            ViewBag.Action = "Delete";

            return View(finalHoneyProduction);
        }

        // POST: FinalHoneyProductions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await RedirectIfLockedAsync(id) is IActionResult locked)
                return locked;

            var finalHoneyProduction = await _context.FinalHoneyProductions.FindAsync(id);
            if (finalHoneyProduction != null)
            {
                _context.FinalHoneyProductions.Remove(finalHoneyProduction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool FinalHoneyProductionExists(int id)
        {
            return _context.FinalHoneyProductions.Any(e => e.FinalHoneyProductionId == id);
        }
    }
}
