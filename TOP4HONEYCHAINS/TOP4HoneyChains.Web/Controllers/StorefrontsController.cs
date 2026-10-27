using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Beekeeper storefronts (ROLE-MODEL section 11, D-30, D-43): the OrgAdmin of a verified association
    // records the consent of a member beekeeper (date, signed consent document) or withdraws it; the
    // beekeeper never logs in. Platform administrators read everything.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public partial class StorefrontsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly StorefrontService _storefronts;
        private readonly IWebHostEnvironment _env;
        // D-56: rights of organization roles (editable matrix)
        private TOP4HoneyChains.Web.Services.OrgModel.Permissions Perm => HttpContext.RequestServices.GetRequiredService<TOP4HoneyChains.Web.Services.OrgModel.Permissions>();

        public StorefrontsController(TOP4HoneyChainsContext context, ICurrentOrg current, StorefrontService storefronts, IWebHostEnvironment env)
        {
            _context = context;
            _current = current;
            _storefronts = storefronts;
            _env = env;
        }

        public record Row(Beekeeper Beekeeper, StorefrontService.State State, string? RegistryNumber);

        private IActionResult ReadDenied(ActiveMembership active)
        {
            TempData["Error"] = $"Your role ({active.OrgRole}) cannot open this page; roles: {string.Join(", ", Perm.Roles(PermissionAreas.Storefront, PermissionAreas.Read))}.";
            return RedirectToAction("Index", "MyWork");
        }

        // Why the active membership may not manage storefronts (null = allowed)
        private async Task<string?> ManageDeniedAsync()
        {
            if (_current.IsPlatformAdmin)
                return "Platform administrators are read-only on business data; request a correction instead.";
            var active = await _current.GetAsync();
            if (active == null)
                return "No active membership.";
            if (!active.Has(Capabilities.Association))
                return "Only a beekeepers' association manages storefronts.";
            if (!active.IsVerified)
                return $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            if (!Perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Storefront, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter, active.OrgRole))
                return $"Your role ({active.OrgRole}) cannot manage storefronts; roles: {string.Join(", ", Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Storefront, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter))}.";
            return null;
        }

        // A member beekeeper of the active association (current link)
        private async Task<Beekeeper?> MemberAsync(string beekeeperId)
        {
            var active = await _current.GetAsync();
            if (active == null)
                return null;
            var member = await _context.BeekeeperAssociations.AnyAsync(a => a.BeekeeperId == beekeeperId && a.AssociationOrgId == active.OrgId && a.ValidTo == null);
            return member ? await _context.Beekeepers.AsNoTracking().FirstOrDefaultAsync(b => b.BeekeeperId == beekeeperId) : null;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Controller = "Storefronts";
            ViewBag.Action = "Index";
            IQueryable<string> ids;
            if (_current.IsPlatformAdmin)
                ids = _context.Consents.Where(c => c.Purpose == ConsentPurpose.Storefront).Select(c => c.BeekeeperId).Distinct();
            else
            {
                var active = await _current.GetAsync();
                if (active == null || !active.Has(Capabilities.Association))
                    return NotFound();
                // C1 (D-84): the read right is checked on the page, not only in the menu
                if (!MemberMenu.Storefronts(active, Perm))
                    return ReadDenied(active);
                ids = _context.BeekeeperAssociations.Where(a => a.AssociationOrgId == active.OrgId && a.ValidTo == null).Select(a => a.BeekeeperId);
            }
            var list = await ids.Take(500).ToListAsync();
            var states = await _storefronts.StatesAsync(list);
            var beekeepers = await _context.Beekeepers.AsNoTracking().Where(b => list.Contains(b.BeekeeperId)).ToListAsync();
            var registry = await _context.BeekeeperRegistry.AsNoTracking().Where(r => list.Contains(r.BeekeeperId)).ToDictionaryAsync(r => r.BeekeeperId, r => r.RegistryNumber);
            ViewBag.Rows = beekeepers.OrderBy(b => b.BeekeeperLastName).ThenBy(b => b.BeekeeperFirstName)
                .Select(b => new Row(b, states[b.BeekeeperId], registry.GetValueOrDefault(b.BeekeeperId))).ToList();
            ViewBag.ManageDenied = await ManageDeniedAsync();
            ViewBag.IsAdmin = _current.IsPlatformAdmin;
            // D-78 P3 (O-6): members who let the association read their asking prices
            ViewBag.PriceConsents = _current.IsPlatformAdmin || await _current.GetAsync() is not { } m ? new HashSet<string>() : await PriceConsentsAsync(m.OrgId, list);
            return View();
        }

        public async Task<IActionResult> Consent(string id)
        {
            if (await ManageDeniedAsync() is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            var bk = await MemberAsync(id);
            if (bk == null)
                return NotFound();
            ViewBag.Beekeeper = bk;
            ViewBag.State = (await _storefronts.StatesAsync(new[] { id }))[id];
            ViewBag.Controller = "Storefronts";
            ViewBag.Action = "Consent";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Consent(string id, bool nameConsented, DateOnly? givenOn, IFormFile? document, bool textRead)
        {
            if (await ManageDeniedAsync() is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            var bk = await MemberAsync(id);
            if (bk == null)
                return NotFound();
            var active = (await _current.GetAsync())!;
            var fileError = document == null ? null : await UploadValidator.ValidateDocumentAsync(document);
            string? error = null;
            if (await _context.Consents.AnyAsync(c => c.BeekeeperId == id && c.Purpose == ConsentPurpose.Storefront && c.WithdrawnAt == null))
                error = "This beekeeper already has an active consent; withdraw it first to record a new one.";
            else if (!textRead)
                error = "Confirm that the consent text was read to the beekeeper.";
            else if (givenOn == null || givenOn > DateOnly.FromDateTime(DateTime.Today))
                error = "Enter the date of the consent (not in the future).";
            else if (document == null)
                error = "Attach the signed consent document.";
            else if (fileError != null)
                error = fileError;
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Consent), new { id });
            }

            var (stored, sha) = await FileStore.SaveAsync(_env, StorefrontService.ConsentDocuments, document!);
            try
            {
                _context.Consents.Add(new Consent
                {
                    BeekeeperId = id,
                    AssociationOrgId = active.OrgId,
                    Purpose = ConsentPurpose.Storefront,
                    NameConsented = nameConsented,
                    TextVersion = ConsentText.Version,
                    TextSha256 = ConsentText.Sha256,
                    GivenOn = givenOn!.Value,
                    DocumentStoredFileName = stored,
                    DocumentOriginalFileName = Path.GetFileName(document!.FileName),
                    DocumentSha256 = sha,
                    RecordedByUserId = _current.UserId!,
                    RecordedAt = DateTime.UtcNow
                });
                await _storefronts.EnsureCodeAsync(id);
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (FileStore.PathOf(_env, StorefrontService.ConsentDocuments, stored) is string path)
                    System.IO.File.Delete(path);
                throw;
            }
            var state = (await _storefronts.StatesAsync(new[] { id }))[id];
            TempData["Success"] = state.Published
                ? "Consent recorded; the storefront is published."
                : "Consent recorded. The storefront is published once the registry number is verified.";
            return RedirectToAction(nameof(Index));
        }

        // Withdrawal unpublishes at once (D-30)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int consentId, string? reason)
        {
            if (await ManageDeniedAsync() is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            // D-78: the association withdraws only storefront consents; consents given at registration are managed elsewhere
            var consent = await _context.Consents.FirstOrDefaultAsync(c => c.ConsentId == consentId && c.Purpose == ConsentPurpose.Storefront);
            if (consent == null || await MemberAsync(consent.BeekeeperId) == null)
                return NotFound();
            reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            if (consent.WithdrawnAt != null)
                TempData["Error"] = "This consent was already withdrawn.";
            else if (reason?.Length > 500)
                TempData["Error"] = "The reason can be at most 500 characters.";
            else
            {
                consent.WithdrawnAt = DateTime.UtcNow;
                consent.WithdrawnByUserId = _current.UserId;
                consent.WithdrawReason = reason;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Consent withdrawn; the storefront is no longer public.";
            }
            return RedirectToAction(nameof(Index));
        }

        // The signed consent document: the association that recorded it and platform administrators
        public async Task<IActionResult> Document(int id)
        {
            var consent = await _context.Consents.AsNoTracking().FirstOrDefaultAsync(c => c.ConsentId == id);
            if (consent == null)
                return NotFound();
            // C2 (D-84): the association's admin (who records storefront consents) and Compliance Officer
            var access = HttpContext.RequestServices.GetRequiredService<EvidenceAccess>();
            var result = await access.CheckAsync(consent.AssociationOrgId);
            if (result != EvidenceAccess.Result.Allowed)
                return EvidenceAccess.Refuse(result);
            var path = FileStore.PathOf(_env, StorefrontService.ConsentDocuments, consent.DocumentStoredFileName);
            if (path == null)
                return NotFound();
            await access.AuditAsync("Consents", id.ToString(), consent.DocumentOriginalFileName);
            return PhysicalFile(path, FileStore.ContentTypeOf(path), consent.DocumentOriginalFileName);
        }

        // Printable card with the QR code of the public page
        public async Task<IActionResult> Card(string id)
        {
            if (!_current.IsPlatformAdmin && await MemberAsync(id) == null)
                return NotFound();
            if (!_current.IsPlatformAdmin && await _current.GetAsync() is { } m && !MemberMenu.Storefronts(m, Perm))
                return ReadDenied(m);
            var bk = await _context.Beekeepers.AsNoTracking().FirstOrDefaultAsync(b => b.BeekeeperId == id);
            var state = bk == null ? null : (await _storefronts.StatesAsync(new[] { id })).GetValueOrDefault(id);
            if (state == null || !state.Published)
            {
                TempData["Error"] = "The storefront is not published.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.State = state;
            ViewBag.DisplayName = StorefrontService.DisplayName(state.Consent!.NameConsented, bk!.BeekeeperFirstName, bk.BeekeeperLastName, state.Code!);
            ViewBag.PublicUrl = PublicUrl.Build(HttpContext, Url.Content("~/storefront/" + state.Code));
            return View();
        }
    }
}
