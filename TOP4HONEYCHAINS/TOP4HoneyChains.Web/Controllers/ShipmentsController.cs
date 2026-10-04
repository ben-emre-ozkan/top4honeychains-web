using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Holdings and shipments of finalized batches (ROLE-MODEL section 6, D-41): the holder ships a
    // quantity to another verified organization, the receiver confirms (custody moves) or rejects with a
    // reason. Later holders record CCP 10 (storage) for the quantity they hold.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class ShipmentsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly CustodyService _custody;
        private readonly CcpResponsibilityService _resp;
        private readonly OrgScope _scope;
        private readonly AuditWriter _audit;
        private readonly CcpIntegrityService _ccp;

        public ShipmentsController(TOP4HoneyChainsContext context, ICurrentOrg current, CustodyService custody,
            CcpResponsibilityService resp, OrgScope scope, AuditWriter audit, CcpIntegrityService ccp)
        {
            _ccp = ccp;
            _context = context;
            _current = current;
            _custody = custody;
            _resp = resp;
            _scope = scope;
            _audit = audit;
        }

        public record HoldingRow(FinalHoneyProduction Batch, bool Owner, CustodyService.Holding Holding, HolderStorageRecord? Storage);

        public async Task<IActionResult> Index()
        {
            ViewBag.Controller = "Shipments";
            ViewBag.Action = "Index";
            if (_current.IsPlatformAdmin)
            {
                // Platform administrators see every shipment, read-only
                ViewBag.All = await _context.Shipments.AsNoTracking().Include(s => s.Batch).Include(s => s.FromOrg).Include(s => s.ToOrg)
                    .OrderByDescending(s => s.ShipmentId).Take(300).ToListAsync();
                return View("All");
            }
            var active = await _current.GetAsync();
            if (active == null)
                return NotFound();

            var owned = await (await _scope.OwnedBatchesAsync(active.OrgId)).Where(f => f.FinalHoneyProductionIsFinalized)
                .Select(f => f.FinalHoneyProductionId).ToListAsync();
            var received = await _custody.BatchesReceivedBy(active.OrgId).ToListAsync();
            var storageCcp = await _custody.StorageCcpIdAsync();
            var rows = new List<HoldingRow>();
            foreach (var id in owned.Union(received).Distinct().OrderByDescending(x => x))
            {
                var batch = await _context.FinalHoneyProductions.AsNoTracking().FirstAsync(f => f.FinalHoneyProductionId == id);
                var storage = await _context.HolderStorageRecords.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.FinalHoneyProductionId == id && r.HolderOrgId == active.OrgId && r.CcpId == storageCcp);
                rows.Add(new HoldingRow(batch, owned.Contains(id), await _custody.HoldingOfAsync(id, active.OrgId), storage));
            }
            ViewBag.Holdings = rows;
            ViewBag.Incoming = await _context.Shipments.AsNoTracking().Include(s => s.Batch).Include(s => s.FromOrg)
                .Where(s => s.ToOrgId == active.OrgId).OrderBy(s => s.Status == ShipmentStatus.Pending ? 0 : 1).ThenByDescending(s => s.ShipmentId).Take(200).ToListAsync();
            ViewBag.Outgoing = await _context.Shipments.AsNoTracking().Include(s => s.Batch).Include(s => s.ToOrg)
                .Where(s => s.FromOrgId == active.OrgId).OrderByDescending(s => s.ShipmentId).Take(200).ToListAsync();
            ViewBag.CanShip = active.IsVerified && CustodyService.ShippingRoles.Contains(active.OrgRole);
            return View();
        }

        private async Task<string?> ShipDeniedAsync(ActiveMembership? active)
        {
            if (_current.IsPlatformAdmin)
                return "Platform administrators are read-only on business data; request a correction instead.";
            if (active == null)
                return "No active membership.";
            if (!active.IsVerified)
                return $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            if (!CustodyService.ShippingRoles.Contains(active.OrgRole))
                return $"Your role ({active.OrgRole}) cannot ship or receive goods; roles: {string.Join(", ", CustodyService.ShippingRoles)}.";
            return null;
        }

        public async Task<IActionResult> Create(int batchId)
        {
            var active = await _current.GetAsync();
            if (await ShipDeniedAsync(active) is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            var batch = await _context.FinalHoneyProductions.AsNoTracking().FirstOrDefaultAsync(f => f.FinalHoneyProductionId == batchId);
            var holding = await _custody.HoldingOfAsync(batchId, active!.OrgId);
            if (batch == null || holding.Held <= 0)
                return NotFound();
            if (CustodyService.DraftBatchError(batch.FinalHoneyProductionIsFinalized) is string draft)
            {
                TempData["Error"] = draft;
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Batch = batch;
            ViewBag.Holding = holding;
            ViewBag.Receivers = await _context.Organizations.AsNoTracking()
                .Where(o => !o.IsPlatform && o.Status == OrgStatus.Verified && o.OrgId != active.OrgId)
                .OrderBy(o => o.DisplayName).ToListAsync();
            ViewBag.Controller = "Shipments";
            ViewBag.Action = "Create";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int batchId, int toOrgId, decimal? quantity, string? note)
        {
            var active = await _current.GetAsync();
            if (await ShipDeniedAsync(active) is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            var batch = await _context.FinalHoneyProductions.AsNoTracking().FirstOrDefaultAsync(f => f.FinalHoneyProductionId == batchId);
            if (batch == null || (await _custody.HoldingOfAsync(batchId, active!.OrgId)).Held <= 0)
                return NotFound();
            note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

            // Serialize shipments of one batch so two parallel requests cannot ship the same quantity twice
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            await _context.FinalHoneyProductions.FromSqlInterpolated(
                $"SELECT * FROM dbo.FinalHoneyProductions WITH (UPDLOCK, HOLDLOCK) WHERE FinalHoneyProductionId = {batchId}").ToListAsync();
            var holding = await _custody.HoldingOfAsync(batchId, active.OrgId);

            var receiverVerified = await _context.Organizations.AnyAsync(o => o.OrgId == toOrgId && !o.IsPlatform && o.Status == OrgStatus.Verified);
            var error = CustodyService.CheckShipment(batch.FinalHoneyProductionIsFinalized, active.OrgId, toOrgId, receiverVerified,
                quantity, holding.Available, note);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Create), new { batchId });
            }

            _context.Shipments.Add(new Shipment
            {
                FinalHoneyProductionId = batchId,
                FromOrgId = active.OrgId,
                ToOrgId = toOrgId,
                Quantity = quantity!.Value,
                Note = note,
                ShippedByUserId = _current.UserId!,
                ShippedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = "Shipment created; custody moves when the receiver confirms it.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decide(int id, bool confirm, string? reason)
        {
            var active = await _current.GetAsync();
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.ShipmentId == id);
            if (shipment == null || active == null || shipment.ToOrgId != active.OrgId)
                return NotFound();
            reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            string? error = await ShipDeniedAsync(active);
            if (error == null && shipment.Status != ShipmentStatus.Pending)
                error = "This shipment was already decided.";
            else if (error == null && !confirm && reason == null)
                error = "A reason is required to reject a shipment.";
            else if (error == null && reason?.Length > 500)
                error = "The reason can be at most 500 characters.";
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }
            shipment.Status = confirm ? ShipmentStatus.Received : ShipmentStatus.Rejected;
            shipment.DecidedByUserId = _current.UserId;
            shipment.DecidedAt = DateTime.UtcNow;
            shipment.RejectReason = confirm ? null : reason;
            await _context.SaveChangesAsync();
            TempData["Success"] = confirm ? "Receipt confirmed; you now hold this quantity." : "Shipment rejected; the quantity stays with the sender.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------- CCP 10 of a later holder

        public record StorageAccess(bool Read, bool Enter, bool Approve, string? EnterDenied, string? ApproveDenied);

        private async Task<StorageAccess> StorageAccessAsync(int batchId, int ccpId)
        {
            if (_current.IsPlatformAdmin)
                return new StorageAccess(true, false, false, "Platform administrators are read-only on business data; request a correction instead.",
                    "Platform administrators do not approve CCP records.");
            var active = await _current.GetAsync();
            if (active == null || !await _custody.HasReceivedAsync(batchId, active.OrgId))
                return new StorageAccess(false, false, false, null, null);
            (await _resp.ConfigsAsync()).TryGetValue(ccpId, out var cfg);
            var held = (await _custody.HoldingOfAsync(batchId, active.OrgId)).Held;
            string? enter = null, approve = null;
            if (held <= 0)
                enter = approve = "You no longer hold any quantity of this batch.";
            else if (!active.IsVerified)
                enter = approve = $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            else if (cfg == null || !cfg.Capabilities.Any(active.Has))
                enter = approve = $"This control point needs one of these capabilities: {string.Join(", ", cfg?.Capabilities ?? new HashSet<string>())}.";
            else
            {
                if (!cfg.EnterRoles.Contains(active.OrgRole))
                    enter = $"Your role ({active.OrgRole}) cannot enter this control point; entering roles: {string.Join(", ", cfg.EnterRoles)}.";
                if (!cfg.ApproveRoles.Contains(active.OrgRole))
                    approve = $"Your role ({active.OrgRole}) cannot approve this control point; approving roles: {string.Join(", ", cfg.ApproveRoles)}.";
            }
            return new StorageAccess(true, enter == null, approve == null, enter, approve);
        }

        private async Task<(HolderStorageRecord? Record, List<CriticalControlPointParameter> Parameters, int CcpId)> LoadStorageAsync(int batchId, int orgId)
        {
            var ccpId = await _custody.StorageCcpIdAsync() ?? 0;
            var record = await _context.HolderStorageRecords.Include(r => r.Values)
                .FirstOrDefaultAsync(r => r.FinalHoneyProductionId == batchId && r.HolderOrgId == orgId && r.CcpId == ccpId);
            var parameters = await _context.CriticalControlPointParameters.AsNoTracking()
                .Where(p => p.CCPId == ccpId && p.DataType != CcpDataType.File)
                .Include(p => p.Limits).OrderBy(p => p.ParameterId).ToListAsync();
            return (record, parameters, ccpId);
        }

        // holderOrgId: the record of which holder (a platform administrator chooses; a member sees its own)
        public async Task<IActionResult> Storage(int batchId, int? holderOrgId)
        {
            var ccpId = await _custody.StorageCcpIdAsync() ?? 0;
            var access = await StorageAccessAsync(batchId, ccpId);
            if (!access.Read)
                return NotFound();
            var active = await _current.GetAsync();
            var orgId = _current.IsPlatformAdmin ? holderOrgId ?? 0 : active!.OrgId;
            var (record, parameters, _) = await LoadStorageAsync(batchId, orgId);
            ViewBag.Batch = await _context.FinalHoneyProductions.AsNoTracking().FirstOrDefaultAsync(f => f.FinalHoneyProductionId == batchId);
            ViewBag.Holder = await _context.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.OrgId == orgId);
            ViewBag.Held = (await _custody.HoldingOfAsync(batchId, orgId)).Held;
            ViewBag.Record = record;
            ViewBag.Parameters = parameters;
            ViewBag.Access = access;
            ViewBag.SelfRecorded = record?.RecordedByUserId == _current.UserId;
            ViewBag.FourEyesExempt = active?.FourEyesExempt == true;
            ViewBag.Controller = "Shipments";
            ViewBag.Action = "Storage";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Storage(int batchId, [Bind(Prefix = "values")] Dictionary<int, string?>? values)
        {
            var ccpId = await _custody.StorageCcpIdAsync() ?? 0;
            var access = await StorageAccessAsync(batchId, ccpId);
            if (!access.Read)
                return NotFound();
            if (!access.Enter)
            {
                TempData["Error"] = access.EnterDenied;
                return RedirectToAction(nameof(Storage), new { batchId });
            }
            var active = (await _current.GetAsync())!;
            var (record, parameters, _) = await LoadStorageAsync(batchId, active.OrgId);

            // Parse and check every value first; nothing is saved when one is invalid
            var parsed = new Dictionary<int, (decimal? Number, string? Text)>();
            foreach (var p in parameters)
            {
                string? raw = null;
                values?.TryGetValue(p.ParameterId, out raw);
                raw = raw?.Trim();
                if (string.IsNullOrEmpty(raw))
                {
                    if (p.IsRequired)
                    {
                        TempData["Error"] = $"{p.ParameterName}: enter a value.";
                        return RedirectToAction(nameof(Storage), new { batchId });
                    }
                    continue;
                }
                if (p.DataType == CcpDataType.Number || p.DataType == null && ParseNumber(raw) != null)
                {
                    if (ParseNumber(raw) is not decimal n)
                    {
                        TempData["Error"] = $"{p.ParameterName}: enter a number.";
                        return RedirectToAction(nameof(Storage), new { batchId });
                    }
                    parsed[p.ParameterId] = (n, null);
                }
                else if (p.DataType == CcpDataType.YesNo && raw is not ("Yes" or "No"))
                {
                    TempData["Error"] = $"{p.ParameterName}: choose Yes or No.";
                    return RedirectToAction(nameof(Storage), new { batchId });
                }
                else if (raw.Length > 100)
                {
                    TempData["Error"] = "The text can be at most 100 characters.";
                    return RedirectToAction(nameof(Storage), new { batchId });
                }
                else
                    parsed[p.ParameterId] = (null, raw);
            }

            if (record == null)
            {
                record = new HolderStorageRecord { FinalHoneyProductionId = batchId, HolderOrgId = active.OrgId, CcpId = ccpId };
                _context.HolderStorageRecords.Add(record);
            }
            var changed = false;
            var numbers = parsed.Where(kv => kv.Value.Number != null).ToDictionary(kv => kv.Key, kv => kv.Value.Number!.Value);
            var honey = await _ccp.GetHoneyProfileForBatchAsync(batchId);
            foreach (var p in parameters)
            {
                var existing = record.Values.FirstOrDefault(v => v.ParameterId == p.ParameterId);
                if (!parsed.TryGetValue(p.ParameterId, out var v))
                {
                    if (existing != null) { _context.HolderStorageValues.Remove(existing); changed = true; }
                    continue;
                }
                if (existing == null)
                {
                    existing = new HolderStorageValue { ParameterId = p.ParameterId };
                    record.Values.Add(existing);
                    changed = true;
                }
                else if (existing.MeasuredNumericValue != v.Number || existing.MeasuredTextValue != v.Text)
                    changed = true;
                existing.MeasuredNumericValue = v.Number;
                existing.MeasuredTextValue = v.Text;
                existing.MeasuredAt = DateTime.UtcNow;
                var result = CcpLimitEvaluator.Evaluate(p, v.Number, numbers, honey);
                existing.IsInRange = result.IsInRange;
                existing.AppliedLimit = result.AppliedLimit;
            }

            var complete = parameters.Where(p => p.IsRequired).All(p => parsed.ContainsKey(p.ParameterId)) && parsed.Count > 0;
            if (changed || record.RecordId == 0)
            {
                record.ApprovedByUserId = null;
                record.ApprovedAt = null;
                record.SelfApproved = false;
                record.RecordedByUserId = _current.UserId;
                record.RecordedAt = DateTime.UtcNow;
            }
            record.HeldQuantity = (await _custody.HoldingOfAsync(batchId, active.OrgId)).Held;
            record.Status = !complete ? AssignmentStatusIds.InProgress
                : record.ApprovedAt != null || !await _resp.RequiresApprovalAsync(ccpId) ? AssignmentStatusIds.Completed
                : AssignmentStatusIds.Submitted;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Storage values saved.";
            return RedirectToAction(nameof(Storage), new { batchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStorage(int batchId)
        {
            var ccpId = await _custody.StorageCcpIdAsync() ?? 0;
            var access = await StorageAccessAsync(batchId, ccpId);
            if (!access.Read)
                return NotFound();
            var active = (await _current.GetAsync())!;
            var (record, _, _) = await LoadStorageAsync(batchId, active.OrgId);
            var self = record?.RecordedByUserId == _current.UserId;
            string? error = null;
            if (!access.Approve)
                error = access.ApproveDenied;
            else if (record == null || record.Status != AssignmentStatusIds.Submitted)
                error = "Only a submitted (or \"Not applicable\") CCP record can be approved.";
            else if (self && !active.FourEyesExempt)
                error = "Four-eyes: you recorded values of this CCP, so another person must approve it.";
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Storage), new { batchId });
            }
            record!.ApprovedByUserId = _current.UserId;
            record.ApprovedAt = DateTime.UtcNow;
            record.SelfApproved = self;
            record.Status = AssignmentStatusIds.Completed;
            await _context.SaveChangesAsync();
            if (self)
                await _audit.WriteAsync("Approve", "HolderStorageRecords", record.RecordId.ToString(), null, null, "SelfApproved");
            TempData["Success"] = self ? "Approved under the four-eyes exemption (flagged in the audit log)." : "CCP record approved.";
            return RedirectToAction(nameof(Storage), new { batchId });
        }

        // "1.5" and "1,5" both mean 1.5 (ENG-07)
        private static decimal? ParseNumber(string raw) =>
            DecimalInput.Parse(raw);
    }
}
