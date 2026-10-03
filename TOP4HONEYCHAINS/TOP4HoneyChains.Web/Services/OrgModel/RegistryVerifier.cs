using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    public record RegistryCheckRequest(string BeekeeperId, string EnterpriseNumber, string RequestedByUserId,
        string? OriginalFileName, string? StoredFileName, string? Sha256);

    public record RegistryCheckResult(string Status, int? RequestId, string? Message);

    // National beekeeping registry (AKS) verification (D-29). Manual now; an AKS implementation can
    // replace ManualRegistryVerifier in Program.cs without changes elsewhere.
    public interface IRegistryVerifier
    {
        string Method { get; }

        // "TR" + 2-digit province code (01-81) + 10 digits
        bool IsWellFormed(string enterpriseNumber);

        Task<RegistryCheckResult> VerifyAsync(RegistryCheckRequest request, CancellationToken ct = default);
    }

    public static class RegistryNumber
    {
        private static readonly Regex Format = new(@"^TR(0[1-9]|[1-7][0-9]|8[01])\d{10}$", RegexOptions.CultureInvariant);

        public static string Normalize(string? value) => (value ?? "").Trim().Replace(" ", "").ToUpperInvariant();

        public static bool IsWellFormed(string value) => Format.IsMatch(Normalize(value));
    }

    // Stores the request with its proof document as Pending; a platform administrator decides later
    public class ManualRegistryVerifier : IRegistryVerifier
    {
        private readonly TOP4HoneyChainsContext _context;

        public ManualRegistryVerifier(TOP4HoneyChainsContext context) => _context = context;

        public string Method => "Manual";

        public bool IsWellFormed(string enterpriseNumber) => RegistryNumber.IsWellFormed(enterpriseNumber);

        public async Task<RegistryCheckResult> VerifyAsync(RegistryCheckRequest request, CancellationToken ct = default)
        {
            var number = RegistryNumber.Normalize(request.EnterpriseNumber);
            if (!IsWellFormed(number))
                return new RegistryCheckResult(RegistryStatus.Rejected, null, "The number must be TR, a province code 01-81 and 10 digits (e.g. TR061234567890).");
            if (request.StoredFileName == null)
                return new RegistryCheckResult(RegistryStatus.Rejected, null, "A proof document is required for manual verification.");

            var verification = new RegistryVerification
            {
                BeekeeperId = request.BeekeeperId,
                EnterpriseNumber = number,
                Method = Method,
                Status = RegistryStatus.Pending,
                OriginalFileName = request.OriginalFileName,
                StoredFileName = request.StoredFileName,
                Sha256 = request.Sha256,
                RequestedByUserId = request.RequestedByUserId
            };
            _context.RegistryVerifications.Add(verification);

            var registry = await _context.BeekeeperRegistry.FindAsync(new object[] { request.BeekeeperId }, ct);
            if (registry == null)
            {
                registry = new BeekeeperRegistry { BeekeeperId = request.BeekeeperId };
                _context.BeekeeperRegistry.Add(registry);
            }
            registry.RegistryNumber = number;
            registry.Status = RegistryStatus.Pending;
            registry.VerifiedAt = null;
            registry.VerifiedByUserId = null;

            await _context.SaveChangesAsync(ct);
            return new RegistryCheckResult(RegistryStatus.Pending, verification.Id, "Submitted; waiting for a platform administrator.");
        }

        // The manual decision (approve or reject) on a pending request
        public async Task<string?> DecideAsync(int requestId, bool approve, string? reason, string userId)
        {
            var request = await _context.RegistryVerifications.FirstOrDefaultAsync(v => v.Id == requestId);
            if (request == null || request.Status != RegistryStatus.Pending)
                return "The request is not pending.";
            if (!approve && string.IsNullOrWhiteSpace(reason))
                return "A reason is required to reject.";

            request.Status = approve ? RegistryStatus.Verified : RegistryStatus.Rejected;
            request.DecidedByUserId = userId;
            request.DecidedAt = DateTime.UtcNow;
            request.Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

            var registry = await _context.BeekeeperRegistry.FindAsync(request.BeekeeperId);
            if (registry != null && registry.RegistryNumber == request.EnterpriseNumber)
            {
                registry.Status = request.Status;
                registry.VerifiedAt = approve ? DateTime.UtcNow : null;
                registry.VerifiedByUserId = approve ? userId : null;
            }
            await _context.SaveChangesAsync();
            return null;
        }
    }
}
