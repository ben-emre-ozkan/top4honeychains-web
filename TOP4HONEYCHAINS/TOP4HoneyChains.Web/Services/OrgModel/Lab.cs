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
                .Include(p => p.Limits)
                .OrderBy(p => p.ParameterId).ToListAsync();

        // D-54: required lab parameters that have no value in the report yet
        public static List<CriticalControlPointParameter> MissingRequired(LabReport? report, IEnumerable<CriticalControlPointParameter> parameters) =>
            parameters.Where(p => p.IsRequired && !(report?.Results.Any(r => r.ParameterId == p.ParameterId
                && (r.MeasuredNumericValue != null || !string.IsNullOrWhiteSpace(r.MeasuredTextValue))) ?? false)).ToList();

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
        public async Task ApplyApprovedReportAsync(LabReport report, Sample sample, string approverUserId)
        {
            var ccpId = await LabCcpIdAsync() ?? throw new InvalidOperationException("CCP 1 is missing.");
            var lotId = sample.RawHoneyProductionId;
            var parameters = (await LabParametersAsync(ccpId)).ToDictionary(p => p.ParameterId);
            var logs = await _context.CriticalControlPointMonitoringLogs
                .Where(l => l.RawHoneyProductionId == lotId && l.CCPId == ccpId).ToListAsync();
            foreach (var r in report.Results)
            {
                var log = logs.FirstOrDefault(l => l.ParameterId == r.ParameterId);
                if (log == null)
                {
                    log = new CriticalControlPointMonitoringLog { RawHoneyProductionId = lotId, CCPId = ccpId, ParameterId = r.ParameterId };
                    _context.CriticalControlPointMonitoringLogs.Add(log);
                    logs.Add(log);
                }
                log.MeasuredNumericValue = r.MeasuredNumericValue;
                log.MeasuredTextValue = r.MeasuredTextValue;
                log.MeasurementDate = report.IssuedOn?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
                log.RecordedByUserId = report.EnteredByUserId;
                log.RecordedByOrgId = report.LabOrgId;
                log.LabReportId = report.ReportId;
                log.StoredFileName = null;
                log.Notes = $"Lab report {report.ReportCode}";
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
            var complete = allParameters.Where(p => p.IsRequired).All(p => filled.Contains(p.ParameterId)) && filled.Count > 0;
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
