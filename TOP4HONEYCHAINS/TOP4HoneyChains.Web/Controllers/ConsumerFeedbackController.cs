using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-87: consumer feedback about the organization's batches. Members whose role holds "Consumer feedback" Read see the list and
    // the messages; Enter marks them read, answers and closes them. The platform administrator reads the reports of a suspected fake
    // or opened jar (read-only). Nobody sees more of the consumer than the pseudonym; audit entries carry ids only.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class ConsumerFeedbackController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly Permissions _perm;
        private readonly AuditWriter _audit;

        public ConsumerFeedbackController(TOP4HoneyChainsContext context, ICurrentOrg current, Permissions perm, AuditWriter audit)
        {
            _context = context;
            _current = current;
            _perm = perm;
            _audit = audit;
        }

        private record Access(int? OrgId, bool CanAnswer, bool Admin);

        // the organization whose feedback the user reads, or the administrator's view of suspected fakes; null = no access
        private async Task<Access?> AccessAsync()
        {
            if (!HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptionsSnapshot<FeatureFlags>>().Value.ConsumerAccounts)
                return null;
            if (_current.IsPlatformAdmin)
                return new Access(null, false, true);
            if (await _current.GetAsync() is not { } m || !m.IsVerified || !_perm.Allows(PermissionAreas.ConsumerFeedback, PermissionAreas.Read, m.OrgRole))
                return null;
            return new Access(m.OrgId, _perm.Allows(PermissionAreas.ConsumerFeedback, PermissionAreas.Enter, m.OrgRole), false);
        }

        private IQueryable<ConsumerFeedback> Scope(Access access) => access.Admin
            ? _context.ConsumerFeedback.Where(f => f.Category == ConsumerFeedbackCategory.SuspectedFake)
            : _context.ConsumerFeedback.Where(f => f.OrgId == access.OrgId);

        public async Task<IActionResult> Index(string? status)
        {
            if (await AccessAsync() is not { } access)
                return NotFound();
            var query = Scope(access).AsNoTracking().Include(f => f.Batch).Include(f => f.Org).AsQueryable();
            if (status != null && ConsumerFeedbackStatus.All.Contains(status))
                query = query.Where(f => f.Status == status);
            ViewBag.Status = status;
            ViewBag.Admin = access.Admin;
            ViewBag.Controller = "ConsumerFeedback";
            return View(await query.OrderBy(f => f.Status == ConsumerFeedbackStatus.Closed).ThenByDescending(f => f.CreatedAt).Take(500).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            if (await AccessAsync() is not { } access)
                return NotFound();
            var feedback = await Scope(access).AsNoTracking().Include(f => f.Batch).Include(f => f.Org).FirstOrDefaultAsync(f => f.FeedbackId == id);
            if (feedback == null)
                return NotFound();
            ViewBag.CanAnswer = access.CanAnswer;
            ViewBag.Admin = access.Admin;
            ViewBag.Controller = "ConsumerFeedback";
            return View(feedback);
        }

        private async Task<(ConsumerFeedback? Feedback, IActionResult? Refused)> ForAnswerAsync(int id)
        {
            if (await AccessAsync() is not { CanAnswer: true } access)
                return (null, NotFound());
            var feedback = await Scope(access).FirstOrDefaultAsync(f => f.FeedbackId == id);
            return feedback == null ? (null, NotFound()) : (feedback, null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var (feedback, refused) = await ForAnswerAsync(id);
            if (refused != null)
                return refused;
            if (feedback!.Status == ConsumerFeedbackStatus.New)
            {
                feedback.Status = ConsumerFeedbackStatus.Read;
                feedback.ReadAt = DateTime.UtcNow;
                feedback.ReadByUserId = _current.UserId;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Answer(int id, string? answer)
        {
            var (feedback, refused) = await ForAnswerAsync(id);
            if (refused != null)
                return refused;
            answer = answer?.Trim();
            if (string.IsNullOrEmpty(answer) || answer.Length > ConsumerService.MessageLimit)
                TempData["Error"] = "Write an answer of at most 1000 characters.";
            else if (feedback!.Status == ConsumerFeedbackStatus.Closed)
                TempData["Error"] = "This feedback is closed.";
            else
            {
                var now = DateTime.UtcNow;
                feedback.Answer = answer;
                feedback.AnsweredAt = now;
                feedback.AnsweredByUserId = _current.UserId;
                feedback.ReadAt ??= now;
                feedback.ReadByUserId ??= _current.UserId;
                feedback.Status = ConsumerFeedbackStatus.Answered;
                await _context.SaveChangesAsync();
                await _audit.WriteAsync("Answer", "ConsumerFeedback", id.ToString(System.Globalization.CultureInfo.InvariantCulture), null,
                    new { FeedbackId = id, feedback.OrgId, Status = feedback.Status });
                TempData["Success"] = "The answer was sent to the consumer.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var (feedback, refused) = await ForAnswerAsync(id);
            if (refused != null)
                return refused;
            if (feedback!.Status != ConsumerFeedbackStatus.Closed)
            {
                var old = feedback.Status;
                feedback.Status = ConsumerFeedbackStatus.Closed;
                feedback.ClosedAt = DateTime.UtcNow;
                feedback.ClosedByUserId = _current.UserId;
                await _context.SaveChangesAsync();
                await _audit.WriteAsync("Close", "ConsumerFeedback", id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    new { FeedbackId = id, Status = old }, new { FeedbackId = id, feedback.OrgId, Status = feedback.Status });
                TempData["Success"] = "The feedback was closed.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
