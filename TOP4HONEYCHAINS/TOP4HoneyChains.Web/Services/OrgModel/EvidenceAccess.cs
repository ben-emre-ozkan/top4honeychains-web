using Microsoft.AspNetCore.Mvc;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P5 (D78-SPEC 6.4, C2, D-84): who downloads evidence documents (signed delivery forms, registration and consent
    // evidence, organization documents). The organization that holds the record: the recording role, its Compliance Officer and
    // its organization admin; and platform administrators. Another role of that organization gets 403; another organization 404
    // (its record is not revealed). Every download is an audit entry.
    public class EvidenceAccess
    {
        public enum Result { Allowed, Forbidden, NotFound }

        private readonly ICurrentOrg _current;
        private readonly AuditWriter _audit;

        public EvidenceAccess(ICurrentOrg current, AuditWriter audit)
        {
            _current = current;
            _audit = audit;
        }

        public async Task<Result> CheckAsync(int? holderOrgId, params string[] recordingRoles)
        {
            if (_current.IsPlatformAdmin)
                return Result.Allowed;
            var active = await _current.GetAsync();
            if (active == null || holderOrgId == null || active.OrgId != holderOrgId)
                return Result.NotFound;
            return active.OrgRole is OrgRoles.OrgAdmin or OrgRoles.ComplianceOfficer || recordingRoles.Contains(active.OrgRole)
                ? Result.Allowed : Result.Forbidden;
        }

        public Task AuditAsync(string entity, string key, string? file) =>
            _audit.WriteAsync("Download", entity, key, null, new { File = file });

        // the answer for a refused download
        public static IActionResult Refuse(Result result) =>
            result == Result.Forbidden ? new StatusCodeResult(StatusCodes.Status403Forbidden) : new NotFoundResult();
    }
}
