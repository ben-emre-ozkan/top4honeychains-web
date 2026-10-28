using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Filters
{
    /// <summary>
    /// D-78 P1 (D78-SPEC 4.3, 4.9): what a signed-in user must do before using the panel. First, a user with issued sign-in
    /// details changes the temporary password (always). Then, with Notices:Enforce, a user who has not accepted the current
    /// notice(s) is sent to the notice page; while it is off, a banner asks instead (O-14).
    /// Public pages and the pages of the gate itself are never redirected.
    /// </summary>
    public class AccountGateFilter : IAsyncActionFilter
    {
        // controller.action pairs that stay open while something is pending
        private static readonly HashSet<string> Open = new(StringComparer.OrdinalIgnoreCase)
        {
            "Account.Logout", "Account.Login", "Account.AccessDenied", "Home.SetLanguage",
            "Notices.Accept", "Notices.SendCode", "Notices.Public", "Account.FirstPassword",
            // D-86: on a demo site the codes for the notice confirmation are read in the demo SMS inbox
            "Demo.Inbox"
        };

        private readonly NoticeService _notices;
        private readonly CredentialService _credentials;

        public AccountGateFilter(NoticeService notices, CredentialService credentials)
        {
            _notices = notices;
            _credentials = credentials;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any() || IsOpen(context))
            {
                await next();
                return;
            }

            if (await _credentials.PendingAsync(userId) != null)
            {
                context.Result = new RedirectResult("/Account/FirstPassword");
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
