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
    // public pages. D-87: a consumer account uses only its own area (/Consumer), signing out, its notice, the language switch and the
    // public pages. For both kinds every other action, page or POST answers 403 before authorization could redirect anywhere: the
    // fallback policy only requires a signed-in user, so this boundary is what keeps both kinds away from the staff pages.
    public class BeekeeperBoundaryMiddleware
    {
        private static readonly HashSet<string> Open = new(StringComparer.OrdinalIgnoreCase)
        {
            "Account.Logout", "Account.FirstPassword", "Account.AccessDenied", "Account.Animations", "Account.TourSeen", "Home.SetLanguage",
            "Notices.Accept", "Notices.SendCode", "Notices.Public"
        };

        // D-87: consumers never receive issued sign-in details, so the first-password page is not theirs
        private static readonly HashSet<string> ConsumerOpen = new(StringComparer.OrdinalIgnoreCase)
        {
            "Account.Logout", "Account.AccessDenied", "Account.Animations", "Account.TourSeen", "Home.SetLanguage",
            "Notices.Accept", "Notices.SendCode", "Notices.Public"
        };

        public const string BeekeeperArea = "Me";
        public const string ConsumerArea = "Consumer";

        private readonly RequestDelegate _next;

        public BeekeeperBoundaryMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext http, TOP4HoneyChainsContext db, IMemoryCache cache)
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var endpoint = http.GetEndpoint();
            var action = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (userId != null && action != null && !IsPublic(endpoint!))
            {
                var name = action.ControllerName + "." + action.ActionName;
                if ((action.ControllerName != BeekeeperArea && !Open.Contains(name) && await IsBeekeeperAsync(userId, db, cache))
                    || (action.ControllerName != ConsumerArea && !ConsumerOpen.Contains(name) && await IsConsumerAsync(userId, db, cache)))
                {
                    http.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }
            await _next(http);
        }

        // every page without an [Authorize] (public pages)
        private static bool IsPublic(Endpoint endpoint) =>
            endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null || endpoint.Metadata.GetMetadata<IAuthorizeData>() == null;

        // the allowlists, for the tests that enumerate every action
        public static bool AllowedFor(string kind, string controller, string action, bool isPublic) =>
            isPublic || kind switch
            {
                "beekeeper" => controller == BeekeeperArea || Open.Contains(controller + "." + action),
                "consumer" => controller == ConsumerArea || ConsumerOpen.Contains(controller + "." + action),
                _ => true
            };

        public static Task<bool> IsBeekeeperAsync(string userId, TOP4HoneyChainsContext db, IMemoryCache cache) =>
            cache.GetOrCreateAsync("beekeeper-account:" + userId, e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return db.BeekeeperAccounts.AsNoTracking().AnyAsync(a => a.UserId == userId);
            });

        // D-87: a consumer account (any status: a locked consumer is still kept inside the boundary)
        public static Task<bool> IsConsumerAsync(string userId, TOP4HoneyChainsContext db, IMemoryCache cache) =>
            cache.GetOrCreateAsync("consumer-account:" + userId, e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return db.ConsumerAccounts.AsNoTracking().AnyAsync(a => a.UserId == userId);
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
