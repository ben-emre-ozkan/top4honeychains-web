using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.ViewModels.Monitoring;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-88: control sheets. One page per lot (CCP 2–3) and per batch (CCP 4–10) with all values at once: the same checks as
    // the page of a single CCP (data types, limits, "Not applicable" rules, lock, enter rights), saved together or not at all.
    // The approver sees every value and approves the sheet in one action; four-eyes applies to each record (never one's own
    // entries) and every approved record gets its own audit entry.
    public partial class CCPMonitoringController
    {
        public const string SheetFlag = "ControlSheet";

        private async Task<List<int>> SheetCcpIdsAsync(Owner owner, int ownerId) => owner == Owner.RawLot
            ? await _context.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == ownerId && !ControlSheets.OwnPageLotCcps.Contains(a.CCP!.Code))
                .OrderBy(a => a.RawHoneyCCPAssignmentCCP).Select(a => a.RawHoneyCCPAssignmentCCP).ToListAsync()
            : await _context.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == ownerId)
                .OrderBy(a => a.FinalHoneyCCPAssignmentCCP).Select(a => a.FinalHoneyCCPAssignmentCCP).ToListAsync();

        private async Task<bool> RecordExistsAsync(Owner owner, int ownerId) => owner == Owner.RawLot
            ? await _context.RawHoneyProductions.AnyAsync(r => r.RawHoneyProductionId == ownerId)
            : await _context.FinalHoneyProductions.AnyAsync(f => f.FinalHoneyProductionId == ownerId);

        // null: no such record; an empty sheet: the user may read none of its CCPs
        private async Task<ControlSheetViewModel?> BuildSheetAsync(Owner owner, int ownerId)
        {
            if (!await RecordExistsAsync(owner, ownerId))
                return null;
            var sheet = new ControlSheetViewModel { Kind = KindOf(owner), Id = ownerId, LockReason = await LockReasonAsync(owner, ownerId) };
            foreach (var ccpId in await SheetCcpIdsAsync(owner, ownerId))
            {
                if (!(await AuthorizeAsync(owner, ownerId, ccpId, CcpOperations.Read)).Succeeded)
                    continue;
                CcpMonitoringViewModelBase? ccp = owner == Owner.RawLot
                    ? await BuildRawModelAsync(ownerId, ccpId)
                    : await BuildBatchModelAsync(ownerId, ccpId);
                if (ccp == null)
                    continue;
                sheet.Ccps.Add(ccp);
                if (ccp.ApprovedAt != null || !(ccp.StatusId is AssignmentStatusIds.Submitted or AssignmentStatusIds.Completed or AssignmentStatusIds.NotApplicable))
                    continue;
                if (!_current.Enabled || !await _resp.RequiresApprovalAsync(ccpId))
                    continue;
                if (ccp.CanApprove && ccp.LockReason == null)
                    sheet.Approvable.Add(ccpId);
                else if (ccp.ApproveDeniedReason != null)
                    sheet.NotApprovable.Add($"{ccp.CCPCode}: {ccp.ApproveDeniedReason}");
            }
            sheet.Lot = sheet.Ccps.FirstOrDefault() as CCPMonitoringViewModelForRawHoney;
            sheet.Batch = sheet.Ccps.FirstOrDefault() as CCPMonitoringViewModelForFinalBatch;
            return sheet;
        }

        public async Task<IActionResult> Sheet(string kind, int id)
        {
            var owner = ParseOwner(kind);
            if (owner == null)
                return NotFound();
            var sheet = await BuildSheetAsync(owner.Value, id);
            if (sheet == null)
                return NotFound();
            if (sheet.Ccps.Count == 0)
                return Forbidden();
            ViewBag.Controller = "CCPMonitoring";
            ViewBag.Action = "Sheet";
            return View(sheet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Sheet")]
        public async Task<IActionResult> SaveSheet(ControlSheetPost model)
        {
            var owner = ParseOwner(model.Kind);
            if (owner == null)
                return NotFound();
            var sheet = await BuildSheetAsync(owner.Value, model.Id);
            if (sheet == null)
                return NotFound();
            if (sheet.Ccps.Count == 0)
                return Forbidden();

            // only the sheet's CCPs the user may enter; read-only ones are not posted (disabled fields)
            var posted = (model.Ccps ?? new()).Where(p => p.CCPId > 0).GroupBy(p => p.CCPId).Select(g => g.First()).ToList();
            var errors = new List<string>();
            var work = new List<(ControlSheetCcpPost Post, AssignmentRef Assignment, Dictionary<int, CriticalControlPointParameter> Parameters, List<CCPParameterInput> Inputs)>();
            foreach (var p in posted)
            {
                var view = sheet.Ccps.FirstOrDefault(c => c.CCPId == p.CCPId);
                if (view == null)
                    continue;
                var (assignment, refused) = await CheckEntryAsync(owner.Value, model.Id, p.CCPId);
                if (refused != null)
                {
                    errors.Add($"{view.CCPCode}: {refused}");
                    continue;
                }
                var parameters = (await LoadParametersAsync(p.CCPId)).ToDictionary(x => x.ParameterId);
                var inputs = PostedInputs(parameters, p.Groups);
                var problems = await ValidateValuesAsync(parameters, inputs);
                errors.AddRange(problems.Select(e => $"{view.CCPCode}: {e}"));
                if (p.NotApplicable)
                {
                    // the rules of SetNotApplicable: no values recorded or entered, and a reason
                    var reason = p.NotApplicableReason?.Trim();
                    if (view.HasValues || inputs.Any(i => i.MeasuredNumericValue != null || i.MeasuredTextValue != null || i.File != null))
                        errors.Add($"{view.CCPCode}: This CCP already has recorded values, so it cannot be marked \"Not applicable\". Clear the values first if the CCP really does not apply.");
                    else if (string.IsNullOrEmpty(reason))
                        errors.Add($"{view.CCPCode}: A reason is required to mark a CCP as \"Not applicable\".");
                    else if (reason.Length > 500)
                        errors.Add($"{view.CCPCode}: The reason can be at most 500 characters.");
                }
                work.Add((p, assignment!, parameters, inputs));
            }
            if (posted.Count == 0)
                errors.Add("Nothing on this sheet can be recorded by you.");

            if (errors.Count > 0)
            {
                // the page again with the posted values and every problem; nothing was saved
                foreach (var p in posted)
                {
                    var view = sheet.Ccps.FirstOrDefault(c => c.CCPId == p.CCPId);
                    if (view == null)
                        continue;
                    var byId = (p.Groups ?? new()).SelectMany(g => g.Parameters ?? new()).GroupBy(x => x.ParameterId).ToDictionary(g => g.Key, g => g.First());
                    foreach (var input in view.Groups.SelectMany(g => g.Parameters))
                        if (byId.TryGetValue(input.ParameterId, out var x))
                        {
                            input.MeasuredNumericValue = x.MeasuredNumericValue;
                            input.MeasuredTextValue = x.MeasuredTextValue;
                            input.Notes = x.Notes;
                        }
                    sheet.NotApplicable[p.CCPId] = (p.NotApplicable, p.NotApplicableReason);
                }
                sheet.Errors = errors;
                ModelState.Clear();
                ViewBag.Controller = "CCPMonitoring";
                ViewBag.Action = "Sheet";
                return View("Sheet", sheet);
            }

            var files = new FileChanges(Path.Combine(_env.ContentRootPath, FileFolder));
            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var (p, assignment, parameters, inputs) in work)
                    {
                        if (p.NotApplicable)
                        {
                            assignment.Status = AssignmentStatusIds.NotApplicable;
                            assignment.NotApplicableReason = p.NotApplicableReason!.Trim();
                            assignment.NotApplicableBy = User.Identity?.Name ?? "unknown";
                            assignment.NotApplicableAt = DateTime.Now;
                            assignment.EndDate = DateTime.Now;
                            assignment.ClearApproval();
                            await _context.SaveChangesAsync();
                        }
                        else
                            await StoreValuesAsync(owner.Value, model.Id, p.CCPId, assignment, parameters, inputs, files);
                    }
                    await transaction.CommitAsync();
                }
                catch
                {
                    files.RollBack();
                    throw;
                }
            }
            files.Commit();
            TempData["Success"] = "Control sheet saved.";
            return RedirectToAction(nameof(Sheet), new { kind = model.Kind, id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSheet(string kind, int id, bool confirmOutOfLimit = false)
        {
            var owner = ParseOwner(kind);
            if (!_current.Enabled || owner == null || !await RecordExistsAsync(owner.Value, id))
                return NotFound();
            var readable = new List<int>();
            foreach (var ccpId in await SheetCcpIdsAsync(owner.Value, id))
                if ((await AuthorizeAsync(owner.Value, id, ccpId, CcpOperations.Read)).Succeeded)
                    readable.Add(ccpId);
            if (readable.Count == 0)
                return Forbidden();
            if (await LockReasonAsync(owner.Value, id) is string lockReason)
            {
                TempData["Error"] = lockReason;
                return RedirectToAction(nameof(Sheet), new { kind, id });
            }

            var codes = await _context.CriticalControlPoints.Where(c => readable.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Code);
            var approve = new List<(AssignmentRef Assignment, int CcpId, bool Self)>();
            var refused = new List<string>();
            foreach (var ccpId in readable)
            {
                var assignment = await FindAssignmentAsync(owner.Value, id, ccpId);
                if (assignment == null || assignment.ApprovedAt != null
                    || !(assignment.Status is AssignmentStatusIds.Submitted or AssignmentStatusIds.Completed or AssignmentStatusIds.NotApplicable)
                    || !await _resp.RequiresApprovalAsync(ccpId))
                    continue;
                var result = await AuthorizeAsync(owner.Value, id, ccpId, CcpOperations.Approve);
                // the self-approval marker of this check belongs to this record only
                var self = HttpContext.Items.Remove(CcpAuthorizationHandler.SelfApprovalKey);
                if (!result.Succeeded)
                {
                    refused.Add($"{codes[ccpId]}: {ReasonOf(result) ?? $"Only a {OrgRoles.QualityApprover} of the organization can approve."}");
                    continue;
                }
                approve.Add((assignment, ccpId, self));
            }

            if (approve.Count == 0)
            {
                TempData["Error"] = refused.Count > 0 ? string.Join(" ", refused) : "Nothing on this sheet waits for your approval.";
                return RedirectToAction(nameof(Sheet), new { kind, id });
            }
            // D-64: values outside their limits are approved only when confirmed explicitly
            var approveIds = approve.Select(a => a.CcpId).ToList();
            if (!confirmOutOfLimit && await _context.CriticalControlPointMonitoringLogs.AnyAsync(l => l.CCPId != null && approveIds.Contains(l.CCPId.Value) && l.IsInRange == false
                    && (owner == Owner.RawLot ? l.RawHoneyProductionId == id : l.FinalHoneyProductionId == id)))
            {
                TempData["Error"] = "The sheet has values outside their limits; confirm them to approve.";
                return RedirectToAction(nameof(Sheet), new { kind, id });
            }

            var active = await _current.GetAsync();
            var table = owner == Owner.RawLot ? "RawHoneyCCPAssignments" : "FinalHoneyCCPAssignments";
            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                foreach (var (assignment, ccpId, self) in approve)
                {
                    assignment.ApprovedByUserId = _current.UserId;
                    assignment.ApprovedByOrgId = active?.OrgId;
                    assignment.ApprovedAt = DateTime.Now;
                    assignment.SelfApproved = self;
                    if (assignment.Status == AssignmentStatusIds.Submitted)
                        assignment.Status = AssignmentStatusIds.Completed;
                    await _context.SaveChangesAsync();
                    // one audit entry per approved record
                    await _audit.WriteAsync("Approve", table, $"{id}/{ccpId}", null, new { Status = assignment.Status },
                        self ? $"{SheetFlag},SelfApproved" : SheetFlag);
                }
                await transaction.CommitAsync();
            }

            var approvedCodes = string.Join(", ", approve.Select(a => codes[a.CcpId]));
            TempData["Success"] = approve.Any(a => a.Self)
                ? $"Approved: {approvedCodes} (four-eyes exemption flagged in the audit log)."
                : $"Approved: {approvedCodes}.";
            if (refused.Count > 0)
                TempData["Error"] = string.Join(" ", refused);
            TempData["JustApproved"] = true;
            return RedirectToAction(nameof(Sheet), new { kind, id });
        }
    }
}
