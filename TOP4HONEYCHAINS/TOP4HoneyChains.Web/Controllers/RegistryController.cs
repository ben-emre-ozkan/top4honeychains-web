using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Beekeepers as records: association link and registry (AKS) verification (D-23, D-29).
    // P1: platform administrators only; associations submit in a later phase.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class RegistryController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly IRegistryVerifier _verifier;
        private readonly ManualRegistryVerifier _manual;
        private readonly ICurrentOrg _current;
        private readonly AuditWriter _audit;
        private readonly IWebHostEnvironment _env;

        public RegistryController(TOP4HoneyChainsContext context, IRegistryVerifier verifier, ManualRegistryVerifier manual,
            ICurrentOrg current, AuditWriter audit, IWebHostEnvironment env)
        {
            _context = context;
            _verifier = verifier;
            _manual = manual;
            _current = current;
            _audit = audit;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var rows = await _context.Beekeepers.AsNoTracking()
                .OrderBy(b => b.BeekeeperBusinessNumber)
                .Select(b => new RegistryRow(
                    b.BeekeeperId!,
                    b.BeekeeperBusinessNumber,
                    b.BeekeeperFirstName + " " + b.BeekeeperLastName,
                    _context.BeekeeperRegistry.Where(r => r.BeekeeperId == b.BeekeeperId).Select(r => r.RegistryNumber).FirstOrDefault(),
                    _context.BeekeeperRegistry.Where(r => r.BeekeeperId == b.BeekeeperId).Select(r => r.Status).FirstOrDefault() ?? RegistryStatus.Unverified,
                    _context.BeekeeperAssociations.Where(a => a.BeekeeperId == b.BeekeeperId && a.ValidTo == null).Select(a => a.AssociationOrg!.DisplayName).FirstOrDefault()))
                .ToListAsync();
            ViewBag.Controller = "Registry";
            ViewBag.Action = "Index";
            return View(rows);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var beekeeper = await _context.Beekeepers.AsNoTracking().FirstOrDefaultAsync(b => b.BeekeeperId == id);
            if (beekeeper == null)
                return NotFound();
            ViewBag.Beekeeper = beekeeper;
            ViewBag.Registry = await _context.BeekeeperRegistry.AsNoTracking().FirstOrDefaultAsync(r => r.BeekeeperId == id);
            ViewBag.Requests = await _context.RegistryVerifications.AsNoTracking().Where(v => v.BeekeeperId == id).OrderByDescending(v => v.Id).ToListAsync();
            ViewBag.Link = await _context.BeekeeperAssociations.AsNoTracking().Include(a => a.AssociationOrg)
                .FirstOrDefaultAsync(a => a.BeekeeperId == id && a.ValidTo == null);
            ViewBag.Associations = await _context.Organizations.AsNoTracking()
                .Where(o => o.Capabilities.Any(c => c.Capability == Capabilities.Association))
                .OrderBy(o => o.DisplayName).ToListAsync();
            ViewBag.Method = _verifier.Method;
            ViewBag.Controller = "Registry";
            ViewBag.Action = "Edit";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAssociation(string id, int? associationOrgId)
        {
            if (!await _context.Beekeepers.AnyAsync(b => b.BeekeeperId == id))
                return NotFound();
            if (associationOrgId != null && !await _context.Organizations.AnyAsync(o => o.OrgId == associationOrgId
                    && o.Capabilities.Any(c => c.Capability == Capabilities.Association)))
            {
                TempData["Error"] = "Choose an organization with the Association capability.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var current = await _context.BeekeeperAssociations.FirstOrDefaultAsync(a => a.BeekeeperId == id && a.ValidTo == null);
            if (current?.AssociationOrgId == associationOrgId)
                return RedirectToAction(nameof(Edit), new { id });
            if (current != null)
                current.ValidTo = today;   // history is kept, never deleted
            if (associationOrgId != null)
                _context.BeekeeperAssociations.Add(new BeekeeperAssociation
                {
                    BeekeeperId = id,
                    AssociationOrgId = associationOrgId.Value,
                    ValidFrom = today,
                    CreatedByUserId = _current.UserId
                });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Association saved.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string id, string? enterpriseNumber, IFormFile? proof)
        {
            if (!await _context.Beekeepers.AnyAsync(b => b.BeekeeperId == id))
                return NotFound();
            if (!_verifier.IsWellFormed(enterpriseNumber ?? ""))
            {
                TempData["Error"] = "The number must be TR, a province code 01-81 and 10 digits (e.g. TR061234567890).";
                return RedirectToAction(nameof(Edit), new { id });
            }
            string? stored = null, sha = null;
            if (proof != null && proof.Length > 0)
            {
                if (await UploadValidator.ValidateDocumentAsync(proof) is string error)
                {
                    TempData["Error"] = error;
                    return RedirectToAction(nameof(Edit), new { id });
                }
                (stored, sha) = await FileStore.SaveAsync(_env, FileStore.RegistryProofs, proof);
            }

            var result = await _verifier.VerifyAsync(new RegistryCheckRequest(id, enterpriseNumber!, _current.UserId!,
                proof == null ? null : Path.GetFileName(proof.FileName), stored, sha));
            if (result.Status == RegistryStatus.Rejected)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decide(int requestId, bool approve, string? reason)
        {
            var request = await _context.RegistryVerifications.AsNoTracking().FirstOrDefaultAsync(v => v.Id == requestId);
            if (request == null)
                return NotFound();
            var error = await _manual.DecideAsync(requestId, approve, reason, _current.UserId!);
            if (error != null)
                TempData["Error"] = error;
            else
            {
                await _audit.WriteAsync("Verify", "RegistryVerifications", requestId.ToString(), null, new { Approved = approve, Reason = reason });
                TempData["Success"] = approve ? "Registry number verified." : "Request rejected.";
            }
            return RedirectToAction(nameof(Edit), new { id = request.BeekeeperId });
        }

        public async Task<IActionResult> Proof(int id)
        {
            var request = await _context.RegistryVerifications.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            var path = FileStore.PathOf(_env, FileStore.RegistryProofs, request?.StoredFileName);
            if (path == null)
                return NotFound();
            return PhysicalFile(path, FileStore.ContentTypeOf(path), request!.OriginalFileName ?? Path.GetFileName(path));
        }
    }

    public record RegistryRow(string BeekeeperId, string? BusinessNumber, string Name, string? RegistryNumber, string Status, string? Association);
}
