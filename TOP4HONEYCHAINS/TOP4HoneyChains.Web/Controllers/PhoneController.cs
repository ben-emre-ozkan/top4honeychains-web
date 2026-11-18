using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Services.Messaging;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-88: the phone page of the fake SMS provider (PhonePage). Every refusal is a plain 404: another provider, Production, a wrong
    // key or a malformed number. No sign-in, no menus; the key never leaves the page (no-referrer), nothing is cached or indexed.
    [AllowAnonymous]
    [Route("phone")]
    public class PhoneController : Controller
    {
        private readonly TOP4HoneyChainsContext? _context;
        private readonly SmsOptions _options;
        private readonly IWebHostEnvironment _env;
        private readonly PhonePageKey _key;

        public PhoneController(TOP4HoneyChainsContext? context, IOptions<SmsOptions> options, IWebHostEnvironment env, PhonePageKey key)
        {
            _context = context;
            _options = options.Value;
            _env = env;
            _key = key;
        }

        private string? Allowed(string? key, string? number) =>
            PhonePage.Available(_options, _env) && _key.Matches(key) ? PhonePage.E164(number) : null;

        private void Private()
        {
            Response.Headers.CacheControl = "no-store";
            Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
            Response.Headers["Referrer-Policy"] = "no-referrer";
        }

        [HttpGet("{key}/{number}")]
        public IActionResult Index(string key, string number)
        {
            if (Allowed(key, number) is not string e164)
                return NotFound();
            Private();
            ViewBag.Number = PhonePage.Pretty(e164);
            ViewBag.Sender = _options.Sender;
            return View();
        }

        // the newest 50 messages to the number, newest first
        [HttpGet("{key}/{number}/messages")]
        public async Task<IActionResult> Messages(string key, string number)
        {
            if (Allowed(key, number) is not string e164 || _context == null)
                return NotFound();
            Private();
            var hash = CodeRules.PhoneHash(_options.KeyBytes(), e164);
            var rows = await _context.SmsMessages.AsNoTracking().Where(m => m.ToPhoneHash == hash)
                .OrderByDescending(m => m.SmsId).Take(50)
                .Select(m => new { m.SmsId, m.CreatedAt, m.BodyStored }).ToListAsync();
            return Json(rows.Select(m => new { id = m.SmsId, at = DateTime.SpecifyKind(m.CreatedAt, DateTimeKind.Utc), text = m.BodyStored }));
        }
    }
}
