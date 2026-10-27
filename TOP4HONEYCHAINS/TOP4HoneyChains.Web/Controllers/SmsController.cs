using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Services.Messaging;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P1 (D78-SPEC 5.3): the outbox of the fake SMS provider, for development and demonstrations. It exists only while
    // Sms:Provider is Fake (404 otherwise); a real provider never stores codes or passwords in clear.
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class SmsController : Controller
    {
        public record OutboxRow(DateTime At, string Purpose, string ToMasked, string? ToFull, string Language, string Text, int Segments, string Status,
            string? SubjectType, string? SubjectKey);

        private readonly TOP4HoneyChainsContext _context;
        private readonly SmsOptions _options;
        private readonly FakeSmsStore _store;

        public SmsController(TOP4HoneyChainsContext context, IOptions<SmsOptions> options, FakeSmsStore store)
        {
            _context = context;
            _options = options.Value;
            _store = store;
        }

        public async Task<IActionResult> Outbox()
        {
            if (!_options.IsFake)
                return NotFound();
            var messages = await _context.SmsMessages.AsNoTracking().OrderByDescending(m => m.SmsId).Take(200).ToListAsync();
            var rows = messages.Select(m => new OutboxRow(m.CreatedAt, m.Purpose, m.ToPhoneMasked,
                m.ProviderMessageId == null ? null : _store.Find(m.ProviderMessageId)?.To, m.Language, m.BodyStored, m.Segments, m.Status,
                m.SubjectType, m.SubjectKey)).ToList();
            ViewBag.Today = await _context.SmsMessages.Where(m => m.CreatedAt >= DateTime.UtcNow.Date).SumAsync(m => (int?)m.Segments) ?? 0;
            ViewBag.DailyLimit = _options.DailyLimit;
            ViewBag.Controller = "Sms";
            return View(rows);
        }
    }
}
