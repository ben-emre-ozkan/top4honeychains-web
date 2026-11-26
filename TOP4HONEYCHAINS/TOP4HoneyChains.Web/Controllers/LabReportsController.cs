using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Lab results (ROLE-MODEL section 7, D-42): the Analyst of the receiving laboratory enters the results
    // per CCP 1 parameter and uploads the report PDF (SHA-256 stored), then submits; a LabManager, always
    // another person (no four-eyes exemption for lab results, D-27), approves. Approved results become the
    // lot's CCP values and are immutable.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class LabReportsController : Controller
    {
        // D-56: rights of organization roles (editable matrix); lab results always need a second person (fixed)
        private TOP4HoneyChains.Web.Services.OrgModel.Permissions Perm => HttpContext.RequestServices.GetRequiredService<TOP4HoneyChains.Web.Services.OrgModel.Permissions>();
        private string[] EnterRoles => Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.LabResults, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter);
        private string[] ApproveRoles => Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.LabResults, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve);

        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly LabService _lab;
        private readonly CcpIntegrityService _ccp;
        private readonly AuditWriter _audit;
        private readonly IWebHostEnvironment _env;
        private readonly CcpResponsibilityService _resp;
        private LabCustodyService LabCustody => HttpContext.RequestServices.GetRequiredService<LabCustodyService>();

        public LabReportsController(TOP4HoneyChainsContext context, ICurrentOrg current, LabService lab, CcpIntegrityService ccp,
            AuditWriter audit, IWebHostEnvironment env, CcpResponsibilityService resp)
        {
            _resp = resp;
            _context = context;
            _current = current;
            _lab = lab;
            _ccp = ccp;
            _audit = audit;
            _env = env;
        }

        // Administrators, the laboratory and the organization that took the sample may read
        private async Task<Sample?> ReadableSampleAsync(int sampleId)
        {
            var sample = await _context.Samples
                .Include(s => s.LabOrg).Include(s => s.TakingOrg).Include(s => s.Events)
                .Include(s => s.Report!).ThenInclude(r => r.Results)
                .Include(s => s.RawHoneyProduction!).ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .FirstOrDefaultAsync(s => s.SampleId == sampleId);
            if (sample == null)
                return null;
            if (_current.IsPlatformAdmin)
                return sample;
            var active = await _current.GetAsync();
            return active != null && (active.OrgId == sample.LabOrgId || active.OrgId == sample.TakingOrgId)
                   && Perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.LabResults, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, active.OrgRole) ? sample : null;
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sample = await ReadableSampleAsync(id);
            if (sample == null)
                return NotFound();
            var ccpId = await _lab.LabCcpIdAsync() ?? 0;
            ViewBag.Sample = sample;
            ViewBag.Parameters = await _lab.LabParametersAsync(ccpId);
            var ccp0 = await _lab.Ccp0PendingAsync(sample.RawHoneyProductionId);
            ViewBag.EnterDenied = sample.Status != SampleStatus.Received ? "Results are entered only for a received sample with an intact seal."
                : await _lab.LabDeniedAsync(sample.LabOrgId, EnterRoles) ?? ccp0 ?? await LabCustody.ReportDeniedAsync(sample);
            ViewBag.SetsLotValues = LabCustody.SetsLotValues(sample);
            // D-82: the values of every container of the lot next to the lot's CCP 1 value
            if (sample.Kind == SampleKind.Container)
            {
                ViewBag.ContainerResults = await LabCustody.ContainerResultsAsync(sample.RawHoneyProductionId);
                ViewBag.LotLabValues = await _context.CriticalControlPointMonitoringLogs.AsNoTracking()
                    .Where(l => l.RawHoneyProductionId == sample.RawHoneyProductionId && l.CCPId == ccpId && l.ParameterId != null)
                    .ToDictionaryAsync(l => l.ParameterId!.Value);
            }
            ViewBag.ApproveDenied = await _lab.LabDeniedAsync(sample.LabOrgId, ApproveRoles);
            ViewBag.Ccp0Pending = ccp0;
            ViewBag.UserId = _current.UserId;
            ViewBag.Controller = "Lab";
            ViewBag.Action = "Report";
            return View(sample.Report);
        }

        private static decimal? ParseNumber(string raw) =>
            DecimalInput.Parse(raw);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Save(int id, string? reportNumber, DateOnly? issuedOn, [Bind(Prefix = "values")] Dictionary<int, string?>? values, IFormFile? reportFile)
        {
            var sample = await ReadableSampleAsync(id);
            if (sample == null)
                return NotFound();
            string? error = sample.Status != SampleStatus.Received ? "Results are entered only for a received sample with an intact seal."
                : await _lab.LabDeniedAsync(sample.LabOrgId, EnterRoles) ?? await _lab.Ccp0PendingAsync(sample.RawHoneyProductionId) ?? await LabCustody.ReportDeniedAsync(sample);
            var report = sample.Report;
            if (error == null && report != null && report.Status != LabReportStatus.Draft)
                error = report.Status == LabReportStatus.Approved
                    ? "An approved lab report cannot be changed; request a correction instead."
                    : $"The report is submitted; the {OrgRoles.LabManager} approves it or returns it for changes.";
            // D-75: a draft cannot be saved either once the lot is locked in a finalized batch
            if (error == null && await _ccp.GetRawLotLockReasonAsync(sample.RawHoneyProductionId) != null)
                error = "The lot is locked in a finalized batch; lab values can only be changed through a correction.";
            reportNumber = string.IsNullOrWhiteSpace(reportNumber) ? null : reportNumber.Trim();
            if (error == null && reportNumber?.Length > 50)
                error = "The report number can be at most 50 characters.";
            if (error == null && issuedOn > DateOnly.FromDateTime(DateTime.Today))
                error = "The issue date cannot be in the future.";
            if (error == null && reportFile != null)
            {
                if (await UploadValidator.ValidateDocumentAsync(reportFile) is string fileError)
                    error = fileError;
                else if (!UploadValidator.SafeDocumentExtension(reportFile).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    error = "The lab report must be a PDF.";
            }

            // results: parse and check every value first
            var ccpId = await _lab.LabCcpIdAsync() ?? 0;
            var parameters = await _lab.LabParametersAsync(ccpId);
            var parsed = new Dictionary<int, (decimal? Number, string? Text)>();
            foreach (var p in parameters)
            {
                if (error != null)
                    break;
                string? raw = null;
                values?.TryGetValue(p.ParameterId, out raw);
                raw = raw?.Trim();
                if (string.IsNullOrEmpty(raw))
                    continue;
                if (p.DataType == CcpDataType.Number)
                {
                    if (ParseNumber(raw) is decimal n) parsed[p.ParameterId] = (n, null);
                    else error = $"{p.ParameterName}: enter a number.";
                }
                else if (p.DataType == CcpDataType.YesNo && raw is not ("Yes" or "No"))
                    error = $"{p.ParameterName}: choose Yes or No.";
                else if (raw.Length > 100)
                    error = "The text can be at most 100 characters.";
                else
                    parsed[p.ParameterId] = (null, raw);
            }
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Edit), new { id });
            }

            string? newFile = null;
            if (reportFile != null)
            {
                var (stored, sha) = await FileStore.SaveAsync(_env, LabService.Reports, reportFile);
                newFile = stored;
                if (report?.StoredFileName != null && FileStore.PathOf(_env, LabService.Reports, report.StoredFileName) is string old)
                    System.IO.File.Delete(old);
                report ??= NewReport(sample);
                report.StoredFileName = stored;
                report.OriginalFileName = Path.GetFileName(reportFile.FileName);
                report.Sha256 = sha;
            }
            try
            {
                report ??= NewReport(sample);
                report.ReportNumber = reportNumber;
                report.IssuedOn = issuedOn;
                report.EnteredByUserId = _current.UserId!;
                report.ReturnReason = null;
                var siblings = parsed.Where(kv => kv.Value.Number != null).ToDictionary(kv => kv.Key, kv => kv.Value.Number!.Value);
                var honey = await _ccp.GetHoneyProfileForRawLotAsync(sample.RawHoneyProductionId);
                foreach (var p in parameters)
                {
                    var existing = report.Results.FirstOrDefault(r => r.ParameterId == p.ParameterId);
                    if (!parsed.TryGetValue(p.ParameterId, out var v))
                    {
                        if (existing != null)
                            _context.LabResults.Remove(existing);
                        continue;
                    }
                    if (existing == null)
                    {
                        existing = new LabResult { ParameterId = p.ParameterId };
                        report.Results.Add(existing);
                    }
                    existing.MeasuredNumericValue = v.Number;
                    existing.MeasuredTextValue = v.Text;
                    var result = CcpLimitEvaluator.Evaluate(p, v.Number, siblings, honey);
                    existing.IsInRange = result.IsInRange;
                    existing.AppliedLimit = result.AppliedLimit;
                }
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (newFile != null && FileStore.PathOf(_env, LabService.Reports, newFile) is string path)
                    System.IO.File.Delete(path);
                throw;
            }
            TempData["Success"] = "Lab results saved.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        private LabReport NewReport(Sample sample)
        {
            var report = new LabReport
            {
                SampleId = sample.SampleId,
                LabOrgId = sample.LabOrgId,
                ReportCode = LabService.NewReportCode(),
                EnteredByUserId = _current.UserId!,
                CreatedAt = DateTime.UtcNow
            };
            _context.LabReports.Add(report);
            sample.Report = report;
            return report;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var sample = await ReadableSampleAsync(id);
            if (sample == null)
                return NotFound();
            var report = sample.Report;
            string? error = await _lab.LabDeniedAsync(sample.LabOrgId, EnterRoles) ?? await _lab.Ccp0PendingAsync(sample.RawHoneyProductionId);
            if (error == null && (report == null || report.Status != LabReportStatus.Draft))
                error = "Only a draft report can be submitted.";
            else if (error == null && await _ccp.GetRawLotLockReasonAsync(sample.RawHoneyProductionId) != null)
                error = "The lot is locked in a finalized batch; lab values can only be changed through a correction.";   // D-75
            else if (error == null && report!.Results.Count == 0)
                error = "Enter at least one result before submitting.";
            else if (error == null && report!.StoredFileName == null)
                error = "Upload the report PDF before submitting.";
            else if (error == null && (report!.ReportNumber == null || report.IssuedOn == null))
                error = "Enter the report number and the issue date before submitting.";
            if (error == null)
            {
                // D-54: every required parameter needs a value before the report goes to the lab manager
                var parameters = (await _lab.LabParametersAsync(await _lab.LabCcpIdAsync() ?? 0)).Where(p => p.IsRequired).ToList();
                var missing = LabService.MissingRequired(report, parameters);
                if (missing.Count > 0)
                    error = $"Enter all required values before submitting: {parameters.Count - missing.Count} of {parameters.Count} entered.";
            }
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Edit), new { id });
            }
            report!.Status = LabReportStatus.Submitted;
            report.SubmittedAt = DateTime.UtcNow;
            report.EnteredByUserId = _current.UserId!;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Report submitted; a {OrgRoles.LabManager} (another person) must approve it.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decide(int id, bool approve, string? reason, bool confirmOutOfLimit = false)
        {
            var sample = await ReadableSampleAsync(id);
            if (sample == null)
                return NotFound();
            var report = sample.Report;
            reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            // returning a report is always possible; approving waits for the lot's CCP 0 (D-61)
            string? error = await _lab.LabDeniedAsync(sample.LabOrgId, ApproveRoles) ?? (approve ? await _lab.Ccp0PendingAsync(sample.RawHoneyProductionId) : null);
            if (error == null && (report == null || report.Status != LabReportStatus.Submitted))
                error = "Only a submitted report can be approved or returned.";
            // D-64: values outside their limits are approved only when confirmed explicitly
            else if (error == null && approve && !confirmOutOfLimit && report!.Results.Any(r => r.IsInRange == false))
                error = "The record has values outside their limits; confirm them to approve.";
            // D-27: four-eyes is never waived for lab results
            else if (error == null && report!.EnteredByUserId == _current.UserId)
                error = "Four-eyes: the person who entered the results cannot approve them (no exemption for lab results).";
            else if (error == null && !approve && reason == null)
                error = "A reason is required to return a report.";
            else if (error == null && reason?.Length > 500)
                error = "The reason can be at most 500 characters.";
            if (error == null && approve)
            {
                // The stored file must still match its hash
                var path = FileStore.PathOf(_env, LabService.Reports, report!.StoredFileName);
                if (path == null || await LabService.Sha256OfFileAsync(path) != report.Sha256)
                    error = "The stored report file does not match its SHA-256 hash; upload it again.";
                else if (await _ccp.GetRawLotLockReasonAsync(sample.RawHoneyProductionId) != null)
                    error = "The lot is locked in a finalized batch; lab values can only be changed through a correction.";
            }
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (!approve)
            {
                report!.Status = LabReportStatus.Draft;
                report.ReturnReason = reason;
                report.SubmittedAt = null;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Report returned to the analyst.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var (accredited, number) = await _lab.AccreditationAsync(sample.LabOrgId);
            var lotOwner = await _resp.LotOwnerAsync(sample.RawHoneyProductionId);
            report!.Status = LabReportStatus.Approved;
            report.ApprovedByUserId = _current.UserId;
            report.ApprovedAt = DateTime.UtcNow;
            report.IsAccredited = accredited;
            report.AccreditationNumber = number;
            report.InHouse = lotOwner == sample.LabOrgId;
            await _context.SaveChangesAsync();
            // D-78 P2: in composite mode a single container jar's report is kept with the jar and does not replace the lot's CCP 1 values
            var setsLot = LabCustody.SetsLotValues(sample);
            if (setsLot)
            {
                // D-82: in per-container mode the lot gets the worst value of its containers, every container's value stays on its report
                IReadOnlyDictionary<int, LabService.LabValue>? values = null;
                if (sample.Kind == SampleKind.Container)
                    values = await LabCustody.WorstValuesAsync(sample.RawHoneyProductionId, await _lab.LabParametersAsync(await _lab.LabCcpIdAsync() ?? 0),
                        await _ccp.GetHoneyProfileForRawLotAsync(sample.RawHoneyProductionId));
                await _lab.ApplyApprovedReportAsync(report, sample, _current.UserId!, values);
                await _context.SaveChangesAsync();
            }
            await _audit.WriteAsync("Approve", "LabReports", report.ReportId.ToString(), null,
                new { report.ReportCode, report.Sha256, report.IsAccredited, report.InHouse });
            await transaction.CommitAsync();
            TempData["Success"] = sample.Kind == SampleKind.Batch ? "Lab report approved; it is the batch's final honey result."
                : setsLot ? "Lab report approved; the results are now the lot's CCP 1 values."
                : "Lab report approved for this container's jar; the lot's CCP 1 values stay those of the composite.";
            TempData["JustApproved"] = true;   // D-67: the status pill draws its check mark once
            return RedirectToAction(nameof(Edit), new { id });
        }

        public async Task<IActionResult> Pdf(int id)
        {
            var sample = await ReadableSampleAsync(id);
            if (sample?.Report?.StoredFileName == null)
                return NotFound();
            var path = FileStore.PathOf(_env, LabService.Reports, sample.Report.StoredFileName);
            if (path == null)
                return NotFound();
            return PhysicalFile(path, "application/pdf", sample.Report.OriginalFileName ?? "report.pdf");
        }
    }
}
