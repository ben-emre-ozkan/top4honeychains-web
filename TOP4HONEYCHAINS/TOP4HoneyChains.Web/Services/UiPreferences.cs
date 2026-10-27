using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Services
{
    // D-78 P4 (D78-SPEC 7.2, D-83): the user's "Show animations" switch (UserSettings.ShowAnimations, default on) and the first-use
    // tour of the active role. Read once per request.
    public class UiPreferences
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly IHttpContextAccessor _http;
        private bool? _show;

        public UiPreferences(TOP4HoneyChainsContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        private string? UserId => _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<bool> ShowAnimationsAsync()
        {
            if (_show != null)
                return _show.Value;
            var id = UserId;
            _show = id == null || await _context.UserSettings.AsNoTracking().Where(s => s.UserId == id).Select(s => (bool?)s.ShowAnimations).FirstOrDefaultAsync() != false;
            return _show.Value;
        }

        public async Task SetShowAnimationsAsync(string userId, bool show)
        {
            var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null)
                _context.UserSettings.Add(settings = new UserSetting { UserId = userId });
            settings.ShowAnimations = show;
            await _context.SaveChangesAsync();
            _show = show;
        }

        // an opaque key for the browser-side "seen" marks (scenes, tour): not the user id itself
        public string? UserKey => UserId is string id ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("ui|" + id)))[..16].ToLowerInvariant() : null;

        // ---------------------------------------------------------------- first-use tour (7.1): 3-5 steps per role

        // Target: the data-tour key of a menu item (never a URL, so the page names no entry the role does not have)
        public record TourStep(string Target, string TextKey);

        public static List<TourStep> TourFor(string? orgRole, bool admin, bool beekeeper)
        {
            if (beekeeper)
                return new()
                {
                    new("me-deliveries", "Tour.Me.Deliveries"), new("me-samples", "Tour.Me.Samples"),
                    new("me-privacy", "Tour.Me.Privacy"), new("me-account", "Tour.Me.Account")
                };
            var steps = new List<TourStep>();
            if (admin)
            {
                steps.Add(new("dashboard", "Tour.Admin.Dashboard"));
                steps.Add(new("data", "Tour.Admin.Data"));
            }
            else
            {
                steps.Add(new("work", "Tour.Work"));
                steps.Add(new("org", "Tour.Org"));
                switch (orgRole)
                {
                    case OrgRoles.SampleAcceptanceOfficer:
                        steps.Add(new("intake", "Tour.Intake"));
                        steps.Add(new("samples", "Tour.Samples"));
                        break;
                    case OrgRoles.Analyst:
                    case OrgRoles.LabManager:
                        steps.Add(new("samples", "Tour.Lab"));
                        break;
                    case OrgRoles.RegistrationOfficer:
                        steps.Add(new("registration", "Tour.Registration"));
                        break;
                    case OrgRoles.ComplianceOfficer:
                        steps.Add(new("compliance", "Tour.Compliance"));
                        break;
                    case OrgRoles.OrgAdmin:
                    case OrgRoles.Staff:
                        steps.Add(new("shipments", "Tour.Shipments"));
                        break;
                    default:   // FieldInspector, QualityApprover
                        steps.Add(new("work", "Tour.Ccp"));
                        break;
                }
            }
            steps.Add(new("user", "Tour.Menu"));
            return steps;
        }
    }
}
