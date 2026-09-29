using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TOP4HoneyChains.Web.Filters
{
    /// <summary>
    /// Turns database constraint violations raised by SaveChanges (FK, CHECK, unique)
    /// into a friendly message on the previous page instead of an HTTP 500.
    /// Any other exception is left to the global exception handler.
    /// </summary>
    public class DbUpdateExceptionFilter : IExceptionFilter
    {
        public const string TempDataKey = "DbUpdateError";

        // SQL Server error numbers
        private const int ConstraintViolation = 547;          // FK or CHECK constraint
        private static readonly int[] DuplicateKey = { 2601, 2627 }; // unique index / constraint

        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ILogger<DbUpdateExceptionFilter> _logger;

        public DbUpdateExceptionFilter(
            ITempDataDictionaryFactory tempDataFactory,
            IStringLocalizer<SharedResource> localizer,
            ILogger<DbUpdateExceptionFilter> logger)
        {
            _tempDataFactory = tempDataFactory;
            _localizer = localizer;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not DbUpdateException { InnerException: SqlException sql })
                return;

            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString() ?? "";
            var isDelete = action.StartsWith("Delete", StringComparison.OrdinalIgnoreCase);

            string messageKey;
            if (sql.Number == ConstraintViolation)
                messageKey = isDelete ? "Error.RecordInUse" : "Error.InvalidReference";
            else if (DuplicateKey.Contains(sql.Number))
                messageKey = "Error.DuplicateRecord";
            else
                return;

            _logger.LogWarning(context.Exception,
                "Database constraint violation {SqlErrorNumber} in {Controller}.{Action}",
                sql.Number, controller, action);

            _tempDataFactory.GetTempData(context.HttpContext)[TempDataKey] = _localizer[messageKey].Value;
            context.Result = GetReturnResult(context.HttpContext.Request, controller);
            context.ExceptionHandled = true;
        }

        // Back to the page the form was posted from (same host only), else the controller's list
        private static IActionResult GetReturnResult(HttpRequest request, string? controller)
        {
            var referer = request.Headers.Referer.ToString();
            if (Uri.TryCreate(referer, UriKind.Absolute, out var uri)
                && string.Equals(uri.Authority, request.Host.Value, StringComparison.OrdinalIgnoreCase))
            {
                return new LocalRedirectResult(uri.PathAndQuery);
            }

            return new RedirectToActionResult("Index", controller, null);
        }
    }
}
