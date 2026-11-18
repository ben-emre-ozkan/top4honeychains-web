using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using A = TOP4HoneyChains.Web.Models.Entities.PermissionAreas;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-56: roles and permissions in one place for the platform administrator: members (user, organization, role),
    // the permission matrix per role (CCP enter/approve from the CCP responsibility matrix, everything else from
    // RolePermissions) and organization capabilities. Fixed and not editable here: no self-approval, four-eyes on
    // lab results, the append-only audit log. Every change is audited by the interceptor.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class PermissionsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly Permissions _perm;
        private readonly UserManager<ApplicationUser> _users;

        public PermissionsController(TOP4HoneyChainsContext context, ICurrentOrg current, Permissions perm, UserManager<ApplicationUser> users)
        {
            _context = context;
            _current = current;
            _perm = perm;
            _users = users;
        }

        public record MemberRow(int MembershipId, string UserName, string OrgName, string OrgRole, DateTime GrantedAt);
        public record Cell(string Key, bool Allowed, bool Available);
        public record MatrixRow(string Label, string? Detail, Cell Enter, Cell Approve, Cell Read, int? CcpId = null);
        public record CapabilityRow(int OrgId, string OrgName, string Status, HashSet<string> Capabilities);

        public async Task<IActionResult> Index(string? tab, string? role)
        {
            tab = tab is "roles" or "capabilities" ? tab : "members";
            role = role != null && OrgRoles.All.Contains(role) ? role : OrgRoles.All[0];
            ViewBag.Tab = tab;
            ViewBag.Role = role;
            ViewBag.Controller = "Roles and permissions";
            ViewBag.Action = tab == "members" ? "Members" : tab == "roles" ? "Role permissions" : "Capabilities";

            if (tab == "members")
            {
                // memberships live in the business database, users in the identity database: joined in memory
                var memberships = await (from m in _context.Memberships.AsNoTracking()
                                         join o in _context.Organizations.AsNoTracking() on m.OrgId equals o.OrgId
                                         where m.Status == MembershipStatus.Active
                                         select new { m.MembershipId, m.UserId, o.DisplayName, m.OrgRole, m.GrantedAt }).ToListAsync();
                var users = await _users.Users.AsNoTracking().Select(u => new { u.Id, u.UserName }).ToListAsync();
                var names = users.ToDictionary(u => u.Id, u => u.UserName ?? u.Id);
                ViewBag.Members = memberships.Select(m => new MemberRow(m.MembershipId, names.TryGetValue(m.UserId, out var n) ? n : m.UserId, m.DisplayName, m.OrgRole, m.GrantedAt))
                    .OrderBy(m => m.OrgName).ThenBy(m => m.UserName).ToList();
                ViewBag.Users = users.Select(u => u.UserName ?? "").Where(n => n.Length > 0).OrderBy(n => n).ToList();
                ViewBag.Orgs = await _context.Organizations.AsNoTracking().OrderBy(o => o.DisplayName).Select(o => new { o.OrgId, o.DisplayName }).ToListAsync();
            }
            else if (tab == "roles")
                ViewBag.Matrix = await MatrixAsync(role);
            else
            {
                ViewBag.AllCapabilities = await _context.Capabilities.AsNoTracking().Where(c => c.Code != Capabilities.PlatformAdmin).OrderBy(c => c.SortOrder).Select(c => c.Code).ToListAsync();
                ViewBag.CapabilityRows = (await _context.Organizations.AsNoTracking().Include(o => o.Capabilities).Where(o => !o.IsPlatform).OrderBy(o => o.DisplayName).ToListAsync())
                    .Select(o => new CapabilityRow(o.OrgId, o.DisplayName, o.Status, o.Capabilities.Select(c => c.Capability).ToHashSet())).ToList();
            }
            return View();
        }

        private async Task<List<MatrixRow>> MatrixAsync(string role)
        {
            var ccps = await _context.CriticalControlPoints.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
            var roles = await _context.CcpResponsibilityRoles.AsNoTracking().Where(r => r.OrgRole == role).ToListAsync();
            var rows = new List<MatrixRow>();
            foreach (var c in ccps)
            {
                var id = c.Id.ToString(CultureInfo.InvariantCulture);
                rows.Add(new MatrixRow(c.Code, c.Title,
                    new Cell($"Ccp:{id}:{A.Enter}", roles.Any(r => r.CcpId == c.Id && r.Action == CcpActions.Enter), true),
                    new Cell($"Ccp:{id}:{A.Approve}", roles.Any(r => r.CcpId == c.Id && r.Action == CcpActions.Approve), true),
                    new Cell($"Ccp:{id}:{A.Read}", _perm.CcpRead(c.Id, role), true), c.Id));
            }
            foreach (var (area, actions) in A.Functions)
            {
                Cell C(string action) => new($"{area}::{action}", actions.Contains(action) && _perm.Allows(area, action, role), actions.Contains(action));
                rows.Add(new MatrixRow("Area." + area, null, C(A.Enter), C(A.Approve), C(A.Read)));
            }
            return rows;
        }

        // ---------------------------------------------------------------- members

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(string userName, int orgId, string orgRole)
        {
            var user = string.IsNullOrWhiteSpace(userName) ? null : await _users.FindByNameAsync(userName.Trim());
            string? error = null;
            if (user == null)
                error = "No user with this user name.";
            else if (!await _context.Organizations.AnyAsync(o => o.OrgId == orgId))
                error = "Choose an organization.";
            else if (!OrgRoles.All.Contains(orgRole))
                error = "Choose an org role.";
            else if (await ConsumerRules.IsConsumerAsync(_context, user.Id))
                error = ConsumerRules.NoRoleForConsumers;   // D-87
            else if (MembershipRules.Check(orgRole, await _context.OrgCapabilities.Where(c => c.OrgId == orgId).Select(c => c.Capability).ToListAsync()) is string placement)
                error = placement;
            else if (await _context.Memberships.AnyAsync(m => m.UserId == user.Id && m.OrgId == orgId && m.OrgRole == orgRole && m.Status == MembershipStatus.Active))
                error = "The user already has this role in the organization.";
            if (error != null)
                TempData["Error"] = error;
            else
            {
                _context.Memberships.Add(new Membership { UserId = user!.Id, OrgId = orgId, OrgRole = orgRole, GrantedByUserId = _current.UserId });
                await _context.SaveChangesAsync();
                TempData["Success"] = $"{user.UserName} added as {orgRole}.";
            }
            return RedirectToAction(nameof(Index), new { tab = "members" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    return RedirectToAction(nameof(Index), new { tab = "members" });
                }
                m.Status = MembershipStatus.Revoked;
                m.RevokedAt = DateTime.UtcNow;
                m.RevokedByUserId = _current.UserId;
                await _context.SaveChangesAsync();
                var user = await _users.FindByIdAsync(m.UserId);
                if (user != null)
                    await _users.UpdateSecurityStampAsync(user);
                TempData["Success"] = "Membership revoked.";
            }
            return RedirectToAction(nameof(Index), new { tab = "members" });
        }

        // ---------------------------------------------------------------- role matrix

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRole(string role, string[]? allow)
        {
            if (!OrgRoles.All.Contains(role))
                return NotFound();
            var wanted = (allow ?? Array.Empty<string>()).ToHashSet();
            var ccps = await _context.CriticalControlPoints.AsNoTracking().Select(c => c.Id).ToListAsync();
            var configs = await _context.CcpResponsibilities.Include(r => r.Roles).ToListAsync();
            var stored = await _context.RolePermissions.ToListAsync();
            if (stored.Count == 0)
            {
                // first edit: start from the built-in defaults so other roles keep their rights
                stored = Permissions.Defaults(ccps).Select(d => new RolePermission { Area = d.Area, AreaKey = d.Key, Action = d.Action, OrgRole = d.Role, UpdatedAt = DateTime.UtcNow, UpdatedByUserId = _current.UserId }).ToList();
                _context.RolePermissions.AddRange(stored);
            }

            // CCP enter / approve: the CCP responsibility matrix
            foreach (var id in ccps)
            {
                var key = id.ToString(CultureInfo.InvariantCulture);
                var cfg = configs.FirstOrDefault(c => c.CcpId == id);
                if (cfg == null)
                    continue;
                foreach (var action in new[] { CcpActions.Enter, CcpActions.Approve })
                {
                    var want = wanted.Contains($"Ccp:{key}:{action}") && (action == CcpActions.Enter || cfg.RequiresApproval);
                    var row = cfg.Roles.FirstOrDefault(r => r.Action == action && r.OrgRole == role);
                    if (want && row == null)
                        cfg.Roles.Add(new CcpResponsibilityRole { CcpId = id, Action = action, OrgRole = role });
                    else if (!want && row != null)
                        _context.CcpResponsibilityRoles.Remove(row);
                }
                SetRight(stored, A.Ccp, key, A.Read, role, wanted.Contains($"Ccp:{key}:{A.Read}"));
            }
            foreach (var (area, actions) in A.Functions)
                foreach (var action in actions)
                    SetRight(stored, area, "", action, role, wanted.Contains($"{area}::{action}"));

            // every right keeps at least one role (nobody could act otherwise; lab results always need an approver)
            var left = new List<string>();
            foreach (var cfg in configs)
            {
                var enter = cfg.Roles.Where(r => r.Action == CcpActions.Enter && _context.Entry(r).State != EntityState.Deleted).ToList();
                var approve = cfg.Roles.Where(r => r.Action == CcpActions.Approve && _context.Entry(r).State != EntityState.Deleted).ToList();
                var code = await _context.CriticalControlPoints.Where(c => c.Id == cfg.CcpId).Select(c => c.Code).FirstAsync();
                if (enter.Count == 0)
                    left.Add($"{code} {A.Enter}");
                if (cfg.RequiresApproval && approve.Count == 0)
                    left.Add($"{code} {A.Approve}");
            }
            foreach (var (area, actions) in A.Functions)
                foreach (var action in actions)
                    if (!stored.Any(p => p.Area == area && p.AreaKey == "" && p.Action == action && _context.Entry(p).State != EntityState.Deleted))
                        left.Add($"{area} {action}");
            if (left.Count > 0)
            {
                TempData["Error"] = $"Every right needs at least one role; nobody would have: {string.Join(", ", left)}.";
                return RedirectToAction(nameof(Index), new { tab = "roles", role });
            }
            await _context.SaveChangesAsync();
            _perm.Invalidate();
            TempData["Success"] = $"Permissions of {role} saved; they apply at once.";
            return RedirectToAction(nameof(Index), new { tab = "roles", role });
        }

        private void SetRight(List<RolePermission> stored, string area, string key, string action, string role, bool want)
        {
            var row = stored.FirstOrDefault(p => p.Area == area && p.AreaKey == key && p.Action == action && p.OrgRole == role);
            if (want && row == null)
            {
                row = new RolePermission { Area = area, AreaKey = key, Action = action, OrgRole = role, UpdatedAt = DateTime.UtcNow, UpdatedByUserId = _current.UserId };
                stored.Add(row);
                _context.RolePermissions.Add(row);
            }
            else if (!want && row != null)
            {
                if (_context.Entry(row).State == EntityState.Added)
                    _context.Entry(row).State = EntityState.Detached;
                else
                    _context.RolePermissions.Remove(row);
                stored.Remove(row);
            }
        }

        // ---------------------------------------------------------------- capabilities

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCapabilities(int orgId, string[]? capabilities)
        {
            var org = await _context.Organizations.Include(o => o.Capabilities).FirstOrDefaultAsync(o => o.OrgId == orgId && !o.IsPlatform);
            if (org == null)
                return NotFound();
            var known = await _context.Capabilities.Where(c => c.Code != Capabilities.PlatformAdmin).Select(c => c.Code).ToListAsync();
            var wanted = (capabilities ?? Array.Empty<string>()).Where(known.Contains).ToHashSet();
            if (wanted.Count == 0)
            {
                TempData["Error"] = "An organization needs at least one capability.";
                return RedirectToAction(nameof(Index), new { tab = "capabilities" });
            }
            _context.OrgCapabilities.RemoveRange(org.Capabilities.Where(c => c.Capability != Capabilities.PlatformAdmin && !wanted.Contains(c.Capability)));
            foreach (var c in wanted.Where(c => org.Capabilities.All(x => x.Capability != c)))
                org.Capabilities.Add(new OrgCapability { Capability = c, AddedByUserId = _current.UserId });
            await _context.SaveChangesAsync();
            await LegacyWholesalers.EnsureAsync(_context, orgId);   // D-88
            TempData["Success"] = $"Capabilities of {org.DisplayName} saved.";
            return RedirectToAction(nameof(Index), new { tab = "capabilities" });
        }
    }
}
