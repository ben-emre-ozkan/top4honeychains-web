using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P3 (D78-SPEC 2.5, 4.8, O-9, D-82): the Compliance Officer's duties. The CO supervises and never enters workflow data:
    // reviews of sampling quality (deliveries confirmed by a signed photo, rejected samples, seals found broken), data-subject
    // requests with their 30-day deadline, the approval mechanism (stale approvals, self-approvals) and the organization's audit log.
    public class ComplianceOptions
    {
        // approvals waiting longer than this are flagged
        public int StaleApprovalDays { get; set; } = 7;
        // O-9: organizations verified before the requirement get this many days from GraceStart to name a Compliance Officer
        public DateOnly? GraceStart { get; set; }
        public int GraceDays { get; set; } = 60;
    }

    public class ComplianceService
    {
        public const int ResponseDays = 30;

        private readonly TOP4HoneyChainsContext _context;
        private readonly ComplianceOptions _options;
        private readonly TimeProvider _clock;

        public ComplianceService(TOP4HoneyChainsContext context, IOptionsSnapshot<ComplianceOptions> options, TimeProvider clock)
        {
            _context = context;
            _options = options.Value;
            _clock = clock;
        }

        public ComplianceOptions Options => _options;
        private DateTime Now => _clock.GetUtcNow().UtcDateTime;
        private DateOnly Today => DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);

        // ---------------------------------------------------------------- sampling quality (4.8)

        public record QualityItem(string SubjectType, int SubjectId, string Kind, string Title, string? Detail, string Url, DateTime At, string RecordedBy,
            List<ComplianceReview> Reviews)
        {
            public bool Open => Reviews.Count == 0;
        }

        public async Task<List<QualityItem>> QualityItemsAsync(int orgId)
        {
            var reviews = (await _context.ComplianceReviews.AsNoTracking().Where(r => r.OrgId == orgId).OrderBy(r => r.ReviewId).ToListAsync())
                .GroupBy(r => (r.SubjectType, r.SubjectId)).ToDictionary(g => g.Key, g => g.ToList());
            List<ComplianceReview> Of(string type, int id) => reviews.GetValueOrDefault((type, id)) ?? new();
            var items = new List<QualityItem>();
            foreach (var i in await _context.LotIntakes.AsNoTracking()
                         .Where(i => i.ReceivingOrgId == orgId && i.ConfirmationMethod == ConfirmationMethod.SignedPhoto)
                         .OrderByDescending(i => i.IntakeId).Take(200).ToListAsync())
                items.Add(new QualityItem(ReviewSubject.Intake, i.IntakeId, "PhotoIntake", $"#{i.RawHoneyProductionId}", i.FallbackReason,
                    $"/Intake/Details/{i.IntakeId}", i.ReceivedAt, i.ReceivedByUserId, Of(ReviewSubject.Intake, i.IntakeId)));
            foreach (var s in await _context.Samples.AsNoTracking()
                         .Where(s => s.TakingOrgId == orgId && s.Status == SampleStatus.Rejected)
                         .OrderByDescending(s => s.SampleId).Take(200).ToListAsync())
                items.Add(new QualityItem(ReviewSubject.Sample, s.SampleId, "RejectedSample", s.SampleCode, s.RejectReason,
                    $"/LabReports/Edit/{s.SampleId}", s.ReceivedAt ?? s.TakenAt, s.TakenByUserId, Of(ReviewSubject.Sample, s.SampleId)));
            foreach (var e in await _context.ContainerSealEvents.AsNoTracking().Include(e => e.Container)
                         .Where(e => e.Intact == false && e.Container!.Intake!.ReceivingOrgId == orgId)
                         .OrderByDescending(e => e.EventId).Take(200).ToListAsync())
                items.Add(new QualityItem(ReviewSubject.SealEvent, e.EventId, "BrokenSeal", e.Container!.ContainerCode, e.SealNumber,
                    $"/Intake/Details/{e.Container.IntakeId}", e.At, e.UserId, Of(ReviewSubject.SealEvent, e.EventId)));
            return items.OrderByDescending(i => i.Open).ThenByDescending(i => i.At).ToList();
        }

        public async Task<string?> ReviewAsync(int orgId, string userId, string? subjectType, int subjectId, string? outcome, string? finding, string? action)
        {
            var item = (await QualityItemsAsync(orgId)).FirstOrDefault(i => i.SubjectType == subjectType && i.SubjectId == subjectId);
            if (item == null)
                return "This item is not one of your organization's.";
            if (item.RecordedBy == userId)
                return "Four-eyes: you recorded this item; another person reviews it.";
            if (outcome is not (ReviewOutcome.InOrder or ReviewOutcome.Finding))
                return "Choose whether the item is in order or has a finding.";
            finding = finding?.Trim();
            action = string.IsNullOrWhiteSpace(action) ? null : action.Trim();
            if (string.IsNullOrEmpty(finding))
                return "Describe what you checked and found.";
            if (finding.Length > 1000 || action?.Length > 1000)
                return "The finding and the action can be at most 1000 characters each.";
            if (outcome == ReviewOutcome.Finding && action == null)
                return "A finding needs the action taken or requested.";
            _context.ComplianceReviews.Add(new ComplianceReview
            {
                OrgId = orgId, SubjectType = item.SubjectType, SubjectId = item.SubjectId, Outcome = outcome, Finding = finding, Action = action,
                ReviewedByUserId = userId, ReviewedAt = Now
            });
            await _context.SaveChangesAsync();
            return null;
        }

        // ---------------------------------------------------------------- data-subject requests (KVKK, 30 days)

        public Task<List<DataSubjectRequest>> RequestsAsync(int orgId) =>
            _context.DataSubjectRequests.AsNoTracking().Where(r => r.OrgId == orgId)
                .OrderBy(r => r.Status == DataSubjectRequestStatus.Open ? 0 : 1).ThenBy(r => r.DueOn).ThenByDescending(r => r.RequestId).Take(300).ToListAsync();

        public async Task<string?> RecordRequestAsync(int orgId, string userId, string? subjectName, string? beekeeperNumber, string? kind, string? channel,
            DateOnly? receivedOn, string? details, string? consentPurpose)
        {
            subjectName = subjectName?.Trim();
            channel = channel?.Trim();
            details = details?.Trim();
            if (!DataSubjectRequestKind.All.Contains(kind))
                return "Choose the kind of request.";
            if (string.IsNullOrEmpty(subjectName) || subjectName.Length > 200)
                return "Enter who made the request (at most 200 characters).";
            if (string.IsNullOrEmpty(channel) || channel.Length > 50)
                return "Enter how the request came in (at most 50 characters).";
            if (receivedOn == null || receivedOn > Today)
                return "Enter the date the request was received (not in the future).";
            if (string.IsNullOrEmpty(details) || details.Length > 1000)
                return "Describe the request (at most 1000 characters).";
            Beekeeper? beekeeper = null;
            if (!string.IsNullOrWhiteSpace(beekeeperNumber))
            {
                beekeeper = await _context.Beekeepers.FirstOrDefaultAsync(b => b.BeekeeperBusinessNumber == beekeeperNumber.Trim());
                if (beekeeper == null)
                    return "No beekeeper with this number.";
            }
            var request = new DataSubjectRequest
            {
                OrgId = orgId, BeekeeperId = beekeeper?.BeekeeperId, SubjectName = subjectName, Kind = kind!, Channel = channel, ReceivedOn = receivedOn.Value,
                DueOn = receivedOn.Value.AddDays(ResponseDays), Details = details, RecordedByUserId = userId, RecordedAt = Now
            };
            // a consent withdrawal received outside the portal takes effect at once (D-30)
            if (kind == DataSubjectRequestKind.ConsentWithdrawal)
            {
                if (beekeeper == null || !BeekeeperPortalService.PortalPurposes.Contains(consentPurpose))
                    return "A consent withdrawal needs the beekeeper's number and the consent.";
                var active = await _context.Consents.Where(c => c.BeekeeperId == beekeeper.BeekeeperId && c.Purpose == consentPurpose && c.WithdrawnAt == null).ToListAsync();
                foreach (var c in active)
                {
                    c.WithdrawnAt = Now;
                    c.WithdrawnByUserId = userId;
                    c.WithdrawReason = "Withdrawal received by the Compliance Officer: " + channel;
                }
                request.Status = DataSubjectRequestStatus.Answered;
                request.Outcome = active.Count > 0 ? $"Consent {consentPurpose} withdrawn." : $"No active consent {consentPurpose}.";
                request.ClosedByUserId = userId;
                request.ClosedAt = Now;
            }
            _context.DataSubjectRequests.Add(request);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> CloseRequestAsync(int orgId, string userId, int requestId, string? status, string? outcome)
        {
            var r = await _context.DataSubjectRequests.FirstOrDefaultAsync(x => x.RequestId == requestId && x.OrgId == orgId);
            if (r == null)
                return "The request was not found.";
            if (r.Status != DataSubjectRequestStatus.Open)
                return "This request is already closed.";
            if (status is not (DataSubjectRequestStatus.Answered or DataSubjectRequestStatus.Refused))
                return "Choose answered or refused.";
            outcome = outcome?.Trim();
            if (string.IsNullOrEmpty(outcome) || outcome.Length > 1000)
                return "Describe the answer (at most 1000 characters).";
            r.Status = status;
            r.Outcome = outcome;
            r.ClosedByUserId = userId;
            r.ClosedAt = Now;
            await _context.SaveChangesAsync();
            return null;
        }

        // ---------------------------------------------------------------- approval mechanism

        public record ApprovalFlag(int LotId, string Ccp, DateTime? Since, bool SelfApproved);

        public async Task<List<ApprovalFlag>> ApprovalFlagsAsync(int orgId)
        {
            var stale = DateTime.Now.AddDays(-_options.StaleApprovalDays);
            return await _context.RawHoneyCCPAssignments.AsNoTracking()
                .Where(a => a.RawHoneyProduction!.OwnerOrgId == orgId
                            && ((a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.Submitted && (a.RawHoneyCCPAssignmentEndDate ?? a.RawHoneyCCPAssignmentStartDate) < stale)
                                || (a.SelfApproved && a.ApprovedByOrgId == orgId)))
                .OrderByDescending(a => a.SelfApproved).ThenBy(a => a.RawHoneyCCPAssignmentEndDate)
                .Select(a => new ApprovalFlag(a.RawHoneyProductionId, a.CCP!.Code, a.RawHoneyCCPAssignmentEndDate ?? a.RawHoneyCCPAssignmentStartDate, a.SelfApproved))
                .Take(200).ToListAsync();
        }

        public Task<List<AuditEntry>> AuditAsync(int orgId) =>
            _context.AuditEntries.AsNoTracking().Where(a => a.OrgId == orgId).OrderByDescending(a => a.AuditId).Take(200).ToListAsync();

        // ---------------------------------------------------------------- O-9

        public static bool HasComplianceOfficer(IEnumerable<Membership> members) =>
            members.Any(m => m.OrgRole == OrgRoles.ComplianceOfficer && m.Status == MembershipStatus.Active);

        // the date by which an organization verified before the requirement names a Compliance Officer (null: no grace period set)
        public DateOnly? GraceUntil => _options.GraceStart?.AddDays(_options.GraceDays);
    }
}
