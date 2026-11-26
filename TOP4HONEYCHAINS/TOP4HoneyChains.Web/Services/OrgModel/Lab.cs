using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // Samples and lab results (ROLE-MODEL section 7, D-42)
    public class LabService
    {
        public const string Reports = "App_Data/lab-reports";

        // Lab roles: both record a sample's receipt; the Analyst enters results, the LabManager approves
        public static readonly string[] ReceiveRoles = { OrgRoles.Analyst, OrgRoles.LabManager };

        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly CcpIntegrityService _ccp;

        public LabService(TOP4HoneyChainsContext context, ICurrentOrg current, CcpIntegrityService ccp)
        {
            _context = context;
            _current = current;
            _ccp = ccp;
        }

        // The control point whose parameters the laboratory analyses (CCP 1, "lab part")
        public Task<int?> LabCcpIdAsync() =>
            _context.CriticalControlPoints.Where(c => c.Code == "CCP 1").Select(c => (int?)c.Id).FirstOrDefaultAsync();

        public Task<List<CriticalControlPointParameter>> LabParametersAsync(int ccpId) =>
            _context.CriticalControlPointParameters.AsNoTracking()
                .Where(p => p.CCPId == ccpId && p.DataType != CcpDataType.File)
                .Include(p => p.Limits).ThenInclude(l => l.ConditionParameter)
                .Include(p => p.Limits).ThenInclude(l => l.SumWithParameter)
                .Include(p => p.Limits).ThenInclude(l => l.RatioWithParameter)
                .Include(p => p.Limits).ThenInclude(l => l.HoneyType)   // D-67: the related parameters name the limits
                .OrderBy(p => p.ParameterId).ToListAsync();

        // D-54: required lab parameters that have no value in the report yet
        public static List<CriticalControlPointParameter> MissingRequired(LabReport? report, IEnumerable<CriticalControlPointParameter> parameters) =>
            parameters.Where(p => p.IsRequired && !(report?.Results.Any(r => r.ParameterId == p.ParameterId
                && (r.MeasuredNumericValue != null || !string.IsNullOrWhiteSpace(r.MeasuredTextValue))) ?? false)).ToList();

        // D-61: the lot's CCP 0 (primary audit, by the association's field inspector or whoever the CCP responsibility
        // matrix names) comes before the lab test. Results are entered and approved only when it is done (approved, or
        // not applicable); a lot without a CCP 0 assignment is not held.
        public const string Ccp0Code = "CCP 0";
        public const string LabCcpCode = "CCP 1";
        public const string Ccp0Pending = "CCP 0 (primary audit) of this lot is not completed yet; lab results are entered and approved only after it.";

        public async Task<HashSet<int>> LotsWaitingForCcp0Async(IEnumerable<int> lotIds)
        {
            var ids = lotIds.Distinct().ToList();
            var done = await _ccp.DonePredicateAsync();
            var assignments = await _context.RawHoneyCCPAssignments.AsNoTracking()
                .Where(a => ids.Contains(a.RawHoneyProductionId) && a.CCP!.Code == Ccp0Code)
                .Select(a => new { a.RawHoneyProductionId, a.RawHoneyCCPAssignmentStatus, a.ApprovedAt, a.RawHoneyCCPAssignmentCCP })
                .ToListAsync();
            return assignments.Where(a => !done(a.RawHoneyCCPAssignmentStatus, a.ApprovedAt, a.RawHoneyCCPAssignmentCCP))
                .Select(a => a.RawHoneyProductionId).ToHashSet();
        }

        public async Task<string?> Ccp0PendingAsync(int lotId) =>
            (await LotsWaitingForCcp0Async(new[] { lotId })).Count > 0 ? Ccp0Pending : null;

        // Why the active membership may not act for a lab (null = allowed)
        public async Task<string?> LabDeniedAsync(int labOrgId, IEnumerable<string> roles)
        {
            if (_current.IsPlatformAdmin)
                return "Platform administrators are read-only on business data; request a correction instead.";
            var active = await _current.GetAsync();
            if (active == null || active.OrgId != labOrgId)
                return "Only the laboratory the sample was sent to can do this.";
            if (!active.IsVerified)
                return $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            if (!active.Has(Capabilities.Laboratory))
                return "Only a Laboratory organization can do this.";
            if (!roles.Contains(active.OrgRole))
                return $"Your role ({active.OrgRole}) cannot do this; roles: {string.Join(", ", roles)}.";
            return null;
        }

        public static string NewReportCode()
        {
            // 12 characters without look-alikes (no 0/O, 1/I/L): about 60 bits, not guessable
            const string alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
            var bytes = RandomNumberGenerator.GetBytes(12);
            return new string(bytes.Select(b => alphabet[b % alphabet.Length]).ToArray());
        }

        public static async Task<string> Sha256OfFileAsync(string path)
        {
            await using var stream = File.OpenRead(path);
            return Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLowerInvariant();
        }

        // Accreditation of the lab at approval time (Laboratory capability, accredited, not expired)
        public async Task<(bool Accredited, string? Number)> AccreditationAsync(int labOrgId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var cap = await _context.OrgCapabilities.AsNoTracking()
                .FirstOrDefaultAsync(c => c.OrgId == labOrgId && c.Capability == Capabilities.Laboratory);
            var ok = cap != null && cap.IsAccredited && (cap.AccreditationValidUntil == null || cap.AccreditationValidUntil >= today);
            return (ok, ok ? cap!.AccreditationNumber : null);
        }

        // Approved results become the lot's CCP values (immutable: LabReportId is set, D-42). The CCP is
        // Completed and approved by the LabManager when every value of it comes from approved reports;
        // when the producer recorded other values of the CCP, it is Submitted for the producer's approval.
        // D-78 P3: values = the lot's values when they do not all come from this report (worst of the containers, D-82)
        public record LabValue(decimal? Number, string? Text, int ReportId, string? Note);

        public async Task ApplyApprovedReportAsync(LabReport report, Sample sample, string approverUserId, IReadOnlyDictionary<int, LabValue>? values = null)
        {
            // D-98: the final honey result of a batch stays with the batch (shown as "final honey result"); no lot values
            if (sample.FinalHoneyProductionId != null)
                return;
            var ccpId = await LabCcpIdAsync() ?? throw new InvalidOperationException("CCP 1 is missing.");
            var lotId = sample.RawHoneyProductionId;
            var parameters = (await LabParametersAsync(ccpId)).ToDictionary(p => p.ParameterId);
            var logs = await _context.CriticalControlPointMonitoringLogs
                .Where(l => l.RawHoneyProductionId == lotId && l.CCPId == ccpId).ToListAsync();
            values ??= report.Results.ToDictionary(r => r.ParameterId, r => new LabValue(r.MeasuredNumericValue, r.MeasuredTextValue, report.ReportId, null));
            foreach (var (parameterId, v) in values)
            {
                var log = logs.FirstOrDefault(l => l.ParameterId == parameterId);
                if (log == null)
                {
                    log = new CriticalControlPointMonitoringLog { RawHoneyProductionId = lotId, CCPId = ccpId, ParameterId = parameterId };
                    _context.CriticalControlPointMonitoringLogs.Add(log);
                    logs.Add(log);
                }
                log.MeasuredNumericValue = v.Number;
                log.MeasuredTextValue = v.Text;
                log.MeasurementDate = report.IssuedOn?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
                log.RecordedByUserId = report.EnteredByUserId;
                log.RecordedByOrgId = report.LabOrgId;
                log.LabReportId = v.ReportId;
                log.StoredFileName = null;
                log.Notes = v.Note ?? $"Lab report {report.ReportCode}";
            }

            // range flags of all values of the CCP
            var siblings = logs.Where(l => l.ParameterId != null && l.MeasuredNumericValue != null)
                .GroupBy(l => l.ParameterId!.Value).ToDictionary(g => g.Key, g => g.First().MeasuredNumericValue!.Value);
            var honey = await _ccp.GetHoneyProfileForRawLotAsync(lotId);
            foreach (var log in logs)
            {
                if (log.ParameterId == null || !parameters.TryGetValue(log.ParameterId.Value, out var p))
                    continue;
                var result = CcpLimitEvaluator.Evaluate(p, log.MeasuredNumericValue, siblings, honey);
                log.IsInRange = result.IsInRange;
                log.AppliedLimit = result.AppliedLimit;
            }

            var assignment = await _context.RawHoneyCCPAssignments
                .FirstOrDefaultAsync(a => a.RawHoneyProductionId == lotId && a.RawHoneyCCPAssignmentCCP == ccpId);
            if (assignment == null)
            {
                assignment = new RawHoneyCCPAssignment
                {
                    RawHoneyProductionId = lotId,
                    RawHoneyCCPAssignmentCCP = ccpId,
                    RawHoneyCCPAssignmentStartDate = DateTime.Now
                };
                _context.RawHoneyCCPAssignments.Add(assignment);
            }
            var allParameters = await _context.CriticalControlPointParameters.AsNoTracking().Where(p => p.CCPId == ccpId).ToListAsync();
            var filled = logs.Where(l => l.ParameterId != null).Select(l => l.ParameterId!.Value).ToHashSet();
            var answers = CcpRequirement.Answers(logs);
            var complete = allParameters.Where(p => CcpRequirement.IsRequired(p, answers)).All(p => filled.Contains(p.ParameterId)) && filled.Count > 0;
            var allFromLab = logs.All(l => l.LabReportId != null);
            assignment.ApprovedByUserId = null;
            assignment.ApprovedByOrgId = null;
            assignment.ApprovedAt = null;
            assignment.SelfApproved = false;
            if (complete && allFromLab)
            {
                assignment.RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed;
                assignment.ApprovedByUserId = approverUserId;
                assignment.ApprovedByOrgId = report.LabOrgId;
                assignment.ApprovedAt = DateTime.Now;
            }
            else
                assignment.RawHoneyCCPAssignmentStatus = complete ? AssignmentStatusIds.Submitted : AssignmentStatusIds.InProgress;
            assignment.RawHoneyCCPAssignmentEndDate = complete ? DateTime.Now : null;
        }
    }
}
