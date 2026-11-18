using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // "İşlerim" (D-44): what is waiting for the signed-in user in the active membership, computed from the
    // same rules that allow the action (so the list never offers something the user cannot do)
    public class MyWorkService
    {
        // TitleKind: a record kind ("RawLot", "Batch") the view translates and adds to the title;
        // Waiting: why the item cannot be done yet (a message the view translates), e.g. CCP 0 before the lab test
        public record Item(string Kind, string Title, string? Detail, string Url, DateTime? Since, string? TitleKind = null, string? Waiting = null);

        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly OrgScope _scope;
        private readonly CcpResponsibilityService _resp;
        private readonly CustodyService _custody;
        private readonly IConfiguration _config;
        private readonly Permissions _perm;
        private readonly LabService _lab;
        private readonly ComplianceService _compliance;
        private readonly PlantBatchService _plant;

        public MyWorkService(TOP4HoneyChainsContext context, ICurrentOrg current, OrgScope scope, CcpResponsibilityService resp,
            CustodyService custody, IConfiguration config, Permissions perm, LabService lab, ComplianceService compliance, PlantBatchService plant)
        {
            _plant = plant;
            _lab = lab;
            _compliance = compliance;
            _perm = perm;
            _context = context;
            _current = current;
            _scope = scope;
            _resp = resp;
            _custody = custody;
            _config = config;
        }

        public async Task<List<Item>> ItemsAsync()
        {
            var items = new List<Item>();
            if (_current.IsPlatformAdmin)
            {
                await AdminItemsAsync(items);
                return items;
            }
            var active = await _current.GetAsync();
            if (active == null || !active.IsVerified)
                return items;

            await CcpItemsAsync(active, items);
            await LabItemsAsync(active, items);
            await CustodyItemsAsync(active, items);
            // D-78: signed-photo registrations of the organization, for its Compliance Officer (never the officer who recorded them)
            if (_perm.Allows(PermissionAreas.Evidence, PermissionAreas.Approve, active.OrgRole))
                await EvidenceItemsAsync(active.OrgId, items);
            // D-78 P3 (4.8): sampling-quality items without a review and open data-subject requests, for the Compliance Officer
            if (active.OrgRole == OrgRoles.ComplianceOfficer)
                await ComplianceItemsAsync(active.OrgId, items);
            // D-87: consumer feedback not yet answered or closed
            if (MemberMenu.ConsumerFeedback(active, _perm))
                await FeedbackItemsAsync(f => f.OrgId == active.OrgId && (f.Status == ConsumerFeedbackStatus.New || f.Status == ConsumerFeedbackStatus.Read), items);
            if (OrganizationAuthorizationHandler.IsCentralVerifier(_config, active))
                foreach (var o in await PendingOrgsAsync(o => o.Capabilities.Any(c => c.Capability == Capabilities.Association)))
                    items.Add(new Item("Verify", o.DisplayName, null, $"/Organizations/Details/{o.OrgId}", o.CreatedAt));
            return items;
        }

        // Values to approve and to enter on the lots and batches the organization works on. D-75: a record waits for
        // approval when it is Submitted, or Completed / "Not applicable" without an approval while its CCP requires one;
        // an approval is offered only when the four-eyes rule would allow it (not to whoever recorded or marked it).
        private async Task CcpItemsAsync(ActiveMembership active, List<Item> items)
        {
            // D-88: the CCPs of a control sheet (lot CCP 2–3, batch CCP 4–10) are one item per lot or batch and action, opening the sheet
            var sheets = new Dictionary<(string Action, string Kind, int Id), (string Label, List<string?> Codes, DateTime? Since)>();
            void Add(string action, string kind, int id, string label, int ccpId, string? code, DateTime? since)
            {
                if (!ControlSheets.OnSheet(kind, code))
                {
                    var url = kind == "raw"
                        ? $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={id}&ccpId={ccpId}"
                        : $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={id}&ccpId={ccpId}";
                    items.Add(new Item(action, $"{code} · {label}", kind == "raw" ? "lot" : "batch", url, since));
                    return;
                }
                var key = (action, kind, id);
                if (!sheets.TryGetValue(key, out var sheet))
                    sheets[key] = sheet = (label, new List<string?>(), since);
                sheet.Codes.Add(code);
                if (since != null && (sheet.Since == null || since < sheet.Since))
                    sheets[key] = (sheet.Label, sheet.Codes, since);
            }

            var lots = await (await _scope.VisibleLotsAsync())
                .SelectMany(r => r.RawHoneyCCPAssignments.Select(a => new { r.RawHoneyProductionId, a.RawHoneyCCPAssignmentCCP, a.RawHoneyCCPAssignmentStatus, a.ApprovedAt, a.CCP!.Code, a.RawHoneyCCPAssignmentStartDate }))
                .Where(a => (a.RawHoneyCCPAssignmentStatus != AssignmentStatusIds.Completed && a.RawHoneyCCPAssignmentStatus != AssignmentStatusIds.NotApplicable) || a.ApprovedAt == null)
                .Take(400).ToListAsync();
            foreach (var a in lots.OrderBy(a => a.RawHoneyCCPAssignmentCCP))
            {
                var status = a.RawHoneyCCPAssignmentStatus;
                var done = status is AssignmentStatusIds.Completed or AssignmentStatusIds.NotApplicable;
                if (done && !await _resp.RequiresApprovalAsync(a.RawHoneyCCPAssignmentCCP))
                    continue;
                var access = await _resp.AccessAsync("raw", a.RawHoneyProductionId, a.RawHoneyCCPAssignmentCCP);
                if (status == AssignmentStatusIds.Submitted || done)
                {
                    if (access.Approve && await MayApproveAsync(active, "raw", a.RawHoneyProductionId, a.RawHoneyCCPAssignmentCCP))
                        Add("Approve", "raw", a.RawHoneyProductionId, $"#{a.RawHoneyProductionId}", a.RawHoneyCCPAssignmentCCP, a.Code, a.RawHoneyCCPAssignmentStartDate);
                }
                else if (access.Enter)
                    Add("Enter", "raw", a.RawHoneyProductionId, $"#{a.RawHoneyProductionId}", a.RawHoneyCCPAssignmentCCP, a.Code, a.RawHoneyCCPAssignmentStartDate);
            }
            var batches = await (await _scope.VisibleBatchesAsync())
                .Where(f => !f.FinalHoneyProductionIsFinalized)
                .SelectMany(f => f.FinalHoneyCCPAssignments.Select(a => new { f.FinalHoneyProductionId, f.FinalHoneyProductionTitle, a.FinalHoneyCCPAssignmentCCP, a.FinalHoneyCCPAssignmentStatus, a.ApprovedAt, a.CCP!.Code, a.FinalHoneyCCPAssignmentStartDate }))
                .Where(a => (a.FinalHoneyCCPAssignmentStatus != AssignmentStatusIds.Completed && a.FinalHoneyCCPAssignmentStatus != AssignmentStatusIds.NotApplicable) || a.ApprovedAt == null)
                .Take(400).ToListAsync();
            foreach (var a in batches.OrderBy(a => a.FinalHoneyCCPAssignmentCCP))
            {
                var status = a.FinalHoneyCCPAssignmentStatus;
                var done = status is AssignmentStatusIds.Completed or AssignmentStatusIds.NotApplicable;
                if (done && !await _resp.RequiresApprovalAsync(a.FinalHoneyCCPAssignmentCCP))
                    continue;
                var access = await _resp.AccessAsync("batch", a.FinalHoneyProductionId, a.FinalHoneyCCPAssignmentCCP);
                if (status == AssignmentStatusIds.Submitted || done)
                {
                    if (access.Approve && await MayApproveAsync(active, "batch", a.FinalHoneyProductionId, a.FinalHoneyCCPAssignmentCCP))
                        Add("Approve", "batch", a.FinalHoneyProductionId, a.FinalHoneyProductionTitle ?? $"#{a.FinalHoneyProductionId}", a.FinalHoneyCCPAssignmentCCP, a.Code, a.FinalHoneyCCPAssignmentStartDate);
                }
                else if (access.Enter)
                    Add("Enter", "batch", a.FinalHoneyProductionId, a.FinalHoneyProductionTitle ?? $"#{a.FinalHoneyProductionId}", a.FinalHoneyCCPAssignmentCCP, a.Code, a.FinalHoneyCCPAssignmentStartDate);
            }
            foreach (var ((action, kind, id), sheet) in sheets)
                items.Add(new Item(action, $"{ControlSheets.Codes(sheet.Codes)} · {sheet.Label}", kind == "raw" ? "lot" : "batch", ControlSheets.Url(kind, id), sheet.Since));

            // Finalize: requests to confirm (QualityApprover) and batches ready to request (Staff, OrgAdmin)
            var own = await (await _scope.OwnedBatchesAsync(active.OrgId)).Where(f => !f.FinalHoneyProductionIsFinalized).ToListAsync();
            foreach (var b in own)
            {
                if (b.FinalizeRequestedByUserId != null && _perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Finalize, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve, active.OrgRole))
                    items.Add(new Item("Finalize", b.FinalHoneyProductionTitle ?? $"#{b.FinalHoneyProductionId}", null, $"/OrgWorkspace/Batch/{b.FinalHoneyProductionId}", b.FinalizeRequestedAt));
            }

            // D-88: lots ready for a batch, for the roles that create batches
            if (PlantBatchService.Denied(active, _perm) == null)
            {
                var ready = await _plant.EligibleLotsAsync(active.OrgId);
                if (ready.Count > 0)
                    items.Add(new Item("NewBatch", string.Join(", ", ready.Take(6).Select(l => "#" + l.LotId)) + (ready.Count > 6 ? " …" : ""),
                        $"{ready.Sum(l => l.Left):0.##} kg", "/OrgWorkspace/NewBatch", null));
            }
        }

        // the four-eyes rule of the approve action (CcpAuthorizationHandler): not the person who recorded or marked the record, unless exempt
        private async Task<bool> MayApproveAsync(ActiveMembership active, string kind, int ownerId, int ccpId) =>
            active.FourEyesExempt || await CcpRecorders.OfAsync(_context, kind, ownerId, ccpId, _current.UserId, _current.UserName) == CcpRecorders.Self.None;

        private async Task LabItemsAsync(ActiveMembership active, List<Item> items)
        {
            if (!active.Has(Capabilities.Laboratory))
                return;
            if (_perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve, active.OrgRole))
                foreach (var s in await _context.Samples.AsNoTracking().Where(s => s.LabOrgId == active.OrgId && s.Status == SampleStatus.Sent).ToListAsync())
                    items.Add(new Item("Receive", s.SampleCode, s.SealNumber, "/Samples/Index", s.TakenAt));
            if (_perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.LabResults, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter, active.OrgRole))
            {
                var samples = await _context.Samples.AsNoTracking().Include(s => s.Report)
                    .Where(s => s.LabOrgId == active.OrgId && s.Status == SampleStatus.Received && (s.Report == null || s.Report.Status == LabReportStatus.Draft)).ToListAsync();
                var waiting = await _lab.LotsWaitingForCcp0Async(samples.Select(s => s.RawHoneyProductionId));
                foreach (var s in samples)
                    items.Add(new Item("Results", s.SampleCode, s.Report?.ReturnReason, $"/LabReports/Edit/{s.SampleId}", s.ReceivedAt,
                        Waiting: waiting.Contains(s.RawHoneyProductionId) ? LabService.Ccp0Pending : null));
            }
            if (_perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.LabResults, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve, active.OrgRole))
            {
                var reports = await _context.LabReports.AsNoTracking().Include(r => r.Sample)
                    .Where(r => r.LabOrgId == active.OrgId && r.Status == LabReportStatus.Submitted).ToListAsync();
                var waiting = await _lab.LotsWaitingForCcp0Async(reports.Select(r => r.Sample!.RawHoneyProductionId));
                foreach (var r in reports)
                    items.Add(new Item("LabApprove", r.Sample!.SampleCode, r.ReportNumber, $"/LabReports/Edit/{r.SampleId}", r.SubmittedAt,
                        Waiting: waiting.Contains(r.Sample.RawHoneyProductionId) ? LabService.Ccp0Pending : null));
            }
        }

        private async Task CustodyItemsAsync(ActiveMembership active, List<Item> items)
        {
            if (_perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Shipments, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve, active.OrgRole))
                foreach (var s in await _context.Shipments.AsNoTracking().Include(s => s.Batch).Include(s => s.FromOrg)
                             .Where(s => s.ToOrgId == active.OrgId && s.Status == ShipmentStatus.Pending).ToListAsync())
                    items.Add(new Item("Confirm", s.Batch?.FinalHoneyProductionTitle ?? $"#{s.FinalHoneyProductionId}", $"{s.Quantity:0.##} kg · {s.FromOrg?.DisplayName}", "/Shipments/Index", s.ShippedAt));
            var storageCcp = await _custody.StorageCcpIdAsync();
            if (storageCcp != null && (await _resp.ConfigsAsync()).TryGetValue(storageCcp.Value, out var cfg) && cfg.ApproveRoles.Contains(active.OrgRole))
                foreach (var r in await _context.HolderStorageRecords.AsNoTracking().Include(r => r.Batch)
                             .Where(r => r.HolderOrgId == active.OrgId && r.Status == AssignmentStatusIds.Submitted
                                         && (active.FourEyesExempt || r.RecordedByUserId != _current.UserId)).ToListAsync())   // D-75: four-eyes
                    items.Add(new Item("Approve", $"CCP 10 · {r.Batch?.FinalHoneyProductionTitle}", "storage", $"/Shipments/Storage?batchId={r.FinalHoneyProductionId}", r.RecordedAt));
        }

        // Platform administrators: verification requests and corrections waiting for a second administrator
        private async Task EvidenceItemsAsync(int orgId, List<Item> items)
        {
            foreach (var r in await _context.BeekeeperRegistrations.AsNoTracking().Include(r => r.Beekeeper)
                         .Where(r => r.RegisteringOrgId == orgId && r.EvidenceStatus == EvidenceStatus.PendingReview && r.OfficerUserId != _current.UserId)
                         .ToListAsync())
                items.Add(new Item("Evidence", $"{r.Beekeeper?.BeekeeperFirstName} {r.Beekeeper?.BeekeeperLastName}", r.RegistryNumber,
                    $"/Registration/Details/{r.RegistrationId}", r.At));
            // D-78 P2: deliveries confirmed by a signed photo
            foreach (var i in await _context.LotIntakes.AsNoTracking()
                         .Where(i => i.ReceivingOrgId == orgId && i.EvidenceStatus == EvidenceStatus.PendingReview && i.ReceivedByUserId != _current.UserId)
                         .ToListAsync())
                items.Add(new Item("Evidence", $"#{i.RawHoneyProductionId}", i.FallbackReason, $"/Intake/Details/{i.IntakeId}", i.ReceivedAt, "Kind.lot"));
        }

        private async Task ComplianceItemsAsync(int orgId, List<Item> items)
        {
            foreach (var q in (await _compliance.QualityItemsAsync(orgId)).Where(q => q.Open && q.RecordedBy != _current.UserId))
                items.Add(new Item("Review", q.Title, null, "/Compliance#quality", q.At));
            foreach (var r in await _context.DataSubjectRequests.AsNoTracking().Where(r => r.OrgId == orgId && r.Status == DataSubjectRequestStatus.Open).ToListAsync())
                items.Add(new Item("Request", r.SubjectName, r.DueOn.ToString("dd.MM.yyyy"), "/Compliance#requests", r.ReceivedOn.ToDateTime(TimeOnly.MinValue)));
        }

        // D-87: one item per feedback; organizations and administrators see the pseudonym only
        private async Task FeedbackItemsAsync(System.Linq.Expressions.Expression<Func<ConsumerFeedback, bool>> filter, List<Item> items)
        {
            foreach (var f in await _context.ConsumerFeedback.AsNoTracking().Where(filter).OrderBy(f => f.CreatedAt)
                         .Select(f => new { f.FeedbackId, f.Pseudonym, f.Category, f.CreatedAt, Batch = f.Batch!.FinalHoneyProductionTitle }).ToListAsync())
                items.Add(new Item("Feedback", $"{f.Pseudonym} · {f.Batch}", null, $"/ConsumerFeedback/Details/{f.FeedbackId}", f.CreatedAt, "Consumer.Category." + f.Category));
        }

        private async Task AdminItemsAsync(List<Item> items)
        {
            // D-87: reports of a suspected fake or opened jar also reach the platform administrator until they are closed
            await FeedbackItemsAsync(f => f.Category == ConsumerFeedbackCategory.SuspectedFake && f.Status != ConsumerFeedbackStatus.Closed, items);
            if (await _context.Organizations.AsNoTracking().Where(o => o.IsPlatform).Select(o => (int?)o.OrgId).FirstOrDefaultAsync() is int platformId)
                await EvidenceItemsAsync(platformId, items);
            // C6 / O-16: with the central-association setting, associations are verified by the central association, not here
            var centralSetting = OrganizationAuthorizationHandler.CentralSetting(_config);
            foreach (var o in await PendingOrgsAsync(o => !centralSetting || !o.Capabilities.Any(c => c.Capability == Capabilities.Association)
                                                          || o.Capabilities.Any(c => c.Capability == Capabilities.CentralAssociation)))
                items.Add(new Item("Verify", o.DisplayName, null, $"/Organizations/Details/{o.OrgId}", o.CreatedAt));
            foreach (var r in await _context.RegistryVerifications.AsNoTracking().Where(v => v.Status == RegistryStatus.Pending).ToListAsync())
                items.Add(new Item("Registry", r.EnterpriseNumber, null, $"/Registry/Edit/{r.BeekeeperId}", r.RequestedAt));
            foreach (var c in await _context.CcpCorrections.AsNoTracking().Include(c => c.Ccp)
                         .Where(c => c.Status == CorrectionStatus.Pending && c.RequestedByUserId != _current.UserId).ToListAsync())
                items.Add(new Item("Correction", $"{c.Ccp?.Code} #{c.OwnerId}", c.Reason, "/CCPMonitoring/Corrections", c.RequestedAt,
                    c.OwnerKind == "raw" ? "Kind.lot" : "Kind.batch"));   // D-75: a translated record kind
            // D-78 P3: a delivery correction requested by another administrator
            foreach (var c in await _context.DeliveryCorrections.AsNoTracking()
                         .Where(c => c.Status == CorrectionStatus.Pending && c.RequestedByUserId != _current.UserId).ToListAsync())
                items.Add(new Item("Correction", $"#{c.RawHoneyProductionId}", c.Reason, $"/Intake/Details/{c.IntakeId}", c.RequestedAt, "Kind.lot"));
        }

        // Pending organizations that uploaded at least one document
        private Task<List<Organization>> PendingOrgsAsync(System.Linq.Expressions.Expression<Func<Organization, bool>> filter) =>
            _context.Organizations.AsNoTracking().Include(o => o.Capabilities)
                .Where(o => !o.IsPlatform && o.Status == OrgStatus.Pending && o.Documents.Any())
                .Where(filter).ToListAsync();
    }

    // Which member menu entries the active membership sees (D-44); the actions check the same rules again
    public static class MemberMenu
    {
        private static bool Any(ActiveMembership a, params string[] capabilities) => capabilities.Any(a.Has);

        // D-56: the capability decides which organizations have a function; the role matrix decides who sees it
        public static bool Intake(ActiveMembership a, Permissions p) =>
            a.Has(Capabilities.Processing) && p.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Intake, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, a.OrgRole);

        public static bool Samples(ActiveMembership a, Permissions p) =>
            Any(a, Capabilities.Processing, Capabilities.Laboratory) && p.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, a.OrgRole);

        public static bool Shipments(ActiveMembership a, Permissions p) =>
            Any(a, Capabilities.Processing, Capabilities.Packing, Capabilities.Wholesale, Capabilities.Retail)
            && p.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Shipments, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, a.OrgRole);

        // D-78: beekeeper registration (district directorates)
        public static bool Registration(ActiveMembership a, Permissions p) =>
            a.Has(Capabilities.BeekeeperRegistration)
            && (p.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Registration, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, a.OrgRole)
                || p.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Registration, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter, a.OrgRole));

        // D-78 P3: the Compliance section (4.8), for the organization's Compliance Officer
        public static bool Compliance(ActiveMembership a) => a.OrgRole == OrgRoles.ComplianceOfficer;

        // D-87: consumer feedback about the organization's batches
        public static bool ConsumerFeedback(ActiveMembership a, Permissions p) =>
            p.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.ConsumerFeedback, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, a.OrgRole);

        public static bool Storefronts(ActiveMembership a, Permissions p) =>
            a.Has(Capabilities.Association) && p.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Storefront, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, a.OrgRole);
    }
}
