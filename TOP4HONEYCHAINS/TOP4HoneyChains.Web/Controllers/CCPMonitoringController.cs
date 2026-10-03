using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.ViewModels.Monitoring;
using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.Controllers
{
    // CCP values for raw honey lots and final batches. Rules: values only for an assigned CCP that
    // applies to the record (D-17), never for a locked record (D-14) or a "Not applicable" CCP (D-15);
    // values are checked against the parameter's data type and limits (D-18); file values are stored
    // outside wwwroot (D-19).
    [Authorize(Roles = "Administrator")]
    public class CCPMonitoringController : Controller
    {
        private const string FileFolder = "App_Data/ccp-files";

        private readonly TOP4HoneyChainsContext _context;
        private readonly CcpIntegrityService _ccp;
        private readonly IWebHostEnvironment _env;

        public CCPMonitoringController(TOP4HoneyChainsContext context, CcpIntegrityService ccp, IWebHostEnvironment env)
        {
            _context = context;
            _ccp = ccp;
            _env = env;
        }

        // ------------------------------------------------------------------ raw honey lot

        public async Task<IActionResult> RawHoneyProductionManageCCP(int rawHoneyProductionId, int ccpId)
        {
            var model = await BuildRawModelAsync(rawHoneyProductionId, ccpId);
            if (model == null)
                return NotFound();
            if (await FindAssignmentAsync(Owner.RawLot, rawHoneyProductionId, ccpId) == null)
            {
                TempData["Error"] = "This CCP is not assigned to the lot.";
                return RedirectToAction("RawHoneyProductionByApiary", "RawHoneyProductions", new { id = model.ApiaryId });
            }

            ViewBag.Controller = "Raw Honey Production Manage CCP";
            ViewBag.Action = "Manage";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RawHoneyProductionManageCCP(CCPMonitoringViewModelForRawHoney model)
        {
            var display = await BuildRawModelAsync(model.RawHoneyProductionId, model.CCPId);
            if (display == null)
                return NotFound();

            var errors = await SaveValuesAsync(Owner.RawLot, model.RawHoneyProductionId, model.CCPId, model.Groups);
            if (errors == null)
            {
                TempData["Success"] = $"{display.CCPCode} values saved.";
                return RedirectToAction("RawHoneyProductionByApiary", "RawHoneyProductions", new { id = display.ApiaryId });
            }

            return ShowErrors(display, model.Groups, errors);
        }

        // ------------------------------------------------------------------ final batch

        public async Task<IActionResult> FinalHoneyProductionManageCCP(int finalHoneyProductionId, int ccpId)
        {
            var model = await BuildBatchModelAsync(finalHoneyProductionId, ccpId);
            if (model == null)
                return NotFound();
            if (await FindAssignmentAsync(Owner.Batch, finalHoneyProductionId, ccpId) == null)
            {
                TempData["Error"] = "This CCP is not assigned to the batch.";
                return RedirectToAction("ByWholesaler", "FinalHoneyProductions", new { wholesalerId = model.WholesalerId });
            }

            ViewBag.Controller = "Final Honey Production Manage CCP";
            ViewBag.Action = "Manage";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalHoneyProductionManageCCP(CCPMonitoringViewModelForFinalBatch model)
        {
            var display = await BuildBatchModelAsync(model.FinalHoneyProductionId, model.CCPId);
            if (display == null)
                return NotFound();

            var errors = await SaveValuesAsync(Owner.Batch, model.FinalHoneyProductionId, model.CCPId, model.Groups);
            if (errors == null)
            {
                TempData["Success"] = $"{display.CCPCode} values saved.";
                return RedirectToAction("ByWholesaler", "FinalHoneyProductions", new { wholesalerId = display.WholesalerId });
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
            if (assignment == null)
                return NotFound();

            reason = reason?.Trim();
            string? error = await LockReasonAsync(owner.Value, ownerId);
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
            if (assignment == null)
                return NotFound();

            if (await LockReasonAsync(owner.Value, ownerId) is string lockReason)
            {
                TempData["Error"] = lockReason;
            }
            else if (assignment.Status == AssignmentStatusIds.NotApplicable)
            {
                assignment.Status = AssignmentStatusIds.Assigned;
                assignment.NotApplicableReason = null;
                assignment.NotApplicableBy = null;
                assignment.NotApplicableAt = null;
                assignment.EndDate = null;
                await _context.SaveChangesAsync();
                TempData["Success"] = "\"Not applicable\" was removed; values can be recorded again.";
            }
            return RedirectToMonitoring(owner.Value, ownerId, ccpId);
        }

        // ------------------------------------------------------------------ file values (D-19)

        public async Task<IActionResult> File(int id)
        {
            var log = await _context.CriticalControlPointMonitoringLogs.FirstOrDefaultAsync(l => l.LogId == id);
            if (log?.StoredFileName == null)
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
                            LimitText = CcpLimitEvaluator.DescribeAll(p),
                            LimitVerified = p.LimitVerified,
                            MeasuredNumericValue = log?.MeasuredNumericValue,
                            MeasuredTextValue = log?.MeasuredTextValue,
                            Notes = log?.Notes,
                            IsInRange = log?.IsInRange,
                            AppliedLimit = log?.AppliedLimit,
                            LogId = log?.LogId,
                            HasStoredFile = log?.StoredFileName != null
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
                            _context.CriticalControlPointMonitoringLogs.Remove(log);
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
                    }
                }
                await _context.SaveChangesAsync();

                // 3) range flags: siblings are the numeric values of the same lot/batch and CCP
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

                // 4) status: Completed when every required parameter has a value
                var filled = logs.Where(l => l.ParameterId != null).Select(l => l.ParameterId!.Value).ToHashSet();
                var required = parameters.Values.Where(p => p.IsRequired).Select(p => p.ParameterId).ToList();
                var complete = parameters.Count > 0 && required.All(filled.Contains) && (required.Count > 0 || filled.Count > 0);
                assignment.Status = complete ? AssignmentStatusIds.Completed
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
