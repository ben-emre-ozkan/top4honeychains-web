using Microsoft.AspNetCore.Mvc;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.ViewComponents
{
    // D-98: the containers of the batch's lots on the batch page, each still sealed until it is emptied into the batch after its
    // closing seal was checked (Views/Shared/Components/BatchContainers/Default.cshtml; the form posts to EmptyingController)
    public class BatchContainersViewComponent : ViewComponent
    {
        public record Model(int BatchId, bool Finalized, bool CanEmpty, IReadOnlyList<ContainerEmptyingService.Row> Rows);

        private readonly ContainerEmptyingService _emptying;
        private readonly ICurrentOrg _current;
        private readonly CcpResponsibilityService _resp;
        private readonly Permissions _perm;

        public BatchContainersViewComponent(ContainerEmptyingService emptying, ICurrentOrg current, CcpResponsibilityService resp, Permissions perm)
        {
            _emptying = emptying;
            _current = current;
            _resp = resp;
            _perm = perm;
        }

        public async Task<IViewComponentResult> InvokeAsync(int batchId, bool finalized)
        {
            var rows = await _emptying.ForBatchAsync(batchId);
            var canEmpty = !finalized && ContainerEmptyingService.Denied(await _current.GetAsync(), await _resp.BatchOwnerAsync(batchId), _perm) == null;
            return View(new Model(batchId, finalized, canEmpty, rows));
        }
    }
}
