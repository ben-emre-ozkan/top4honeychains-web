using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.Messaging;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-86: the demo SMS inbox. Testers read the codes and sign-in details that the fake provider "sent" in their own flows:
    // the messages of their active organization and the ones they sent themselves; platform administrators read all.
    // Exists only on a demo site (404 otherwise).
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class DemoController : Controller
    {
        public record InboxRow(DateTime At, string Purpose, string ToMasked, string Text, string? OrgName);

        private readonly TOP4HoneyChainsContext _context;
        private readonly DemoMode _demo;
        private readonly SmsOptions _sms;
        private readonly ICurrentOrg _current;

        public DemoController(TOP4HoneyChainsContext context, DemoMode demo, IOptions<SmsOptions> sms, ICurrentOrg current)
        {
            _context = context;
            _demo = demo;
            _sms = sms.Value;
            _current = current;
        }

        public async Task<IActionResult> Inbox()
        {
            if (!_demo.Enabled || !_sms.IsFake)
                return NotFound();
            Response.Headers.CacheControl = "no-store";
            var userId = _current.UserId;
            var active = _current.IsPlatformAdmin ? null : await _current.GetAsync();
            var activeOrgId = active?.OrgId;
            var messages = _context.SmsMessages.AsNoTracking().Where(m => m.Provider == SmsProviders.Fake);
            if (!_current.IsPlatformAdmin)
                messages = messages.Where(m => m.SentByUserId == userId || (activeOrgId != null && m.OrgId == activeOrgId));
            var rows = await messages.OrderByDescending(m => m.SmsId).Take(100)
                .Select(m => new InboxRow(m.CreatedAt, m.Purpose, m.ToPhoneMasked, m.BodyStored,
                    _context.Organizations.Where(o => o.OrgId == m.OrgId).Select(o => o.DisplayName).FirstOrDefault()))
                .ToListAsync();
            ViewBag.OrgName = active?.OrgName;
            ViewBag.Controller = "Demo";
            return View(rows);
        }
    }
}
