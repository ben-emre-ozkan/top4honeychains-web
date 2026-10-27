using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using A = TOP4HoneyChains.Web.Models.Entities.PermissionAreas;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-56: the editable rights of organization roles outside the CCP enter/approve matrix (table RolePermissions,
    // seeded by 013 with the rules the code applied before). Cached; the permission screen invalidates the cache.
    // Before 013 is applied the built-in defaults below are used, which are the same as the seed.
    public class Permissions
    {
        private readonly IServiceScopeFactory _scopes;
        private volatile HashSet<(string Area, string Key, string Action, string Role)>? _set;

        public Permissions(IServiceScopeFactory scopes) => _scopes = scopes;

        private static readonly string[] AllRoles = OrgRoles.All;

        public static IEnumerable<(string Area, string Key, string Action, string Role)> Defaults(IEnumerable<int> ccpIds)
        {
            foreach (var id in ccpIds)
                foreach (var r in AllRoles)
                    yield return (A.Ccp, id.ToString(CultureInfo.InvariantCulture), A.Read, r);
            var rows = new (string, string, string[])[]
            {
                (A.Intake, A.Enter, new[] { OrgRoles.SampleAcceptanceOfficer }),
                (A.Intake, A.Read, new[] { OrgRoles.SampleAcceptanceOfficer, OrgRoles.OrgAdmin }),
                (A.Samples, A.Enter, new[] { OrgRoles.SampleAcceptanceOfficer }),
                (A.Samples, A.Approve, new[] { OrgRoles.Analyst, OrgRoles.LabManager }),
                (A.Samples, A.Read, new[] { OrgRoles.SampleAcceptanceOfficer, OrgRoles.OrgAdmin, OrgRoles.Analyst, OrgRoles.LabManager }),
                (A.LabResults, A.Enter, new[] { OrgRoles.Analyst }),
                (A.LabResults, A.Approve, new[] { OrgRoles.LabManager }),
                (A.LabResults, A.Read, AllRoles),
                (A.Shipments, A.Enter, new[] { OrgRoles.Staff, OrgRoles.OrgAdmin }),
                (A.Shipments, A.Approve, new[] { OrgRoles.Staff, OrgRoles.OrgAdmin }),
                (A.Shipments, A.Read, new[] { OrgRoles.Staff, OrgRoles.OrgAdmin, OrgRoles.QualityApprover }),
                (A.Storefront, A.Enter, new[] { OrgRoles.OrgAdmin }),
                (A.Storefront, A.Read, new[] { OrgRoles.OrgAdmin }),
                (A.Recall, A.Read, AllRoles),
                (A.Finalize, A.Enter, new[] { OrgRoles.Staff, OrgRoles.OrgAdmin }),
                (A.Finalize, A.Approve, new[] { OrgRoles.QualityApprover }),
                // D-78 (018)
                (A.Registration, A.Enter, new[] { OrgRoles.RegistrationOfficer }),
                (A.Registration, A.Read, new[] { OrgRoles.RegistrationOfficer }),
                (A.Evidence, A.Approve, new[] { OrgRoles.ComplianceOfficer })
            };
            foreach (var (area, action, roles) in rows)
                foreach (var r in roles)
                    yield return (area, "", action, r);
        }

        private HashSet<(string, string, string, string)> Set()
        {
            var set = _set;
            if (set != null)
                return set;
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
            try
            {
                set = db.RolePermissions.AsNoTracking().Select(p => new { p.Area, p.AreaKey, p.Action, p.OrgRole }).ToList()
                    .Select(p => (p.Area, p.AreaKey, p.Action, p.OrgRole)).ToHashSet();
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                set = new();
            }
            if (set.Count == 0)
                set = Defaults(db.CriticalControlPoints.Select(c => c.Id).ToList()).ToHashSet();
            _set = set;
            return set;
        }

        public void Invalidate() => _set = null;

        public bool Allows(string area, string action, string? role, string key = "") =>
            role != null && Set().Contains((area, key, action, role));

        public bool CcpRead(int ccpId, string? role) => Allows(A.Ccp, A.Read, role, ccpId.ToString(CultureInfo.InvariantCulture));

        // roles holding a right, for messages and lists
        public string[] Roles(string area, string action, string key = "") =>
            AllRoles.Where(r => Set().Contains((area, key, action, r))).ToArray();
    }
}
