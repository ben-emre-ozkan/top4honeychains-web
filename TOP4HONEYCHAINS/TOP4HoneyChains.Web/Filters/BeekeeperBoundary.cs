using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Filters
{
    // D-78 P3 (D78-SPEC 2.4): a beekeeper account uses only its portal (/Me), signing out, its first password, the notices and the
    // public pages. Every other action, page or POST, answers 403 before authorization could redirect anywhere.
    public class BeekeeperBoundaryMiddleware
    {
        private static readonly HashSet<string> Open = new(StringComparer.OrdinalIgnoreCase)
        {
            "Account.Logout", "Account.FirstPassword", "Account.AccessDenied", "Account.Animations", "Home.SetLanguage",
            "Notices.Accept", "Notices.SendCode", "Notices.Public"
        };

        private readonly RequestDelegate _next;

        public BeekeeperBoundaryMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext http, TOP4HoneyChainsContext db, IMemoryCache cache)
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var action = http.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (userId != null && action != null && !Allowed(http.GetEndpoint()!, action) && await IsBeekeeperAsync(userId, db, cache))
            {
                http.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            await _next(http);
        }

        // the portal, the open account actions, and every page without an [Authorize] (public pages)
        private static bool Allowed(Endpoint endpoint, ControllerActionDescriptor action) =>
            action.ControllerName == "Me"
            || Open.Contains(action.ControllerName + "." + action.ActionName)
            || endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null
            || endpoint.Metadata.GetMetadata<IAuthorizeData>() == null;

        public static Task<bool> IsBeekeeperAsync(string userId, TOP4HoneyChainsContext db, IMemoryCache cache) =>
            cache.GetOrCreateAsync("beekeeper-account:" + userId, e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return db.BeekeeperAccounts.AsNoTracking().AnyAsync(a => a.UserId == userId);
            });
    }

    // the portal: an active account linked to an active beekeeper, while Features:BeekeeperAccounts is on
    public class BeekeeperSelfRequirement : IAuthorizationRequirement { }

    public class BeekeeperSelfHandler : AuthorizationHandler<BeekeeperSelfRequirement>
    {
        private readonly BeekeeperPortalService _portal;
        private readonly FeatureFlags _flags;

        public BeekeeperSelfHandler(BeekeeperPortalService portal, IOptionsSnapshot<FeatureFlags> flags)
        {
            _portal = portal;
            _flags = flags.Value;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BeekeeperSelfRequirement requirement)
        {
            if (_flags.BeekeeperAccounts && await _portal.BeekeeperOfAsync(context.User.FindFirstValue(ClaimTypes.NameIdentifier)) != null)
                context.Succeed(requirement);
        }
    }
}
