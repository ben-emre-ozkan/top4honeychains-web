using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-98 (migration 036): "Empty into this batch" on the batch page (the section is the BatchContainers view component). The
    // closing seal is scanned or typed and confirmed intact; a different code or a broken seal stops the step and is recorded.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class EmptyingController : Controller
    {
        private readonly ICurrentOrg _current;
        private readonly CcpResponsibilityService _resp;
        private readonly ContainerEmptyingService _emptying;
        private readonly Services.OrgModel.Permissions _perm;

        public EmptyingController(ICurrentOrg current, CcpResponsibilityService resp, ContainerEmptyingService emptying, Services.OrgModel.Permissions perm)
        {
            _current = current;
            _resp = resp;
            _emptying = emptying;
            _perm = perm;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Empty(int batchId, int containerId, string? seal, bool intact = false, bool broken = false)
        {
            var active = await _current.GetAsync();
            var owner = await _resp.BatchOwnerAsync(batchId);
            if (owner == null || active == null || active.OrgId != owner)
                return NotFound();
            if (ContainerEmptyingService.Denied(active, owner, _perm) is string denied)
                TempData["Error"] = denied;
            else
            {
                var result = await _emptying.EmptyAsync(batchId, containerId, seal, intact, broken, _current.UserId!);
                TempData[result.Emptied ? "Success" : "Error"] = result.Message;
            }
            return Redirect(Url.Action("Batch", "OrgWorkspace", new { id = batchId }) + "#batch-containers");
        }
    }
}
