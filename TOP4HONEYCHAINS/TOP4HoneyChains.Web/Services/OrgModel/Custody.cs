using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // Custody of finalized batches (ROLE-MODEL section 6, D-41). The owning organization holds the whole
    // batch; a shipment reserves its quantity at once and moves it to the receiver only on receipt.
    public class CustodyService
    {
        // Who ships and who confirms a receipt
        public static readonly string[] ShippingRoles = { OrgRoles.Staff, OrgRoles.OrgAdmin };

        private readonly TOP4HoneyChainsContext _context;
        private readonly CcpResponsibilityService _resp;

        public CustodyService(TOP4HoneyChainsContext context, CcpResponsibilityService resp)
        {
            _context = context;
            _resp = resp;
        }

        public record Holding(int OrgId, decimal Held, decimal ReservedOut)
        {
            public decimal Available => Held - ReservedOut;
        }

        // Quantity per organization: held = owner's total + received in - received out; pending
        // outgoing shipments are reserved
        public async Task<Dictionary<int, Holding>> HoldingsAsync(int batchId)
        {
            var result = new Dictionary<int, Holding>();
            var batch = await _context.FinalHoneyProductions.AsNoTracking()
                .Where(f => f.FinalHoneyProductionId == batchId)
                .Select(f => new { f.FinalHoneyProductionTotalAmount })
                .FirstOrDefaultAsync();
            if (batch == null)
                return result;
            var owner = await _resp.BatchOwnerAsync(batchId);
            if (owner != null)
                result[owner.Value] = new Holding(owner.Value, batch.FinalHoneyProductionTotalAmount ?? 0, 0);

            var shipments = await _context.Shipments.AsNoTracking()
                .Where(s => s.FinalHoneyProductionId == batchId && s.Status != ShipmentStatus.Rejected).ToListAsync();
            foreach (var s in shipments)
            {
                var sender = result.TryGetValue(s.FromOrgId, out var f) ? f : new Holding(s.FromOrgId, 0, 0);
                result[s.FromOrgId] = s.Status == ShipmentStatus.Received
                    ? sender with { Held = sender.Held - s.Quantity }
                    : sender with { ReservedOut = sender.ReservedOut + s.Quantity };
                if (s.Status == ShipmentStatus.Received)
                {
                    var to = result.TryGetValue(s.ToOrgId, out var t) ? t : new Holding(s.ToOrgId, 0, 0);
                    result[s.ToOrgId] = to with { Held = to.Held + s.Quantity };
                }
            }
            return result;
        }

        public async Task<Holding> HoldingOfAsync(int batchId, int orgId) =>
            (await HoldingsAsync(batchId)).TryGetValue(orgId, out var h) ? h : new Holding(orgId, 0, 0);

        // Batches an organization owns or has received (current or former holder)
        public IQueryable<int> BatchesReceivedBy(int orgId) =>
            _context.Shipments.Where(s => s.ToOrgId == orgId && s.Status == ShipmentStatus.Received)
                .Select(s => s.FinalHoneyProductionId).Distinct();

        public Task<bool> HasReceivedAsync(int batchId, int orgId) =>
            _context.Shipments.AnyAsync(s => s.FinalHoneyProductionId == batchId && s.ToOrgId == orgId && s.Status == ShipmentStatus.Received);

        public Task<bool> TakesPartAsync(int batchId, int orgId) =>
            _context.Shipments.AnyAsync(s => s.FinalHoneyProductionId == batchId && (s.ToOrgId == orgId || s.FromOrgId == orgId));

        public Task<int?> StorageCcpIdAsync() =>
            _context.CriticalControlPoints.Where(c => c.Code == "CCP 10").Select(c => (int?)c.Id).FirstOrDefaultAsync();
    }

    // SMS to a beekeeper after an intake. The interface allows a real provider later; the current
    // implementation only logs that a message would be sent (the number is masked).
    public interface ISmsSender
    {
        Task<string> SendAsync(string phoneNumber, string text);
    }

    public class LoggingSmsSender : ISmsSender
    {
        private readonly ILogger<LoggingSmsSender> _logger;

        public LoggingSmsSender(ILogger<LoggingSmsSender> logger) => _logger = logger;

        public Task<string> SendAsync(string phoneNumber, string text)
        {
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            var masked = digits.Length <= 4 ? "****" : new string('*', digits.Length - 4) + digits[^4..];
            _logger.LogInformation("SMS (not sent, logging only) to {Phone}: {Text}", masked, text);
            return Task.FromResult("Logged");
        }
    }
}
