using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // Features:OrgRoleModel (default false). Off = v1.0 behaviour: admin-only panel (D-01).
    public class FeatureFlags
    {
        public bool OrgRoleModel { get; set; }
    }

    public record ActiveMembership(
        int MembershipId, int OrgId, string OrgName, string OrgRole, string OrgStatus,
        bool IsPlatform, bool FourEyesExempt, IReadOnlySet<string> Capabilities)
    {
        public bool IsVerified => OrgStatus == Models.Entities.OrgStatus.Verified;
        public bool Has(string capability) => Capabilities.Contains(capability);
    }

    public record MembershipOption(int MembershipId, int OrgId, string OrgName, string OrgRole, string OrgStatus);

    // The signed-in user and their active membership. The choice is kept in a cookie, but every request
    // re-reads the membership from the database and checks that it belongs to the user and is active, so
    // a revoked membership or a suspended organization takes effect at once.
    public interface ICurrentOrg
    {
        bool Enabled { get; }
        string? UserId { get; }
        string? UserName { get; }
        bool IsPlatformAdmin { get; }
        Task<ActiveMembership?> GetAsync();
        Task<List<MembershipOption>> GetMembershipsAsync();
    }

    public class CurrentOrg : ICurrentOrg
    {
        public const string CookieName = "T4H.Membership";
        public const string AdminRole = "Administrator";

        private readonly IHttpContextAccessor _http;
        private readonly TOP4HoneyChainsContext _context;
        private readonly FeatureFlags _flags;
        private ActiveMembership? _active;
        private bool _loaded;

        public CurrentOrg(IHttpContextAccessor http, TOP4HoneyChainsContext context, IOptions<FeatureFlags> flags)
        {
            _http = http;
            _context = context;
            _flags = flags.Value;
        }

        private ClaimsPrincipal? User => _http.HttpContext?.User;

        public bool Enabled => _flags.OrgRoleModel;
        public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
        public string? UserName => User?.Identity?.Name;

        // P1: platform administrators are the users in the Administrator role (D-32)
        public bool IsPlatformAdmin => User?.IsInRole(AdminRole) == true;

        public async Task<List<MembershipOption>> GetMembershipsAsync()
        {
            if (!Enabled || UserId == null)
                return new();
            return await _context.Memberships.AsNoTracking()
                .Where(m => m.UserId == UserId && m.Status == MembershipStatus.Active)
                .OrderBy(m => m.Org!.DisplayName).ThenBy(m => m.OrgRole)
                .Select(m => new MembershipOption(m.MembershipId, m.OrgId, m.Org!.DisplayName, m.OrgRole, m.Org.Status))
                .ToListAsync();
        }

        public async Task<ActiveMembership?> GetAsync()
        {
            if (_loaded)
                return _active;
            _loaded = true;
            if (!Enabled || UserId == null)
                return null;

            var options = await GetMembershipsAsync();
            if (options.Count == 0)
                return null;

            int.TryParse(_http.HttpContext?.Request.Cookies[CookieName], out var chosenId);
            var chosen = options.FirstOrDefault(o => o.MembershipId == chosenId) ?? options[0];

            var org = await _context.Organizations.AsNoTracking()
                .Where(o => o.OrgId == chosen.OrgId)
                .Select(o => new { o.DisplayName, o.Status, o.IsPlatform, o.FourEyesExempt, Caps = o.Capabilities.Select(c => c.Capability).ToList() })
                .FirstAsync();
            _active = new ActiveMembership(chosen.MembershipId, chosen.OrgId, org.DisplayName, chosen.OrgRole, org.Status,
                org.IsPlatform, org.FourEyesExempt, org.Caps.ToHashSet());
            return _active;
        }
    }

    // Pages of the organization model answer 404 while the feature flag is off
    public class RequireOrgModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var flags = context.HttpContext.RequestServices.GetRequiredService<IOptions<FeatureFlags>>().Value;
            if (!flags.OrgRoleModel)
                context.Result = new NotFoundResult();
        }
    }
}
