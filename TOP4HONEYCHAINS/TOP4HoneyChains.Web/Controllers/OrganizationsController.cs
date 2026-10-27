using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Organizations, capabilities, documents, verification and memberships (ROLE-MODEL P1).
    // Only with Features:OrgRoleModel; platform administrators manage, members read their own organization.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class OrganizationsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly IAuthorizationService _authz;
        private readonly OrgScope _scope;
        private readonly ICurrentOrg _current;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditWriter _audit;
        private readonly IWebHostEnvironment _env;

        public OrganizationsController(TOP4HoneyChainsContext context, IAuthorizationService authz, OrgScope scope,
            ICurrentOrg current, UserManager<ApplicationUser> userManager, AuditWriter audit, IWebHostEnvironment env)
        {
            _context = context;
            _authz = authz;
            _scope = scope;
            _current = current;
            _userManager = userManager;
            _audit = audit;
            _env = env;
        }

        private async Task<bool> AllowedAsync(Organization org, Microsoft.AspNetCore.Authorization.Infrastructure.OperationAuthorizationRequirement op) =>
            (await _authz.AuthorizeAsync(User, org, op)).Succeeded;

        private Task<Organization?> LoadAsync(int id) =>
            _context.Organizations.Include(o => o.Capabilities).FirstOrDefaultAsync(o => o.OrgId == id);

        // ---------------------------------------------------------------- list, create, edit

        public async Task<IActionResult> Index()
        {
            var orgs = await (await _scope.VisibleOrganizationsAsync())
                .Include(o => o.Capabilities)
                .OrderByDescending(o => o.IsPlatform).ThenBy(o => o.DisplayName)
                .ToListAsync();
            ViewBag.MemberCounts = await _context.Memberships.Where(m => m.Status == MembershipStatus.Active)
                .GroupBy(m => m.OrgId).Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
            ViewBag.Controller = "Organizations";
            ViewBag.Action = "Index";
            return View(orgs);
        }

        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> Create()
        {
            await FillCreateListsAsync();
            ViewBag.Controller = "Organizations";
            ViewBag.Action = "Create";
            return View(new Organization());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> Create([Bind("DisplayName,LegalName,TaxNo,Province,District,LegacyWholesalerId")] Organization org, string[]? capabilities)
        {
            capabilities ??= Array.Empty<string>();
            var known = await _context.Capabilities.Select(c => c.Code).ToListAsync();
            if (capabilities.Length == 0)
                ModelState.AddModelError("", "Choose at least one capability.");
            if (capabilities.Any(c => !known.Contains(c)) || capabilities.Contains(Capabilities.PlatformAdmin))
                ModelState.AddModelError("", "Unknown or reserved capability.");
            if (org.LegacyWholesalerId != null && await _context.Organizations.AnyAsync(o => o.LegacyWholesalerId == org.LegacyWholesalerId))
                ModelState.AddModelError("LegacyWholesalerId", "This wholesaler is already linked to an organization.");
            CheckPlace(org.Province, org.District, null, null);

            if (!ModelState.IsValid)
            {
                await FillCreateListsAsync();
                ViewBag.Selected = capabilities;
                return View(org);
            }

            org.Status = OrgStatus.Pending;
            org.CreatedByUserId = _current.UserId;
            foreach (var c in capabilities.Distinct())
                org.Capabilities.Add(new OrgCapability { Capability = c, AddedByUserId = _current.UserId });
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Organization created. It stays Pending until it is verified.";
            return RedirectToAction(nameof(Details), new { id = org.OrgId });
        }

        // D-52: an optional province from the list, and a district of that province; values already stored stay valid
        private void CheckPlace(string? province, string? district, string? storedProvince, string? storedDistrict)
        {
            var places = HttpContext.RequestServices.GetRequiredService<Places>();
            if (!places.Available || (province == storedProvince && district == storedDistrict))
                return;
            if (string.IsNullOrWhiteSpace(province))
            {
                if (!string.IsNullOrWhiteSpace(district))
                    ModelState.AddModelError("District", "Choose the province first.");
                return;
            }
            var p = places.Province(province);
            if (p == null)
                ModelState.AddModelError("Province", "Choose a province from the list.");
            else if (!string.IsNullOrWhiteSpace(district) && places.DistrictOf(p, district) == null && district != storedDistrict)
                ModelState.AddModelError("District", "Choose a district of the selected province.");
        }

        private async Task FillCreateListsAsync()
        {
            ViewBag.Capabilities = await _context.Capabilities.Where(c => c.Code != Capabilities.PlatformAdmin).OrderBy(c => c.SortOrder).ToListAsync();
            var linked = await _context.Organizations.Where(o => o.LegacyWholesalerId != null).Select(o => o.LegacyWholesalerId).ToListAsync();
            ViewBag.Wholesalers = new SelectList(await _context.Wholesalers.Where(w => !linked.Contains(w.WholesalerId)).OrderBy(w => w.WholesalerTitle).ToListAsync(),
                "WholesalerId", "WholesalerTitle");
        }

        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> Edit(int id)
        {
            var org = await LoadAsync(id);
            if (org == null)
                return NotFound();
            await FillWholesalersAsync(org.OrgId);
            ViewBag.Controller = "Organizations";
            ViewBag.Action = "Edit";
            return View(org);
        }

        // D-75: the wholesaler records an organization can be linked to: those not linked to another organization
        private async Task FillWholesalersAsync(int orgId)
        {
            var linked = await _context.Organizations.Where(o => o.LegacyWholesalerId != null && o.OrgId != orgId).Select(o => o.LegacyWholesalerId).ToListAsync();
            ViewBag.Wholesalers = new SelectList(await _context.Wholesalers.Where(w => !linked.Contains(w.WholesalerId)).OrderBy(w => w.WholesalerTitle).ToListAsync(),
                "WholesalerId", "WholesalerTitle");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> Edit(int id, [Bind("DisplayName,LegalName,TaxNo,Province,District,LegacyWholesalerId")] Organization posted)
        {
            var org = await LoadAsync(id);
            if (org == null)
                return NotFound();
            CheckPlace(posted.Province, posted.District, org.Province, org.District);
            if (posted.LegacyWholesalerId != null && await _context.Organizations.AnyAsync(o => o.LegacyWholesalerId == posted.LegacyWholesalerId && o.OrgId != id))
                ModelState.AddModelError("LegacyWholesalerId", "This wholesaler is already linked to an organization.");
            if (!ModelState.IsValid)
            {
                await FillWholesalersAsync(id);
                return View(org);
            }

            org.DisplayName = posted.DisplayName;
            org.LegalName = posted.LegalName;
            org.TaxNo = posted.TaxNo;
            org.Province = posted.Province;
            org.District = posted.District;
            org.LegacyWholesalerId = posted.LegacyWholesalerId;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Organization saved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------------------------------------------------------------- details

        public async Task<IActionResult> Details(int id)
        {
            var org = await _context.Organizations
                .Include(o => o.Capabilities)
                .Include(o => o.Documents)
                .Include(o => o.VerificationEvents)
                .Include(o => o.LegacyWholesaler)
                .FirstOrDefaultAsync(o => o.OrgId == id);
            // Another organization's page answers 404, so its existence is not revealed
            if (org == null || !await AllowedAsync(org, OrgOperations.Read))
                return NotFound();

            var members = await _context.Memberships.Where(m => m.OrgId == id).OrderBy(m => m.Status).ThenBy(m => m.OrgRole).ToListAsync();
            var userIds = members.Select(m => m.UserId).Distinct().ToList();
            ViewBag.UserNames = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Id);
            ViewBag.Members = members;
            ViewBag.MissingComplianceOfficer = MembershipRules.MissingComplianceOfficer(org.Status, members);
            ViewBag.CanEditDataController = await MayEditDataControllerAsync(id);
            ViewBag.CanManageDocuments = await AllowedAsync(org, OrgOperations.ManageDocuments);
            ViewBag.CanVerify = await AllowedAsync(org, OrgOperations.Verify);
            ViewBag.IsAdmin = _current.IsPlatformAdmin;
            ViewBag.AllCapabilities = await _context.Capabilities.OrderBy(c => c.SortOrder).ToListAsync();
            ViewBag.Roles = OrgRoles.All;
            ViewBag.DocumentKinds = OrgDocumentKinds.All;
            ViewBag.Controller = "Organizations";
            ViewBag.Action = "Details";
            return View(org);
        }

        // ---------------------------------------------------------------- capabilities (platform admin)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> AddCapability(int id, string capability, bool isAccredited, string? accreditationNumber, DateOnly? accreditationValidUntil)
        {
            var org = await LoadAsync(id);
            if (org == null)
                return NotFound();
            if (!await _context.Capabilities.AnyAsync(c => c.Code == capability) || capability == Capabilities.PlatformAdmin)
                TempData["Error"] = "Unknown or reserved capability.";
            else if (org.Capabilities.Any(c => c.Capability == capability))
                TempData["Error"] = "The organization already has this capability.";
            else if (capability == Capabilities.Laboratory && isAccredited && string.IsNullOrWhiteSpace(accreditationNumber))
                TempData["Error"] = "An accredited laboratory needs an accreditation number.";
            else
            {
                org.Capabilities.Add(new OrgCapability
                {
                    Capability = capability,
                    IsAccredited = capability == Capabilities.Laboratory && isAccredited,
                    AccreditationNumber = capability == Capabilities.Laboratory ? accreditationNumber?.Trim() : null,
                    AccreditationValidUntil = capability == Capabilities.Laboratory ? accreditationValidUntil : null,
                    AddedByUserId = _current.UserId
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Capability {capability} added.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> RemoveCapability(int id, string capability)
        {
            var org = await LoadAsync(id);
            var cap = org?.Capabilities.FirstOrDefault(c => c.Capability == capability);
            if (org == null || cap == null)
                return NotFound();
            if (capability == Capabilities.PlatformAdmin)
                TempData["Error"] = "The platform capability cannot be removed.";
            else if (org.Capabilities.Count == 1)
                TempData["Error"] = "An organization needs at least one capability.";
            else if (capability == Capabilities.BeekeeperRegistration
                     && await _context.Memberships.AnyAsync(m => m.OrgId == id && m.OrgRole == OrgRoles.RegistrationOfficer && m.Status == MembershipStatus.Active))
                TempData["Error"] = "Revoke the beekeeper registration officers of this organization first.";
            else
            {
                _context.OrgCapabilities.Remove(cap);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Capability {capability} removed.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> SetFourEyesExempt(int id, bool exempt)
        {
            var org = await LoadAsync(id);
            if (org == null)
                return NotFound();
            org.FourEyesExempt = exempt;
            await _context.SaveChangesAsync();
            TempData["Success"] = exempt
                ? "Four-eyes exemption set: members may approve their own CCP records; each such approval is flagged in the audit log. It never applies to lab results."
                : "Four-eyes exemption removed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------------------------------------------------------------- documents

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int id, string kind, IFormFile? file)
        {
            var org = await LoadAsync(id);
            if (org == null || !await AllowedAsync(org, OrgOperations.Read))
                return NotFound();
            if (!await AllowedAsync(org, OrgOperations.ManageDocuments))
                return Forbid();

            if (!OrgDocumentKinds.All.Contains(kind))
                TempData["Error"] = "Choose a document type.";
            else if (file == null || file.Length == 0)
                TempData["Error"] = "Choose a file.";
            else if (await UploadValidator.ValidateDocumentAsync(file) is string error)
                TempData["Error"] = error;
            else
            {
                var (stored, sha) = await FileStore.SaveAsync(_env, FileStore.OrgDocuments, file);
                _context.OrgDocuments.Add(new OrgDocument
                {
                    OrgId = id,
                    Kind = kind,
                    OriginalFileName = Truncate(Path.GetFileName(file.FileName), 255),
                    StoredFileName = stored,
                    Sha256 = sha,
                    UploadedByUserId = _current.UserId!
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Document uploaded.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Document(int id)
        {
            var doc = await _context.OrgDocuments.Include(d => d.Org).ThenInclude(o => o!.Capabilities).FirstOrDefaultAsync(d => d.DocumentId == id);
            if (doc == null || !await AllowedAsync(doc.Org!, OrgOperations.Read))
                return NotFound();
            var path = FileStore.PathOf(_env, FileStore.OrgDocuments, doc.StoredFileName);
            if (path == null)
                return NotFound();
            return PhysicalFile(path, FileStore.ContentTypeOf(path), doc.OriginalFileName);
        }

        // ---------------------------------------------------------------- verification workflow

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string toStatus, string? reason)
        {
            var org = await _context.Organizations.Include(o => o.Capabilities).Include(o => o.Documents).FirstOrDefaultAsync(o => o.OrgId == id);
            if (org == null || !await AllowedAsync(org, OrgOperations.Read))
                return NotFound();
            if (!await AllowedAsync(org, OrgOperations.Verify))
                return Forbid();

            reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            string? error = null;
            if (org.IsPlatform)
                error = "The platform organization cannot change status.";
            else if (toStatus == OrgStatus.Verified && org.Status is OrgStatus.Pending or OrgStatus.Suspended)
            {
                if (org.Status == OrgStatus.Pending && org.Documents.Count == 0)
                    error = "Upload at least one document (licence, accreditation, registry proof) before verifying.";
            }
            else if (toStatus == OrgStatus.Suspended && org.Status == OrgStatus.Verified)
            {
                if (reason == null)
                    error = "A reason is required to suspend.";
            }
            else if (toStatus == OrgStatus.Pending && org.Status == OrgStatus.Pending)
            {
                if (reason == null)
                    error = "Write which documents are requested.";
            }
            else
                error = $"{org.Status} cannot change to {toStatus}.";
            if (reason?.Length > 500)
                error = "The reason can be at most 500 characters.";

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.OrgVerificationEvents.Add(new OrgVerificationEvent
            {
                OrgId = id,
                FromStatus = org.Status,
                ToStatus = toStatus,
                Reason = reason,
                ByUserId = _current.UserId!
            });
            if (toStatus == OrgStatus.Verified)
            {
                org.VerifiedAt = DateTime.UtcNow;
                org.VerifiedByUserId = _current.UserId;
            }
            org.Status = toStatus;
            await _context.SaveChangesAsync();
            await _audit.WriteAsync("Verify", "Organizations", id.ToString(), null, new { Status = toStatus, Reason = reason });
            TempData["Success"] = toStatus == OrgStatus.Pending ? "Documents requested." : $"Organization is now {toStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------------------------------------------------------------- memberships (platform admin)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> AddMember(int id, string orgRole, string? existingUserName, string? newUserName, string? newEmail, string? newPassword)
        {
            var org = await LoadAsync(id);
            if (org == null)
                return NotFound();
            if (!OrgRoles.All.Contains(orgRole))
            {
                TempData["Error"] = "Choose an org role.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (MembershipRules.Check(orgRole, org.Capabilities.Select(c => c.Capability)) is string placement)
            {
                TempData["Error"] = placement;
                return RedirectToAction(nameof(Details), new { id });
            }

            ApplicationUser? user;
            if (!string.IsNullOrWhiteSpace(existingUserName))
            {
                user = await _userManager.FindByNameAsync(existingUserName.Trim());
                if (user == null)
                {
                    TempData["Error"] = "No user with this user name.";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(newUserName) || string.IsNullOrWhiteSpace(newEmail) || string.IsNullOrWhiteSpace(newPassword))
                {
                    TempData["Error"] = "Enter an existing user name, or a new user name, e-mail and password.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                user = new ApplicationUser { UserName = newUserName.Trim(), Email = newEmail.Trim() };
                // Members get no global role: their access comes only from memberships
                var result = await _userManager.CreateAsync(user, newPassword);
                if (!result.Succeeded)
                {
                    TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Details), new { id });
                }
                await _audit.WriteAsync("Create", "AspNetUsers", user.Id, null, new { user.UserName, user.Email });
            }

            if (await _context.Memberships.AnyAsync(m => m.UserId == user.Id && m.OrgId == id && m.OrgRole == orgRole && m.Status == MembershipStatus.Active))
            {
                TempData["Error"] = "The user already has this role in the organization.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Memberships.Add(new Membership { UserId = user.Id, OrgId = id, OrgRole = orgRole, GrantedByUserId = _current.UserId });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{user.UserName} added as {orgRole}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> RevokeMember(int membershipId)
        {
            var m = await _context.Memberships.FirstOrDefaultAsync(x => x.MembershipId == membershipId);
            if (m == null)
                return NotFound();
            if (m.Status == MembershipStatus.Active)
            {
                if (m.UserId == _current.UserId && await _context.Organizations.AnyAsync(o => o.OrgId == m.OrgId && o.IsPlatform))
                {
                    TempData["Error"] = "You cannot revoke your own platform membership.";
                    return RedirectToAction(nameof(Details), new { id = m.OrgId });
                }
                m.Status = MembershipStatus.Revoked;
                m.RevokedAt = DateTime.UtcNow;
                m.RevokedByUserId = _current.UserId;
                await _context.SaveChangesAsync();
                // Old sign-in cookies of the user are re-validated (security stamp)
                var user = await _userManager.FindByIdAsync(m.UserId);
                if (user != null)
                    await _userManager.UpdateSecurityStampAsync(user);
                TempData["Success"] = "Membership revoked.";
            }
            return RedirectToAction(nameof(Details), new { id = m.OrgId });
        }

        // ---------------------------------------------------------------- data controller (D-78, D78-SPEC 3.5, 6.1)

        // platform administrators, and the organization's own Compliance Officer
        private async Task<bool> MayEditDataControllerAsync(int orgId)
        {
            if (_current.IsPlatformAdmin)
                return true;
            var active = await _current.GetAsync();
            return active != null && active.OrgId == orgId && active.OrgRole == OrgRoles.ComplianceOfficer;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataController(int id, string? name, string? address, string? contact, string? verbis)
        {
            var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OrgId == id);
            if (org == null || !await AllowedAsync(org, OrgOperations.Read))
                return NotFound();
            if (!await MayEditDataControllerAsync(id))
                return Forbid();
            static string? Clean(string? s, int max) => string.IsNullOrWhiteSpace(s) ? null : s.Trim().Length > max ? s.Trim()[..max] : s.Trim();
            org.DataControllerName = Clean(name, 200);
            org.DataControllerAddress = Clean(address, 400);
            org.DataControllerContact = Clean(contact, 200);
            org.VerbisNumber = Clean(verbis, 50);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Data controller details saved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------------------------------------------------------------- helpers

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
    }
}
