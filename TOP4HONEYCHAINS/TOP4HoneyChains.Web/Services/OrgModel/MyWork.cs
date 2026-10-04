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
        public record Item(string Kind, string Title, string? Detail, string Url, DateTime? Since);

        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly OrgScope _scope;
        private readonly CcpResponsibilityService _resp;
        private readonly CustodyService _custody;
        private readonly IConfiguration _config;
        private readonly Permissions _perm;

        public MyWorkService(TOP4HoneyChainsContext context, ICurrentOrg current, OrgScope scope, CcpResponsibilityService resp,
            CustodyService custody, IConfiguration config, Permissions perm)
        {
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
            if (OrganizationAuthorizationHandler.IsCentralVerifier(_config, active))
                foreach (var o in await PendingOrgsAsync(o => o.Capabilities.Any(c => c.Capability == Capabilities.Association)))
                    items.Add(new Item("Verify", o.DisplayName, null, $"/Organizations/Details/{o.OrgId}", o.CreatedAt));
            return items;
        }

        // Values to approve and to enter on the lots and batches the organization works on
        private async Task CcpItemsAsync(ActiveMembership active, List<Item> items)
        {
            var lots = await (await _scope.VisibleLotsAsync())
                .SelectMany(r => r.RawHoneyCCPAssignments.Select(a => new { r.RawHoneyProductionId, a.RawHoneyCCPAssignmentCCP, a.RawHoneyCCPAssignmentStatus, a.CCP!.Code, a.RawHoneyCCPAssignmentStartDate }))
                .Where(a => a.RawHoneyCCPAssignmentStatus != AssignmentStatusIds.Completed && a.RawHoneyCCPAssignmentStatus != AssignmentStatusIds.NotApplicable)
                .Take(400).ToListAsync();
            foreach (var a in lots)
            {
                var access = await _resp.AccessAsync("raw", a.RawHoneyProductionId, a.RawHoneyCCPAssignmentCCP);
                var url = $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={a.RawHoneyProductionId}&ccpId={a.RawHoneyCCPAssignmentCCP}";
                if (a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.Submitted && access.Approve)
                    items.Add(new Item("Approve", $"{a.Code} · #{a.RawHoneyProductionId}", "lot", url, a.RawHoneyCCPAssignmentStartDate));
                else if (a.RawHoneyCCPAssignmentStatus != AssignmentStatusIds.Submitted && access.Enter)
                    items.Add(new Item("Enter", $"{a.Code} · #{a.RawHoneyProductionId}", "lot", url, a.RawHoneyCCPAssignmentStartDate));
            }
            var batches = await (await _scope.VisibleBatchesAsync())
                .Where(f => !f.FinalHoneyProductionIsFinalized)
                .SelectMany(f => f.FinalHoneyCCPAssignments.Select(a => new { f.FinalHoneyProductionId, f.FinalHoneyProductionTitle, a.FinalHoneyCCPAssignmentCCP, a.FinalHoneyCCPAssignmentStatus, a.CCP!.Code, a.FinalHoneyCCPAssignmentStartDate }))
                .Where(a => a.FinalHoneyCCPAssignmentStatus != AssignmentStatusIds.Completed && a.FinalHoneyCCPAssignmentStatus != AssignmentStatusIds.NotApplicable)
                .Take(400).ToListAsync();
            foreach (var a in batches)
            {
                var access = await _resp.AccessAsync("batch", a.FinalHoneyProductionId, a.FinalHoneyCCPAssignmentCCP);
                var url = $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={a.FinalHoneyProductionId}&ccpId={a.FinalHoneyCCPAssignmentCCP}";
                if (a.FinalHoneyCCPAssignmentStatus == AssignmentStatusIds.Submitted && access.Approve)
                    items.Add(new Item("Approve", $"{a.Code} · {a.FinalHoneyProductionTitle}", "batch", url, a.FinalHoneyCCPAssignmentStartDate));
                else if (a.FinalHoneyCCPAssignmentStatus != AssignmentStatusIds.Submitted && access.Enter)
                    items.Add(new Item("Enter", $"{a.Code} · {a.FinalHoneyProductionTitle}", "batch", url, a.FinalHoneyCCPAssignmentStartDate));
            }

            // Finalize: requests to confirm (QualityApprover) and batches ready to request (Staff, OrgAdmin)
            var own = await (await _scope.OwnedBatchesAsync(active.OrgId)).Where(f => !f.FinalHoneyProductionIsFinalized).ToListAsync();
            foreach (var b in own)
            {
                if (b.FinalizeRequestedByUserId != null && _perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Finalize, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve, active.OrgRole))
                    items.Add(new Item("Finalize", b.FinalHoneyProductionTitle ?? $"#{b.FinalHoneyProductionId}", null, $"/OrgWorkspace/Batch/{b.FinalHoneyProductionId}", b.FinalizeRequestedAt));
            }
        }

        private async Task LabItemsAsync(ActiveMembership active, List<Item> items)
        {
            if (!active.Has(Capabilities.Laboratory))
                return;
            if (_perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Samples, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve, active.OrgRole))
                foreach (var s in await _context.Samples.AsNoTracking().Where(s => s.LabOrgId == active.OrgId && s.Status == SampleStatus.Sent).ToListAsync())
                    items.Add(new Item("Receive", s.SampleCode, s.SealNumber, "/Samples/Index", s.TakenAt));
            if (_perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.LabResults, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter, active.OrgRole))
                foreach (var s in await _context.Samples.AsNoTracking().Include(s => s.Report)
                             .Where(s => s.LabOrgId == active.OrgId && s.Status == SampleStatus.Received && (s.Report == null || s.Report.Status == LabReportStatus.Draft)).ToListAsync())
                    items.Add(new Item("Results", s.SampleCode, s.Report?.ReturnReason, $"/LabReports/Edit/{s.SampleId}", s.ReceivedAt));
            if (_perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.LabResults, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve, active.OrgRole))
                foreach (var r in await _context.LabReports.AsNoTracking().Include(r => r.Sample)
                             .Where(r => r.LabOrgId == active.OrgId && r.Status == LabReportStatus.Submitted).ToListAsync())
                    items.Add(new Item("LabApprove", r.Sample!.SampleCode, r.ReportNumber, $"/LabReports/Edit/{r.SampleId}", r.SubmittedAt));
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
                             .Where(r => r.HolderOrgId == active.OrgId && r.Status == AssignmentStatusIds.Submitted).ToListAsync())
                    items.Add(new Item("Approve", $"CCP 10 · {r.Batch?.FinalHoneyProductionTitle}", "storage", $"/Shipments/Storage?batchId={r.FinalHoneyProductionId}", r.RecordedAt));
        }

        // Platform administrators: verification requests and corrections waiting for a second administrator
        private async Task AdminItemsAsync(List<Item> items)
        {
            foreach (var o in await PendingOrgsAsync(o => true))
                items.Add(new Item("Verify", o.DisplayName, null, $"/Organizations/Details/{o.OrgId}", o.CreatedAt));
            foreach (var r in await _context.RegistryVerifications.AsNoTracking().Where(v => v.Status == RegistryStatus.Pending).ToListAsync())
                items.Add(new Item("Registry", r.EnterpriseNumber, null, $"/Registry/Edit/{r.BeekeeperId}", r.RequestedAt));
            foreach (var c in await _context.CcpCorrections.AsNoTracking().Include(c => c.Ccp)
                         .Where(c => c.Status == CorrectionStatus.Pending && c.RequestedByUserId != _current.UserId).ToListAsync())
                items.Add(new Item("Correction", $"{c.Ccp?.Code} · {c.OwnerKind} #{c.OwnerId}", c.Reason, "/CCPMonitoring/Corrections", c.RequestedAt));
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

        public static bool Storefronts(ActiveMembership a, Permissions p) =>
            a.Has(Capabilities.Association) && p.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Storefront, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, a.OrgRole);
    }
}
