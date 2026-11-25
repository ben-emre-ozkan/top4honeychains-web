using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-97 (migration 033): the seal pool and the permanent containers of the active organization.
    // - Everyone who records or reads deliveries sees the pool (Intake Read or Enter), the code states and the container pages.
    // - The organization administrator issues seal label batches, adds purchased plastic seal ranges and registers containers.
    // - Check and FindBox answer the delivery form while a code is typed or scanned (the server checks again on saving).
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class SealsController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly SealPoolService _pool;
        private readonly MessageTranslator _msg;

        public SealsController(TOP4HoneyChainsContext context, ICurrentOrg current, SealPoolService pool, MessageTranslator msg)
        {
            _context = context;
            _current = current;
            _pool = pool;
            _msg = msg;
        }

        private Services.OrgModel.Permissions Perm => HttpContext.RequestServices.GetRequiredService<Services.OrgModel.Permissions>();

        // the active membership when it may see the pool, else null
        private async Task<ActiveMembership?> ReaderAsync()
        {
            if (_current.IsPlatformAdmin)
                return null;
            var active = await _current.GetAsync();
            if (active == null || !active.IsVerified)
                return null;
            return active.OrgRole == OrgRoles.OrgAdmin
                   || Perm.Allows(PermissionAreas.Intake, PermissionAreas.Read, active.OrgRole)
                   || Perm.Allows(PermissionAreas.Intake, PermissionAreas.Enter, active.OrgRole)
                ? active : null;
        }

        private async Task<ActiveMembership?> AdminAsync() =>
            await ReaderAsync() is { OrgRole: OrgRoles.OrgAdmin } a ? a : null;

        public async Task<IActionResult> Index(string? status, string? kind, string? q)
        {
            var active = await ReaderAsync();
            if (active == null)
            {
                TempData["Error"] = "Only the organization administrator and the roles that record or read deliveries can open the seal pool.";
                return RedirectToAction("Index", "MyWork");
            }
            var codes = _context.SealCodes.AsNoTracking().Where(c => c.OrgId == active.OrgId);
            ViewBag.Counts = await codes.GroupBy(c => new { c.Kind, c.Status }).Select(g => new { g.Key.Kind, g.Key.Status, N = g.Count() })
                .ToDictionaryAsync(x => x.Kind + "|" + x.Status, x => x.N);
            if (SealStatus.All.Contains(status ?? ""))
                codes = codes.Where(c => c.Status == status);
            if (SealKind.All.Contains(kind ?? ""))
                codes = codes.Where(c => c.Kind == kind);
            if (SealPoolService.Normalize(q) is string term)
                codes = codes.Where(c => c.Code.Contains(term));
            ViewBag.Codes = await codes.Include(c => c.Container).Include(c => c.Sample)
                .OrderBy(c => c.Status == SealStatus.Issued ? 1 : 0).ThenByDescending(c => c.UsedAt).ThenBy(c => c.Code).Take(200).ToListAsync();
            ViewBag.Batches = await _context.SealBatches.AsNoTracking().Where(b => b.OrgId == active.OrgId).OrderByDescending(b => b.BatchId).Take(50).ToListAsync();
            ViewBag.Boxes = await BoxesAsync(active.OrgId);
            ViewBag.IsAdmin = active.OrgRole == OrgRoles.OrgAdmin;
            ViewBag.Status = status;
            ViewBag.Kind = kind;
            ViewBag.Q = q;
            ViewBag.Controller = "Intake";
            ViewBag.Action = "Seals";
            return View();
        }

        public record BoxRow(OrgContainer Box, int Uses, DateOnly? LastUse);

        private async Task<List<BoxRow>> BoxesAsync(int orgId)
        {
            var boxes = await _context.OrgContainers.AsNoTracking().Where(b => b.OrgId == orgId).OrderBy(b => b.Code).ToListAsync();
            var ids = boxes.Select(b => b.OrgContainerId).ToList();
            var uses = await _context.Containers.AsNoTracking().Where(c => c.OrgContainerId != null && ids.Contains(c.OrgContainerId.Value))
                .GroupBy(c => c.OrgContainerId!.Value).Select(g => new { Id = g.Key, N = g.Count(), Last = g.Max(c => c.Intake!.DeliveryDate) }).ToListAsync();
            return boxes.Select(b => new BoxRow(b, uses.FirstOrDefault(u => u.Id == b.OrgContainerId)?.N ?? 0, uses.FirstOrDefault(u => u.Id == b.OrgContainerId)?.Last)).ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(int count, string? note)
        {
            var active = await AdminAsync();
            if (active == null)
                return Forbid();
            if (count is < 1 or > SealPoolService.MaxBatch)
                TempData["Error"] = "Issue between 1 and 500 seal labels at a time.";
            else
            {
                var batch = await _pool.IssueLabelsAsync(active.OrgId, count, _current.UserId!, note);
                TempData["Success"] = string.Format(CultureInfo.InvariantCulture, "{0} seal labels issued: {1} to {2}. Print them on VOID label stock.", batch.CodeCount, batch.FirstCode, batch.LastCode);
                TempData["Scene"] = "label";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRange(string? prefix, string? from, string? to, string? note)
        {
            var active = await AdminAsync();
            if (active == null)
                return Forbid();
            var (batch, error) = await _pool.AddPlasticRangeAsync(active.OrgId, prefix, from, to, _current.UserId!, note);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = string.Format(CultureInfo.InvariantCulture, "{0} plastic seals added to the pool: {1} to {2}.", batch!.CodeCount, batch.FirstCode, batch.LastCode);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string? kind, string? name, decimal? capacityKg, decimal? tareKg)
        {
            var active = await AdminAsync();
            if (active == null)
                return Forbid();
            var (box, error) = await _pool.RegisterContainerAsync(active.OrgId, kind, name, capacityKg, tareKg, _current.UserId!);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = string.Format(CultureInfo.InvariantCulture, "Container {0} registered. Print its label and fix it to the container.", box!.Code);
            return RedirectToAction(nameof(Index), null, "containers");
        }

        // a batch of seal labels or the permanent containers' labels, printed from the browser (A4 sheet or 100 x 50 mm labels)
        public async Task<IActionResult> Print(int? batchId, string? what, string? size)
        {
            var active = await ReaderAsync();
            if (active == null)
                return NotFound();
            ViewBag.Size = size == "label" ? "label" : "a4";
            ViewBag.OrgName = active.OrgName;
            if (what == "containers")
            {
                ViewBag.Boxes = await _context.OrgContainers.AsNoTracking().Where(b => b.OrgId == active.OrgId && b.IsActive).OrderBy(b => b.Code).ToListAsync();
                return View();
            }
            var batch = await _context.SealBatches.AsNoTracking().Include(b => b.Codes).FirstOrDefaultAsync(b => b.BatchId == batchId && b.OrgId == active.OrgId);
            if (batch == null)
                return NotFound();
            ViewBag.Batch = batch;
            return View();
        }

        // one permanent container and every delivery it was used for
        public async Task<IActionResult> Box(int id)
        {
            var active = await ReaderAsync();
            if (active == null)
                return NotFound();
            var box = await _context.OrgContainers.AsNoTracking().FirstOrDefaultAsync(b => b.OrgContainerId == id && b.OrgId == active.OrgId);
            if (box == null)
                return NotFound();
            ViewBag.Uses = await _context.Containers.AsNoTracking().Where(c => c.OrgContainerId == id)
                .Include(c => c.Intake).Include(c => c.SealEvents).Include(c => c.Samples)
                .OrderByDescending(c => c.ContainerId).ToListAsync();
            ViewBag.Controller = "Intake";
            ViewBag.Action = "Seals";
            return View(box);
        }

        // ---------------------------------------------------------------- answers for the delivery form (JSON)

        // use: origin (sealed at harvest: the organization's pool or the beekeeper's association), jar, closing; closure: the
        // container's closure (the closing seal's kind follows it)
        public async Task<IActionResult> Check(string? code, string? use, string? closure, string? number)
        {
            var active = await ReaderAsync();
            if (active == null)
                return NotFound();
            var c = SealPoolService.Normalize(code);
            if (c == null)
                return Json(new { ok = false, code = "", message = "" });
            IReadOnlyCollection<int> pools = new[] { active.OrgId };
            string[]? kinds = null;
            if (use == "origin")
                pools = await _pool.OriginPoolsAsync(active.OrgId, await BeekeeperIdAsync(number));
            else if (use == "jar")
                kinds = new[] { SealKind.Label };
            else
                kinds = SealPoolService.ClosingKinds(closure);
            var refusal = await _pool.RefusalAsync(c, pools, kinds);
            var kind = await _context.SealCodes.AsNoTracking().Where(s => s.Code == c).Select(s => s.Kind).FirstOrDefaultAsync();
            return Json(new { ok = refusal == null, code = c, kind, message = refusal == null ? "" : _msg.T(refusal) });
        }

        public async Task<IActionResult> FindBox(string? code, string? number)
        {
            var active = await ReaderAsync();
            if (active == null)
                return NotFound();
            var pools = await _pool.OriginPoolsAsync(active.OrgId, await BeekeeperIdAsync(number));
            var (box, error) = await _pool.FindContainerAsync(code, pools);
            if (box == null)
                return Json(new { ok = false, message = _msg.T(error) });
            var l = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>>();
            return Json(new
            {
                ok = true, code = box.Code, kind = box.Kind, kindText = l["Ci.Kind." + box.Kind].Value, name = box.Name,
                tare = box.TareKg?.ToString("0.##", CultureInfo.InvariantCulture), capacity = box.CapacityKg?.ToString("0.##", CultureInfo.InvariantCulture)
            });
        }

        private async Task<string?> BeekeeperIdAsync(string? number)
        {
            number = number?.Trim();
            if (string.IsNullOrEmpty(number))
                return null;
            return await _context.Beekeepers.Where(b => b.BeekeeperBusinessNumber == number).Select(b => b.BeekeeperId).FirstOrDefaultAsync()
                   ?? await _context.BeekeeperRegistry.Where(r => r.RegistryNumber == number).Select(r => r.BeekeeperId).FirstOrDefaultAsync();
        }
    }
}
