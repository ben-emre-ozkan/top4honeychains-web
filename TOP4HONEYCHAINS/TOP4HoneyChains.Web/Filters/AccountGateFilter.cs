using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Filters
{
    /// <summary>
    /// D-78 P1 (D78-SPEC 4.9): what a signed-in user must do before using the panel. With Notices:Enforce, a user who has not
    /// accepted the current notice(s) is sent to the notice page first; while it is off, a banner asks instead (O-14).
    /// Public pages and the pages of the gate itself are never redirected.
    /// </summary>
    public class AccountGateFilter : IAsyncActionFilter
    {
        // controller.action pairs that stay open while something is pending
        private static readonly HashSet<string> Open = new(StringComparer.OrdinalIgnoreCase)
        {
            "Account.Logout", "Account.Login", "Account.AccessDenied", "Home.SetLanguage",
            "Notices.Accept", "Notices.SendCode", "Notices.Public"
        };

        private readonly NoticeService _notices;

        public AccountGateFilter(NoticeService notices) => _notices = notices;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any() || IsOpen(context))
            {
                await next();
                return;
            }

            if (_notices.Options.Enforce && (await _notices.PendingForCurrentUserAsync(http, userId)).Count > 0)
            {
                context.Result = new RedirectResult("/Notices/Accept?returnUrl=" + Uri.EscapeDataString(http.Request.Path + http.Request.QueryString));
                return;
            }
            await next();
        }

        private static bool IsOpen(ActionExecutingContext context) =>
            context.ActionDescriptor is ControllerActionDescriptor a && Open.Contains(a.ControllerName + "." + a.ActionName);
    }
}
