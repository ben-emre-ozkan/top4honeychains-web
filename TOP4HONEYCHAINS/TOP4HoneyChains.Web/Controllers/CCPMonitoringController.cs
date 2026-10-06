using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.ViewModels.Monitoring;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using TOP4HoneyChains.Web.Models.Identity;

namespace TOP4HoneyChains.Web.Controllers
{
    // CCP values for raw honey lots and final batches. Rules: values only for an assigned CCP that
    // applies to the record (D-17), never for a locked record (D-14) or a "Not applicable" CCP (D-15);
    // values are checked against the parameter's data type and limits (D-18); file values are stored
    // outside wwwroot (D-19).
    // Administrators (v1.0); with Features:OrgRoleModel also organization members, checked per record
    // by CcpAuthorizationHandler (read / enter / approve)
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class CCPMonitoringController : Controller
    {
        private const string FileFolder = "App_Data/ccp-files";

        private readonly TOP4HoneyChainsContext _context;
        private readonly CcpIntegrityService _ccp;
        private readonly IWebHostEnvironment _env;
        private readonly IAuthorizationService _authz;
        private readonly ICurrentOrg _current;
        private readonly AuditWriter _audit;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CcpResponsibilityService _resp;

        public CCPMonitoringController(TOP4HoneyChainsContext context, CcpIntegrityService ccp, IWebHostEnvironment env,
            IAuthorizationService authz, ICurrentOrg current, AuditWriter audit, UserManager<ApplicationUser> userManager,
            CcpResponsibilityService resp)
        {
            _resp = resp;
            _context = context;
            _ccp = ccp;
            _env = env;
            _authz = authz;
            _current = current;
            _audit = audit;
            _userManager = userManager;
        }

        private static string KindOf(Owner owner) => owner == Owner.RawLot ? "raw" : "batch";

        private Task<AuthorizationResult> AuthorizeAsync(Owner owner, int ownerId, int ccpId, OperationAuthorizationRequirement op) =>
            _authz.AuthorizeAsync(User, new CcpResource(KindOf(owner), ownerId, ccpId), op);

        private static string? ReasonOf(AuthorizationResult r) =>
            r.Failure?.FailureReasons.Select(x => x.Message).FirstOrDefault();

        // Members have no access to the admin lists: send them back to their workspace
        private IActionResult BackToList(string controller, string action, object routeValues) =>
            User.IsInRole(CurrentOrg.AdminRole) ? RedirectToAction(action, controller, routeValues) : RedirectToAction("Index", "OrgWorkspace");

        // ------------------------------------------------------------------ raw honey lot

        public async Task<IActionResult> RawHoneyProductionManageCCP(int rawHoneyProductionId, int ccpId)
        {
            // Another organization's record answers 404, so its existence is not revealed
            if (!(await AuthorizeAsync(Owner.RawLot, rawHoneyProductionId, ccpId, CcpOperations.Read)).Succeeded)
                return NotFound();
            var model = await BuildRawModelAsync(rawHoneyProductionId, ccpId);
            if (model == null)
                return NotFound();
            if (await FindAssignmentAsync(Owner.RawLot, rawHoneyProductionId, ccpId) == null)
            {
                TempData["Error"] = "This CCP is not assigned to the lot.";
                return BackToList("RawHoneyProductions", "RawHoneyProductionByApiary", new { id = model.ApiaryId });
            }

            ViewBag.Controller = "Raw Honey Production Manage CCP";
            ViewBag.Action = "Manage";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RawHoneyProductionManageCCP(CCPMonitoringViewModelForRawHoney model)
        {
            if (!(await AuthorizeAsync(Owner.RawLot, model.RawHoneyProductionId, model.CCPId, CcpOperations.Read)).Succeeded)
                return NotFound();
            var display = await BuildRawModelAsync(model.RawHoneyProductionId, model.CCPId);
            if (display == null)
                return NotFound();

            var errors = await SaveValuesAsync(Owner.RawLot, model.RawHoneyProductionId, model.CCPId, model.Groups);
            if (errors == null)
            {
                TempData["Success"] = $"{display.CCPCode} values saved.";
                return BackToList("RawHoneyProductions", "RawHoneyProductionByApiary", new { id = display.ApiaryId });
            }

            return ShowErrors(display, model.Groups, errors);
        }

        // ------------------------------------------------------------------ final batch

        public async Task<IActionResult> FinalHoneyProductionManageCCP(int finalHoneyProductionId, int ccpId)
        {
            if (!(await AuthorizeAsync(Owner.Batch, finalHoneyProductionId, ccpId, CcpOperations.Read)).Succeeded)
                return NotFound();
            var model = await BuildBatchModelAsync(finalHoneyProductionId, ccpId);
            if (model == null)
                return NotFound();
            if (await FindAssignmentAsync(Owner.Batch, finalHoneyProductionId, ccpId) == null)
            {
                TempData["Error"] = "This CCP is not assigned to the batch.";
                return BackToList("FinalHoneyProductions", "ByWholesaler", new { wholesalerId = model.WholesalerId });
            }

            ViewBag.Controller = "Final Honey Production Manage CCP";
            ViewBag.Action = "Manage";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalHoneyProductionManageCCP(CCPMonitoringViewModelForFinalBatch model)
        {
            if (!(await AuthorizeAsync(Owner.Batch, model.FinalHoneyProductionId, model.CCPId, CcpOperations.Read)).Succeeded)
                return NotFound();
            var display = await BuildBatchModelAsync(model.FinalHoneyProductionId, model.CCPId);
            if (display == null)
                return NotFound();

            var errors = await SaveValuesAsync(Owner.Batch, model.FinalHoneyProductionId, model.CCPId, model.Groups);
            if (errors == null)
            {
                TempData["Success"] = $"{display.CCPCode} values saved.";
                return BackToList("FinalHoneyProductions", "ByWholesaler", new { wholesalerId = display.WholesalerId });
            }

            return ShowErrors(display, model.Groups, errors);
        }

        // ------------------------------------------------------------------ Not applicable (D-15)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetNotApplicable(string ownerKind, int ownerId, int ccpId, string? reason)
        {
            var owner = ParseOwner(ownerKind);
            if (owner == null)
                return NotFound();

            var assignment = await FindAssignmentAsync(owner.Value, ownerId, ccpId);
            if (assignment == null || !(await AuthorizeAsync(owner.Value, ownerId, ccpId, CcpOperations.Read)).Succeeded)
                return NotFound();

            reason = reason?.Trim();
            string? error = await LockReasonAsync(owner.Value, ownerId);
            var enter = await AuthorizeAsync(owner.Value, ownerId, ccpId, CcpOperations.Enter);
            if (error == null && !enter.Succeeded)
                error = ReasonOf(enter) ?? "Your role cannot record this CCP.";
            if (error == null && await HasValuesAsync(owner.Value, ownerId, ccpId))
                error = "This CCP already has recorded values, so it cannot be marked \"Not applicable\". Clear the values first if the CCP really does not apply.";
            if (error == null && string.IsNullOrEmpty(reason))
                error = "A reason is required to mark a CCP as \"Not applicable\".";
            if (error == null && reason!.Length > 500)
                error = "The reason can be at most 500 characters.";

            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                assignment.Status = AssignmentStatusIds.NotApplicable;
                assignment.NotApplicableReason = reason;
                assignment.NotApplicableBy = User.Identity?.Name ?? "unknown";
                assignment.NotApplicableAt = DateTime.Now;
                assignment.EndDate = DateTime.Now;
                assignment.ClearApproval();
                await _context.SaveChangesAsync();
                TempData["Success"] = "The CCP is marked \"Not applicable\".";
            }
            return RedirectToMonitoring(owner.Value, ownerId, ccpId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearNotApplicable(string ownerKind, int ownerId, int ccpId)
        {
            var owner = ParseOwner(ownerKind);
            if (owner == null)
                return NotFound();

            var assignment = await FindAssignmentAsync(owner.Value, ownerId, ccpId);
            if (assignment == null || !(await AuthorizeAsync(owner.Value, ownerId, ccpId, CcpOperations.Read)).Succeeded)
                return NotFound();

            var enter = await AuthorizeAsync(owner.Value, ownerId, ccpId, CcpOperations.Enter);
            if (await LockReasonAsync(owner.Value, ownerId) is string lockReason)
            {
                TempData["Error"] = lockReason;
            }
            else if (!enter.Succeeded)
            {
                TempData["Error"] = ReasonOf(enter) ?? "Your role cannot record this CCP.";
            }
            else if (assignment.Status == AssignmentStatusIds.NotApplicable)
            {
                assignment.Status = AssignmentStatusIds.Assigned;
                assignment.NotApplicableReason = null;
                assignment.NotApplicableBy = null;
                assignment.NotApplicableAt = null;
                assignment.EndDate = null;
                assignment.ClearApproval();
                await _context.SaveChangesAsync();
                TempData["Success"] = "\"Not applicable\" was removed; values can be recorded again.";
            }
            return RedirectToMonitoring(owner.Value, ownerId, ccpId);
        }

        // ------------------------------------------------------------------ four-eyes approval (D-27)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string ownerKind, int ownerId, int ccpId, bool confirmOutOfLimit = false)
        {
            var owner = ParseOwner(ownerKind);
            if (!_current.Enabled || owner == null)
                return NotFound();
            var assignment = await FindAssignmentAsync(owner.Value, ownerId, ccpId);
            if (assignment == null || !(await AuthorizeAsync(owner.Value, ownerId, ccpId, CcpOperations.Read)).Succeeded)
                return NotFound();

            var result = await AuthorizeAsync(owner.Value, ownerId, ccpId, CcpOperations.Approve);
            string? error = await LockReasonAsync(owner.Value, ownerId);
            if (error == null && !result.Succeeded)
                error = ReasonOf(result) ?? "Only a QualityApprover of the organization can approve.";
            if (error == null && !(assignment.Status is AssignmentStatusIds.Submitted or AssignmentStatusIds.Completed or AssignmentStatusIds.NotApplicable))
                error = "Only a submitted (or \"Not applicable\") CCP record can be approved.";
            if (error == null && assignment.ApprovedAt != null)
                error = "This CCP record is already approved.";
            // D-64: values outside their limits are approved only when confirmed explicitly
            if (error == null && !confirmOutOfLimit)
            {
                var rawId = owner.Value == Owner.RawLot ? ownerId : (int?)null;
                var batchId = owner.Value == Owner.RawLot ? (int?)null : ownerId;
                if (await _context.CriticalControlPointMonitoringLogs.AnyAsync(l => l.CCPId == ccpId && l.IsInRange == false
                        && (rawId != null ? l.RawHoneyProductionId == rawId : l.FinalHoneyProductionId == batchId)))
                    error = "The record has values outside their limits; confirm them to approve.";
            }

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToMonitoring(owner.Value, ownerId, ccpId);
            }

            var self = HttpContext.Items.ContainsKey(CcpAuthorizationHandler.SelfApprovalKey);
            var active = await _current.GetAsync();
            assignment.ApprovedByUserId = _current.UserId;
            assignment.ApprovedByOrgId = active?.OrgId;
            assignment.ApprovedAt = DateTime.Now;
            assignment.SelfApproved = self;
            if (assignment.Status == AssignmentStatusIds.Submitted)
                assignment.Status = AssignmentStatusIds.Completed;
            await _context.SaveChangesAsync();
            await _audit.WriteAsync("Approve", owner == Owner.RawLot ? "RawHoneyCCPAssignments" : "FinalHoneyCCPAssignments",
                $"{ownerId}/{ccpId}", null, new { Status = assignment.Status }, self ? "SelfApproved" : null);
            TempData["Success"] = self ? "Approved under the four-eyes exemption (flagged in the audit log)." : "CCP record approved.";
            return RedirectToMonitoring(owner.Value, ownerId, ccpId);
        }

        // ------------------------------------------------------------------ corrections (D-35)

        // A platform administrator requests the change of one recorded value; it is applied only when a
        // second administrator approves it. Works on locked (finalized) records as well: corrections never unlock.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> RequestCorrection(string ownerKind, int ownerId, int ccpId, int parameterId,
            decimal? newNumericValue, string? newTextValue, string? reason)
        {
            var owner = ParseOwner(ownerKind);
            if (!_current.Enabled || owner == null || await FindAssignmentAsync(owner.Value, ownerId, ccpId) == null)
                return NotFound();
            var p = await _context.CriticalControlPointParameters.FirstOrDefaultAsync(x => x.ParameterId == parameterId && x.CCPId == ccpId);
            if (p == null)
                return NotFound();

            newTextValue = string.IsNullOrWhiteSpace(newTextValue) ? null : newTextValue.Trim();
            reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            string? error = null;
            if (reason == null)
                error = "A reason is required for a correction.";
            else if (reason.Length > 500)
                error = "The reason can be at most 500 characters.";
            else if (p.DataType == CcpDataType.File)
                error = "File values cannot be corrected here.";
            else if (p.DataType == CcpDataType.Number && (newNumericValue == null || newTextValue != null))
                error = $"{p.ParameterName}: enter a number.";
            else if (p.DataType == CcpDataType.YesNo && (newNumericValue != null || newTextValue is not ("Yes" or "No")))
                error = $"{p.ParameterName}: choose Yes or No.";
            else if (p.DataType == CcpDataType.Text && (newNumericValue != null || newTextValue == null))
                error = $"{p.ParameterName}: enter text.";
            else if (p.DataType == null && (newNumericValue == null) == (newTextValue == null))
                error = $"{p.ParameterName}: enter either a number or a text.";
            else if (newTextValue?.Length > 100)
                error = "The text can be at most 100 characters.";

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToMonitoring(owner.Value, ownerId, ccpId);
            }

            var log = await LogsOf(owner.Value, ownerId, ccpId).FirstOrDefaultAsync(l => l.ParameterId == parameterId);
            _context.CcpCorrections.Add(new CcpCorrection
            {
                OwnerKind = KindOf(owner.Value),
                OwnerId = ownerId,
                CcpId = ccpId,
                ParameterId = parameterId,
                OldNumericValue = log?.MeasuredNumericValue,
                OldTextValue = log?.MeasuredTextValue,
                NewNumericValue = newNumericValue,
                NewTextValue = newTextValue,
                Reason = reason!,
                RequestedByUserId = _current.UserId!
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Correction requested; another platform administrator must approve it.";
            return RedirectToMonitoring(owner.Value, ownerId, ccpId);
        }

        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> Corrections()
        {
            if (!_current.Enabled)
                return NotFound();
            var list = await _context.CcpCorrections.AsNoTracking()
                .Include(c => c.Ccp).Include(c => c.Parameter)
                .OrderBy(c => c.Status == CorrectionStatus.Pending ? 0 : 1).ThenByDescending(c => c.CorrectionId)
                .Take(200).ToListAsync();
            var ids = list.SelectMany(c => new[] { c.RequestedByUserId, c.DecidedByUserId }).Where(x => x != null).Distinct().ToList();
            ViewBag.UserNames = await _userManager.Users.Where(u => ids.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Id);
            ViewBag.Controller = "Corrections";
            ViewBag.Action = "Index";
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> DecideCorrection(int correctionId, bool approve, string? note)
        {
            if (!_current.Enabled)
                return NotFound();
            var c = await _context.CcpCorrections.FirstOrDefaultAsync(x => x.CorrectionId == correctionId);
            if (c == null)
                return NotFound();
            if (c.Status != CorrectionStatus.Pending)
                TempData["Error"] = "This correction was already decided.";
            else if (c.RequestedByUserId == _current.UserId)
                TempData["Error"] = "Four-eyes: the person who requested a correction cannot decide it.";
            else if (!approve && string.IsNullOrWhiteSpace(note))
                TempData["Error"] = "A note is required to reject a correction.";
            else
            {
                var owner = ParseOwner(c.OwnerKind)!.Value;
                await using var transaction = await _context.Database.BeginTransactionAsync();
                c.Status = approve ? CorrectionStatus.Applied : CorrectionStatus.Rejected;
                c.DecidedByUserId = _current.UserId;
                c.DecidedAt = DateTime.UtcNow;
                c.DecisionNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
                if (approve)
                {
                    var log = await LogsOf(owner, c.OwnerId, c.CcpId).FirstOrDefaultAsync(l => l.ParameterId == c.ParameterId)
                              ?? NewLog(owner, c.OwnerId, c.CcpId, c.ParameterId);
                    log.MeasuredNumericValue = c.NewNumericValue;
                    log.MeasuredTextValue = c.NewTextValue;
                    log.MeasurementDate = DateTime.Now;
                    log.RecordedByUserId = c.RequestedByUserId;
                    log.RecordedByOrgId = null;
                    await _context.SaveChangesAsync();
                    await RecomputeFlagsAsync(owner, c.OwnerId, c.CcpId, (await LoadParametersAsync(c.CcpId)).ToDictionary(p => p.ParameterId));
                }
                await _context.SaveChangesAsync();
                await _audit.WriteAsync(approve ? "Correct" : "Reject", "CcpCorrections", c.CorrectionId.ToString(),
                    new { c.OldNumericValue, c.OldTextValue }, new { c.NewNumericValue, c.NewTextValue, c.Reason }, "Correction");
                await transaction.CommitAsync();
                TempData["Success"] = approve ? "Correction applied." : "Correction rejected.";
            }
            return RedirectToAction(nameof(Corrections));
        }

        // ------------------------------------------------------------------ file values (D-19)

        public async Task<IActionResult> File(int id)
        {
            var log = await _context.CriticalControlPointMonitoringLogs.FirstOrDefaultAsync(l => l.LogId == id);
            if (log?.StoredFileName == null || log.CCPId == null)
                return NotFound();
            var owner = log.RawHoneyProductionId != null ? Owner.RawLot : Owner.Batch;
            var ownerId = log.RawHoneyProductionId ?? log.FinalHoneyProductionId!.Value;
            if (!(await AuthorizeAsync(owner, ownerId, log.CCPId.Value, CcpOperations.Read)).Succeeded)
                return NotFound();

            var path = Path.Combine(_env.ContentRootPath, FileFolder, Path.GetFileName(log.StoredFileName));
            if (!System.IO.File.Exists(path))
                return NotFound();

            var contentType = Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
            return PhysicalFile(path, contentType, log.MeasuredTextValue ?? Path.GetFileName(path));
        }

        // ------------------------------------------------------------------ helpers

        private enum Owner { RawLot, Batch }

        private static Owner? ParseOwner(string? kind) => kind switch
        {
            "raw" => Owner.RawLot,
            "batch" => Owner.Batch,
            _ => null
        };

        private IActionResult RedirectToMonitoring(Owner owner, int ownerId, int ccpId) => owner == Owner.RawLot
            ? RedirectToAction(nameof(RawHoneyProductionManageCCP), new { rawHoneyProductionId = ownerId, ccpId })
            : RedirectToAction(nameof(FinalHoneyProductionManageCCP), new { finalHoneyProductionId = ownerId, ccpId });

        private Task<HoneyProfile> HoneyProfileAsync(Owner owner, int ownerId) => owner == Owner.RawLot
            ? _ccp.GetHoneyProfileForRawLotAsync(ownerId)
            : _ccp.GetHoneyProfileForBatchAsync(ownerId);

        private Task<string?> LockReasonAsync(Owner owner, int ownerId) => owner == Owner.RawLot
            ? _ccp.GetRawLotLockReasonAsync(ownerId)
            : _ccp.GetBatchLockReasonAsync(ownerId);

        private IQueryable<CriticalControlPointMonitoringLog> LogsOf(Owner owner, int ownerId, int ccpId) => owner == Owner.RawLot
            ? _context.CriticalControlPointMonitoringLogs.Where(l => l.RawHoneyProductionId == ownerId && l.CCPId == ccpId)
            : _context.CriticalControlPointMonitoringLogs.Where(l => l.FinalHoneyProductionId == ownerId && l.CCPId == ccpId);

        private Task<bool> HasValuesAsync(Owner owner, int ownerId, int ccpId) =>
            LogsOf(owner, ownerId, ccpId).AnyAsync();

        // Wraps the raw or final assignment row so both are handled by the same code
        private sealed class AssignmentRef
        {
            public RawHoneyCCPAssignment? Raw { get; init; }
            public FinalHoneyCCPAssignment? Final { get; init; }

            public int Status
            {
                get => Raw?.RawHoneyCCPAssignmentStatus ?? Final!.FinalHoneyCCPAssignmentStatus;
                set { if (Raw != null) Raw.RawHoneyCCPAssignmentStatus = value; else Final!.FinalHoneyCCPAssignmentStatus = value; }
            }
            public DateTime? EndDate
            {
                get => Raw != null ? Raw.RawHoneyCCPAssignmentEndDate : Final!.FinalHoneyCCPAssignmentEndDate;
                set { if (Raw != null) Raw.RawHoneyCCPAssignmentEndDate = value; else Final!.FinalHoneyCCPAssignmentEndDate = value; }
            }
            public string? NotApplicableReason
            {
                get => Raw != null ? Raw.NotApplicableReason : Final!.NotApplicableReason;
                set { if (Raw != null) Raw.NotApplicableReason = value; else Final!.NotApplicableReason = value; }
            }
            public string? NotApplicableBy
            {
                get => Raw != null ? Raw.NotApplicableBy : Final!.NotApplicableBy;
                set { if (Raw != null) Raw.NotApplicableBy = value; else Final!.NotApplicableBy = value; }
            }
            public DateTime? NotApplicableAt
            {
                get => Raw != null ? Raw.NotApplicableAt : Final!.NotApplicableAt;
                set { if (Raw != null) Raw.NotApplicableAt = value; else Final!.NotApplicableAt = value; }
            }
            public string? StatusName => Raw?.AssignmentStatus?.AssignmentStatusName ?? Final?.AssignmentStatus?.AssignmentStatusName;

            public string? ApprovedByUserId
            {
                get => Raw != null ? Raw.ApprovedByUserId : Final!.ApprovedByUserId;
                set { if (Raw != null) Raw.ApprovedByUserId = value; else Final!.ApprovedByUserId = value; }
            }
            public int? ApprovedByOrgId
            {
                get => Raw != null ? Raw.ApprovedByOrgId : Final!.ApprovedByOrgId;
                set { if (Raw != null) Raw.ApprovedByOrgId = value; else Final!.ApprovedByOrgId = value; }
            }
            public DateTime? ApprovedAt
            {
                get => Raw != null ? Raw.ApprovedAt : Final!.ApprovedAt;
                set { if (Raw != null) Raw.ApprovedAt = value; else Final!.ApprovedAt = value; }
            }
            public bool SelfApproved
            {
                get => Raw != null ? Raw.SelfApproved : Final!.SelfApproved;
                set { if (Raw != null) Raw.SelfApproved = value; else Final!.SelfApproved = value; }
            }

            // A changed record needs a new approval
            public void ClearApproval()
            {
                ApprovedByUserId = null;
                ApprovedByOrgId = null;
                ApprovedAt = null;
                SelfApproved = false;
            }
        }

        private async Task<AssignmentRef?> FindAssignmentAsync(Owner owner, int ownerId, int ccpId)
        {
            if (owner == Owner.RawLot)
            {
                var raw = await _context.RawHoneyCCPAssignments.Include(a => a.AssignmentStatus)
                    .FirstOrDefaultAsync(a => a.RawHoneyProductionId == ownerId && a.RawHoneyCCPAssignmentCCP == ccpId);
                return raw == null ? null : new AssignmentRef { Raw = raw };
            }
            var final = await _context.FinalHoneyCCPAssignments.Include(a => a.AssignmentStatus)
                .FirstOrDefaultAsync(a => a.FinalHoneyProductionId == ownerId && a.FinalHoneyCCPAssignmentCCP == ccpId);
            return final == null ? null : new AssignmentRef { Final = final };
        }

        private Task<List<CriticalControlPointParameter>> LoadParametersAsync(int ccpId) =>
            _context.CriticalControlPointParameters
                .Include(p => p.ParameterCategoryNavigation)
                .Include(p => p.Limits).ThenInclude(l => l.ConditionParameter)
                .Include(p => p.Limits).ThenInclude(l => l.SumWithParameter)
                .Include(p => p.Limits).ThenInclude(l => l.RatioWithParameter)
                .Include(p => p.Limits).ThenInclude(l => l.HoneyType)
                .Where(p => p.CCPId == ccpId)
                .OrderBy(p => p.ParameterId)
                .ToListAsync();

        // Header, groups with stored values, status and lock state
        private async Task<bool> FillCommonAsync(CcpMonitoringViewModelBase model, Owner owner, int ownerId, int ccpId)
        {
            var ccp = await _context.CriticalControlPoints.FirstOrDefaultAsync(x => x.Id == ccpId);
            if (ccp == null)
                return false;

            model.CCPId = ccp.Id;
            model.CCPCode = ccp.Code;
            model.CCPTitle = ccp.Title ?? "";
            model.ProcessingStep = ccp.ProcessingStep;
            model.CriticalityJustification = ccp.CriticalityJustification ?? "";

            var parameters = await LoadParametersAsync(ccpId);
            var logs = await LogsOf(owner, ownerId, ccpId).ToListAsync();

            model.Groups = parameters
                .GroupBy(p => p.ParameterCategoryNavigation)
                .Select(g => new CCPParameterGroup
                {
                    CategoryId = g.Key?.CategoryId,
                    CategoryName = g.Key?.CategoryName ?? "Other",
                    CategoryDescription = g.Key?.CategoryDesc,
                    Parameters = g.Select(p =>
                    {
                        var log = logs.FirstOrDefault(x => x.ParameterId == p.ParameterId);
                        return new CCPParameterInput
                        {
                            ParameterId = p.ParameterId,
                            ParameterName = p.ParameterName ?? "",
                            Unit = p.ParameterUnit,
                            CriticalLimit = p.CriticalLimit,
                            DataType = p.DataType,
                            IsRequired = p.IsRequired,
                            RequiredIfParameterId = p.RequiredIfParameterId,
                            RequiredIfValue = p.RequiredIfValue,
                            LimitText = CcpLimitEvaluator.DescribeAll(p),
                            LimitVerified = p.LimitVerified,
                            MeasuredNumericValue = log?.MeasuredNumericValue,
                            MeasuredTextValue = log?.MeasuredTextValue,
                            Notes = log?.Notes,
                            IsInRange = log?.IsInRange,
                            AppliedLimit = log?.AppliedLimit,
                            LogId = log?.LogId,
                            HasStoredFile = log?.StoredFileName != null,
                            LabReportId = log?.LabReportId
                        };
                    }).ToList()
                }).ToList();

            var assignment = await FindAssignmentAsync(owner, ownerId, ccpId);
            model.StatusId = assignment?.Status ?? 0;
            model.StatusName = assignment?.StatusName;
            model.NotApplicableReason = assignment?.NotApplicableReason;
            model.NotApplicableBy = assignment?.NotApplicableBy;
            model.NotApplicableAt = assignment?.NotApplicableAt;
            model.HasValues = logs.Count > 0;
            model.LockReason = await LockReasonAsync(owner, ownerId);
            model.OrgModelOn = _current.Enabled;
            model.AdminCorrections = _current.Enabled && _current.IsPlatformAdmin;
            if (model.AdminCorrections)
            {
                var kind = KindOf(owner);
                model.Corrections = await _context.CcpCorrections.AsNoTracking()
                    .Where(c => c.OwnerKind == kind && c.OwnerId == ownerId && c.CcpId == ccpId)
                    .OrderByDescending(c => c.CorrectionId).Include(c => c.Parameter).ToListAsync();
            }
            var enter = await AuthorizeAsync(owner, ownerId, ccpId, CcpOperations.Enter);
            model.CanEnter = enter.Succeeded;
            model.EnterDeniedReason = enter.Succeeded ? null : ReasonOf(enter) ?? "Your role can read this CCP but not record it.";
            if (_current.Enabled && assignment != null)
            {
                var approve = await AuthorizeAsync(owner, ownerId, ccpId, CcpOperations.Approve);
                model.CanApprove = approve.Succeeded;
                model.ApproveDeniedReason = approve.Succeeded ? null : ReasonOf(approve);
                model.ApprovedAt = assignment.ApprovedAt;
                model.SelfApproved = assignment.SelfApproved;
                if (assignment.ApprovedByUserId != null)
                    model.ApprovedBy = (await _userManager.FindByIdAsync(assignment.ApprovedByUserId))?.UserName ?? assignment.ApprovedByUserId;
                // The self-approval marker of the check above must not leak into a later action
                HttpContext.Items.Remove(CcpAuthorizationHandler.SelfApprovalKey);
            }
            model.HoneyClass = (await HoneyProfileAsync(owner, ownerId)).HoneyClass;
            return true;
        }

        private async Task<CCPMonitoringViewModelForRawHoney?> BuildRawModelAsync(int rawHoneyProductionId, int ccpId)
        {
            var production = await _context.RawHoneyProductions
                .Include(r => r.RawHoneyProductionApiary)
                .ThenInclude(a => a!.ApiaryBeekeeper)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .FirstOrDefaultAsync(r => r.RawHoneyProductionId == rawHoneyProductionId);
            if (production == null)
                return null;

            var model = new CCPMonitoringViewModelForRawHoney
            {
                RawHoneyProductionId = rawHoneyProductionId,
                BeekeeperName = production.RawHoneyProductionApiary?.ApiaryBeekeeper?.BeekeeperFirstName + " " + production.RawHoneyProductionApiary?.ApiaryBeekeeper?.BeekeeperLastName,
                ApiaryName = production.RawHoneyProductionApiary?.ApiaryTitle,
                HoneyType = production.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle,
                ProductionDate = production.RawHoneyProductionDate?.ToDateTime(TimeOnly.MinValue),
                Amount = production.RawHoneyProductionAmount,
                ApiaryId = production.RawHoneyProductionApiaryId ?? 0
            };
            return await FillCommonAsync(model, Owner.RawLot, rawHoneyProductionId, ccpId) ? model : null;
        }

        private async Task<CCPMonitoringViewModelForFinalBatch?> BuildBatchModelAsync(int finalHoneyProductionId, int ccpId)
        {
            var batch = await _context.FinalHoneyProductions
                .Include(f => f.FinalHoneyProductionWholesaler)
                .Include(f => f.FinalHoneyProductionDetails)
                    .ThenInclude(d => d.RawHoneyProduction)
                        .ThenInclude(r => r.RawHoneyProductionApiary)
                            .ThenInclude(a => a!.ApiaryBeekeeper)
                .Include(f => f.FinalHoneyProductionDetails)
                    .ThenInclude(d => d.RawHoneyProduction)
                        .ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == finalHoneyProductionId);
            if (batch == null)
                return null;

            var model = new CCPMonitoringViewModelForFinalBatch
            {
                FinalHoneyProductionId = finalHoneyProductionId,
                BatchTitle = batch.FinalHoneyProductionTitle,
                BatchQR = batch.FinalHoneyProductionQR,
                ProductionDate = batch.FinalHoneyProductionDate,
                TotalAmount = batch.FinalHoneyProductionTotalAmount,
                WholesalerName = batch.FinalHoneyProductionWholesaler?.WholesalerTitle,
                WholesalerId = batch.FinalHoneyProductionWholesalerId,
                RawSourceCount = batch.FinalHoneyProductionDetails.Count
            };
            foreach (var detail in batch.FinalHoneyProductionDetails)
            {
                var raw = detail.RawHoneyProduction;
                model.RawSources.Add(new FinalBatchRawSourceInfo
                {
                    ApiaryName = raw?.RawHoneyProductionApiary?.ApiaryTitle,
                    BeekeeperName = raw?.RawHoneyProductionApiary?.ApiaryBeekeeper?.BeekeeperFirstName
                                    + " " + raw?.RawHoneyProductionApiary?.ApiaryBeekeeper?.BeekeeperLastName,
                    HoneyType = raw?.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle,
                    UsedAmount = detail.UsedAmount
                });
            }
            return await FillCommonAsync(model, Owner.Batch, finalHoneyProductionId, ccpId) ? model : null;
        }

        // Shows the page again with the posted values and the errors; nothing was saved
        private IActionResult ShowErrors(CcpMonitoringViewModelBase display, List<CCPParameterGroup>? posted, List<string> errors)
        {
            var postedById = (posted ?? new()).SelectMany(g => g.Parameters ?? new()).GroupBy(p => p.ParameterId)
                .ToDictionary(g => g.Key, g => g.First());
            foreach (var input in display.Groups.SelectMany(g => g.Parameters))
            {
                if (!postedById.TryGetValue(input.ParameterId, out var p))
                    continue;
                input.MeasuredNumericValue = p.MeasuredNumericValue;
                input.MeasuredTextValue = p.MeasuredTextValue;
                input.Notes = p.Notes;
            }

            ModelState.Clear();
            foreach (var e in errors)
                ModelState.AddModelError(string.Empty, e);

            ViewBag.Action = "Manage";
            return View(display);
        }

        // Validates and stores the posted values, then refreshes the range flags and the status.
        // Returns null on success, or the list of errors (nothing saved).
        private async Task<List<string>?> SaveValuesAsync(Owner owner, int ownerId, int ccpId, List<CCPParameterGroup>? groups)
        {
            var assignment = await FindAssignmentAsync(owner, ownerId, ccpId);
            if (assignment == null)
                return new() { "This CCP is not assigned here, so no values can be recorded." };
            if (await LockReasonAsync(owner, ownerId) is string lockReason)
                return new() { lockReason };
            if (assignment.Status == AssignmentStatusIds.NotApplicable)
                return new() { "This CCP is marked \"Not applicable\". Remove that mark first to record values." };
            var enter = await AuthorizeAsync(owner, ownerId, ccpId, CcpOperations.Enter);
            if (!enter.Succeeded)
                return new() { ReasonOf(enter) ?? "Your role can read this CCP but not record it." };
            var active = await _current.GetAsync();
            var changed = false;

            var parameters = (await LoadParametersAsync(ccpId)).ToDictionary(p => p.ParameterId);
            var posted = (groups ?? new()).SelectMany(g => g.Parameters ?? new())
                .Where(p => parameters.ContainsKey(p.ParameterId))   // ignore ids of other CCPs
                .GroupBy(p => p.ParameterId).Select(g => g.First())
                .ToList();

            // 1) validate everything first
            var errors = new List<string>();
            foreach (var input in posted)
            {
                var p = parameters[input.ParameterId];
                var name = p.ParameterName;
                input.MeasuredTextValue = string.IsNullOrWhiteSpace(input.MeasuredTextValue) ? null : input.MeasuredTextValue.Trim();
                input.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();

                switch (p.DataType)
                {
                    case CcpDataType.Number:
                        if (input.MeasuredTextValue != null)
                            errors.Add($"{name}: enter a number.");
                        break;
                    case CcpDataType.YesNo:
                        if (input.MeasuredNumericValue != null || (input.MeasuredTextValue != null && input.MeasuredTextValue is not ("Yes" or "No")))
                            errors.Add($"{name}: choose Yes or No.");
                        break;
                    case CcpDataType.Text:
                        if (input.MeasuredNumericValue != null)
                            errors.Add($"{name}: enter text, not a number.");
                        break;
                    case CcpDataType.File:
                        input.MeasuredNumericValue = null;
                        input.MeasuredTextValue = null;   // the file name is taken from the upload
                        if (input.File != null && await UploadValidator.ValidateDocumentAsync(input.File) is string fileError)
                            errors.Add($"{name}: {fileError}");
                        break;
                    default:
                        if (input.MeasuredNumericValue != null && input.MeasuredTextValue != null)
                            errors.Add($"{name}: enter either a number or a text, not both.");
                        break;
                }
                if (input.MeasuredTextValue?.Length > 100)
                    errors.Add($"{name}: the text can be at most 100 characters.");
                if (input.Notes?.Length > 500)
                    errors.Add($"{name}: the notes can be at most 500 characters.");
            }
            if (errors.Count > 0)
                return errors;

            // 2) store
            var dir = Path.Combine(_env.ContentRootPath, FileFolder);
            var newFiles = new List<string>();
            var obsoleteFiles = new List<string>();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingLogs = await LogsOf(owner, ownerId, ccpId).ToListAsync();
                foreach (var input in posted)
                {
                    var p = parameters[input.ParameterId];
                    var log = existingLogs.FirstOrDefault(x => x.ParameterId == input.ParameterId);

                    // Values from an approved lab report are immutable; only a correction changes them (D-42)
                    if (log?.LabReportId != null)
                        continue;

                    if (p.DataType == CcpDataType.File)
                    {
                        if (input.File != null)
                        {
                            Directory.CreateDirectory(dir);
                            var stored = Guid.NewGuid().ToString("N") + UploadValidator.SafeDocumentExtension(input.File);
                            await using (var fs = new FileStream(Path.Combine(dir, stored), FileMode.CreateNew))
                                await input.File.CopyToAsync(fs);
                            newFiles.Add(stored);

                            if (log?.StoredFileName != null)
                                obsoleteFiles.Add(log.StoredFileName);
                            log ??= NewLog(owner, ownerId, ccpId, p.ParameterId);
                            changed = true;
                            log.RecordedByUserId = _current.UserId;
                            log.RecordedByOrgId = active?.OrgId;
                            log.StoredFileName = stored;
                            log.MeasuredTextValue = Truncate(Path.GetFileName(input.File.FileName), 100);
                            log.MeasuredNumericValue = null;
                            log.Notes = input.Notes;
                            log.MeasurementDate = DateTime.Now;
                        }
                        else if (input.RemoveFile && log != null)
                        {
                            if (log.StoredFileName != null)
                                obsoleteFiles.Add(log.StoredFileName);
                            _context.CriticalControlPointMonitoringLogs.Remove(log);
                            changed = true;
                        }
                        else if (log != null && log.Notes != input.Notes)
                        {
                            log.Notes = input.Notes;
                        }
                        continue;
                    }

                    var hasValue = input.MeasuredNumericValue != null || input.MeasuredTextValue != null;
                    if (!hasValue)
                    {
                        if (log != null)
                        {
                            _context.CriticalControlPointMonitoringLogs.Remove(log);
                            changed = true;
                        }
                        continue;
                    }

                    log ??= NewLog(owner, ownerId, ccpId, p.ParameterId);
                    if (log.MeasuredNumericValue != input.MeasuredNumericValue || log.MeasuredTextValue != input.MeasuredTextValue
                        || log.Notes != input.Notes || log.MeasurementDate == null)
                    {
                        log.MeasuredNumericValue = input.MeasuredNumericValue;
                        log.MeasuredTextValue = input.MeasuredTextValue;
                        log.Notes = input.Notes;
                        log.MeasurementDate = DateTime.Now;
                        log.RecordedByUserId = _current.UserId;
                        log.RecordedByOrgId = active?.OrgId;
                        changed = true;
                    }
                }
                await _context.SaveChangesAsync();

                // 3) range flags
                var logs = await RecomputeFlagsAsync(owner, ownerId, ccpId, parameters);

                // 4) status: complete when every required parameter has a value. With the organization model
                // a complete record is Submitted until another person approves it (D-27); a save without
                // changes keeps an existing approval.
                var filled = logs.Where(l => l.ParameterId != null).Select(l => l.ParameterId!.Value).ToHashSet();
                var answers = CcpRequirement.Answers(logs);
                var required = parameters.Values.Where(p => CcpRequirement.IsRequired(p, answers)).Select(p => p.ParameterId).ToList();
                var complete = parameters.Count > 0 && required.All(filled.Contains) && (required.Count > 0 || filled.Count > 0);
                if (changed)
                    assignment.ClearApproval();
                var needsApproval = _current.Enabled && await _resp.RequiresApprovalAsync(ccpId);
                assignment.Status = complete
                    ? (needsApproval && assignment.ApprovedAt == null ? AssignmentStatusIds.Submitted : AssignmentStatusIds.Completed)
                    : logs.Count > 0 ? AssignmentStatusIds.InProgress
                    : AssignmentStatusIds.Assigned;
                assignment.EndDate = complete ? (assignment.EndDate ?? DateTime.Now) : null;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                foreach (var f in newFiles)
                    TryDelete(Path.Combine(dir, f));
                throw;
            }

            foreach (var f in obsoleteFiles)
                TryDelete(Path.Combine(dir, Path.GetFileName(f)));
            return null;
        }

        // In-range flags of all values of a lot/batch CCP; siblings are the numeric values of the same record
        private async Task<List<CriticalControlPointMonitoringLog>> RecomputeFlagsAsync(Owner owner, int ownerId, int ccpId,
            Dictionary<int, CriticalControlPointParameter> parameters)
        {
            var logs = await LogsOf(owner, ownerId, ccpId).ToListAsync();
            var siblings = logs.Where(l => l.ParameterId != null && l.MeasuredNumericValue != null)
                .GroupBy(l => l.ParameterId!.Value)
                .ToDictionary(g => g.Key, g => g.First().MeasuredNumericValue!.Value);
            var honey = await HoneyProfileAsync(owner, ownerId);
            foreach (var log in logs)
            {
                if (log.ParameterId == null || !parameters.TryGetValue(log.ParameterId.Value, out var p))
                    continue;
                var result = CcpLimitEvaluator.Evaluate(p, log.MeasuredNumericValue, siblings, honey);
                log.IsInRange = result.IsInRange;
                log.AppliedLimit = result.AppliedLimit;
            }
            return logs;
        }

        private CriticalControlPointMonitoringLog NewLog(Owner owner, int ownerId, int ccpId, int parameterId)
        {
            var log = new CriticalControlPointMonitoringLog
            {
                CCPId = ccpId,
                ParameterId = parameterId,
                RawHoneyProductionId = owner == Owner.RawLot ? ownerId : null,
                FinalHoneyProductionId = owner == Owner.Batch ? ownerId : null
            };
            _context.CriticalControlPointMonitoringLogs.Add(log);
            return log;
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

        private static void TryDelete(string path)
        {
            try { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); }
            catch (IOException) { }
        }
    }
}
