using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-88: an organization that processes, packs or trades honey gets its legacy wholesaler record (the old batch tables
    // still need one) automatically, when it is created or gains one of these capabilities, and at the latest when it
    // creates its first batch. A record linked by the platform administrator stays as it is.
    public static class LegacyWholesalers
    {
        public static readonly string[] Capabilities =
            { Models.Entities.Capabilities.Processing, Models.Entities.Capabilities.Packing, Models.Entities.Capabilities.Wholesale };

        public static async Task<int?> EnsureAsync(TOP4HoneyChainsContext db, int orgId)
        {
            var org = await db.Organizations.Include(o => o.Capabilities).FirstOrDefaultAsync(o => o.OrgId == orgId);
            if (org == null || org.IsPlatform)
                return null;
            if (org.LegacyWholesalerId != null || !org.Capabilities.Any(c => Capabilities.Contains(c.Capability)))
                return org.LegacyWholesalerId;
            var title = org.DisplayName.Trim();
            var record = new Wholesaler
            {
                WholesalerTitle = title.Length > 200 ? title[..200] : title,
                WholesalerTaxNo = org.TaxNo,
                WholesalerIsActive = true,
                CreatedAt = DateTime.Now
            };
            db.Wholesalers.Add(record);
            await db.SaveChangesAsync();
            org.LegacyWholesalerId = record.WholesalerId;
            await db.SaveChangesAsync();
            return record.WholesalerId;
        }
    }

    // D-88: "New batch" of a processing or packing organization. Eligible are the organization's own lots (owner, or the
    // legacy wholesaler link while no owner is set) whose CCPs are all done (completed or "Not applicable", and approved where
    // the matrix requires it) and that still have honey left. The batch belongs to the organization; every CCP that applies
    // to batches and has a row in the CCP responsibility matrix is assigned at once.
    public class PlantBatchService
    {
        public record Lot(int LotId, int? HoneyTypeId, string? HoneyType, string? Apiary, DateOnly? Date, decimal Amount, decimal Used)
        {
            public decimal Left => Amount - Used;
        }

        public record Created(int BatchId, int CcpCount);

        public const int TitleMax = 150;

        private readonly TOP4HoneyChainsContext _context;
        private readonly CcpIntegrityService _ccp;

        public PlantBatchService(TOP4HoneyChainsContext context, CcpIntegrityService ccp)
        {
            _context = context;
            _ccp = ccp;
        }

        // Why the active membership may not create batches; null when it may
        public static string? Denied(ActiveMembership? active, Permissions perm)
        {
            if (active == null)
                return "No active membership.";
            if (!active.IsVerified)
                return $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            if (!active.Has(Models.Entities.Capabilities.Processing) && !active.Has(Models.Entities.Capabilities.Packing))
                return "Only a processing or packing organization creates batches.";
            if (!perm.Allows(PermissionAreas.Batch, PermissionAreas.Enter, active.OrgRole))
                return $"Batches are created by {string.Join(" or ", perm.Roles(PermissionAreas.Batch, PermissionAreas.Enter))} of the organization.";
            return null;
        }

        public async Task<List<Lot>> EligibleLotsAsync(int orgId)
        {
            var legacy = await _context.Organizations.Where(o => o.OrgId == orgId).Select(o => o.LegacyWholesalerId).FirstOrDefaultAsync();
            // the batch takes the organization's wholesaler record, so a lot filed under another record cannot join it
            var own = _context.RawHoneyProductions.Where(r =>
                (r.OwnerOrgId == orgId || (r.OwnerOrgId == null && legacy != null && r.RawHoneyProductionWholesalerId == legacy))
                && (r.RawHoneyProductionWholesalerId == null || r.RawHoneyProductionWholesalerId == legacy));
            var rows = await (await _ccp.EligibleRawLotsAsync(own))
                .Select(r => new
                {
                    r.RawHoneyProductionId, r.RawHoneyProductionHoneyType, HoneyType = r.RawHoneyProductionHoneyTypeNavigation!.HoneyTypeTitle,
                    Apiary = r.RawHoneyProductionApiary!.ApiaryTitle, r.RawHoneyProductionDate, Amount = r.RawHoneyProductionAmount ?? 0,
                    Used = r.FinalHoneyProductionDetails.Sum(d => (decimal?)d.UsedAmount) ?? 0
                })
                .ToListAsync();
            return rows.Select(r => new Lot(r.RawHoneyProductionId, r.RawHoneyProductionHoneyType, r.HoneyType, r.Apiary, r.RawHoneyProductionDate, r.Amount, r.Used))
                .Where(l => l.Left > 0)
                .OrderBy(l => l.Date).ThenBy(l => l.LotId)
                .ToList();
        }

        // Every problem of the request, in English (the views translate them); empty when the batch can be created
        public static List<string> Check(string? title, DateOnly? date, IReadOnlyDictionary<int, decimal> amounts, IReadOnlyList<Lot> eligible, DateOnly today)
        {
            var problems = new List<string>();
            if (string.IsNullOrWhiteSpace(title))
                problems.Add("The title is required.");
            else if (title.Trim().Length > TitleMax)
                problems.Add($"The title can be at most {TitleMax} characters.");
            if (date == null)
                problems.Add("Enter the production date.");
            else if (date > today)
                problems.Add("The production date cannot be in the future.");
            if (amounts.Count == 0)
                problems.Add("Choose at least one lot and the kilograms to use.");
            foreach (var (lotId, kg) in amounts.OrderBy(a => a.Key))
            {
                var lot = eligible.FirstOrDefault(l => l.LotId == lotId);
                if (lot == null)
                    problems.Add($"Lot #{lotId} cannot be used: it is not your organization's, or not all its CCPs are completed and approved, or nothing is left.");
                else if (kg <= 0)
                    problems.Add($"Lot #{lotId}: enter the kilograms to use.");
                else if (kg > lot.Left)
                    problems.Add($"Lot #{lotId}: at most {lot.Left:0.##} kg is available.");
            }
            return problems;
        }

        public async Task<Created> CreateAsync(int orgId, string title, DateOnly date, IReadOnlyDictionary<int, decimal> amounts)
        {
            // a processing or packing organization always gets one (Denied checked the capability)
            var legacy = await LegacyWholesalers.EnsureAsync(_context, orgId)
                         ?? throw new InvalidOperationException("The organization has no legacy wholesaler record.");
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var batch = new FinalHoneyProduction
            {
                FinalHoneyProductionTitle = title.Trim(),
                FinalHoneyProductionDate = date.ToDateTime(TimeOnly.MinValue),
                FinalHoneyProductionQR = Guid.NewGuid(),
                FinalHoneyProductionWholesalerId = legacy,
                FinalHoneyProductionTotalAmount = amounts.Values.Sum(),
                OwnerOrgId = orgId
            };
            _context.FinalHoneyProductions.Add(batch);
            await _context.SaveChangesAsync();

            foreach (var (lotId, kg) in amounts.OrderBy(a => a.Key))
                _context.FinalHoneyProductionDetails.Add(new FinalHoneyProductionDetail
                {
                    FinalHoneyProductionId = batch.FinalHoneyProductionId, RawHoneyProductionId = lotId, UsedAmount = kg
                });

            var ccps = await _context.CriticalControlPoints
                .Where(c => c.AppliesTo == CcpAppliesTo.Batch && _context.CcpResponsibilities.Any(r => r.CcpId == c.Id))
                .OrderBy(c => c.Id).Select(c => c.Id).ToListAsync();
            foreach (var ccpId in ccps)
                _context.FinalHoneyCCPAssignments.Add(new FinalHoneyCCPAssignment
                {
                    FinalHoneyProductionId = batch.FinalHoneyProductionId,
                    FinalHoneyCCPAssignmentCCP = ccpId,
                    FinalHoneyCCPAssignmentStatus = AssignmentStatusIds.Assigned,
                    FinalHoneyCCPAssignmentStartDate = DateTime.Now
                });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new Created(batch.FinalHoneyProductionId, ccps.Count);
        }
    }
}
