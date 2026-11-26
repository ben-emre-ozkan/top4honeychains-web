using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Services.Messaging;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-112: the test SMS inbox of the live site's test set. Every SMS to a test number (TestPhones, +90 512 000 xx xx) stays on the
    // server, also with a real SMS provider, and is shown here with its full text, so a tester confirms a registration or a delivery
    // by SMS on the live site alone. Only a platform administrator and a signed-in test-set account may open it (404 for everyone
    // else); it only ever lists messages to test numbers. No-store, noindex.
    [Authorize(Policy = AuthPolicies.SignedIn)]
    public class TestInboxController : Controller
    {
        public record Row(long Id, DateTime At, string To, string? Person, string Purpose, string Text);

        private readonly TOP4HoneyChainsContext _context;
        private readonly SmsOptions _options;
        private readonly IMemoryCache _cache;

        public TestInboxController(TOP4HoneyChainsContext context, IOptions<SmsOptions> options, IMemoryCache cache)
        {
            _context = context;
            _options = options.Value;
            _cache = cache;
        }

        public static async Task<bool> MayOpenAsync(ClaimsPrincipal user, TOP4HoneyChainsContext db, IMemoryCache cache) =>
            user.IsInRole(CurrentOrg.AdminRole) || await TestData.IsTestAccountAsync(db, cache, user.FindFirstValue(ClaimTypes.NameIdentifier));

        // the phone hash of every test number back to the number (10 000 numbers, made once)
        private IReadOnlyDictionary<string, string> Numbers() =>
            _cache.GetOrCreate("test-inbox-numbers", e =>
            {
                e.Priority = CacheItemPriority.NeverRemove;
                var key = _options.KeyBytes();
                return TestPhones.All().ToDictionary(n => CodeRules.PhoneHash(key, n), n => n);
            })!;

        public async Task<IActionResult> Index(string? to)
        {
            if (!await MayOpenAsync(User, _context, _cache))
                return NotFound();
            Response.Headers.CacheControl = "no-store";
            Response.Headers["X-Robots-Tag"] = "noindex, nofollow";

            var numbers = Numbers();
            var filter = PhonePage.E164(to) is string e164 && TestPhones.IsTest(e164) ? e164 : null;
            var query = _context.SmsMessages.AsNoTracking().Where(m => m.Provider == TestPhones.Provider);
            if (filter != null)
            {
                var hash = CodeRules.PhoneHash(_options.KeyBytes(), filter);
                query = query.Where(m => m.ToPhoneHash == hash);
            }
            var messages = await query.OrderByDescending(m => m.SmsId).Take(200)
                .Select(m => new { m.SmsId, m.CreatedAt, m.ToPhoneHash, m.ToPhoneMasked, m.Purpose, m.BodyStored }).ToListAsync();

            // the people of the test numbers: accounts and beekeepers with a test number (the test set's people only)
            var people = new Dictionary<string, string>();
            foreach (var u in await _context.Database.SqlQuery<PhoneOwner>($"SELECT PhoneNumber AS Phone, UserName AS Name FROM dbo.AspNetUsers WHERE PhoneNumber LIKE {TestPhones.Prefix + "%"}").ToListAsync())
                people.TryAdd(u.Phone, u.Name);
            foreach (var b in await _context.Beekeepers.AsNoTracking().Where(b => b.BeekeeperPhoneNumber!.StartsWith(TestPhones.Prefix))
                         .Select(b => new { b.BeekeeperPhoneNumber, b.BeekeeperFirstName, b.BeekeeperLastName }).ToListAsync())
                people[b.BeekeeperPhoneNumber!] = $"{b.BeekeeperFirstName} {b.BeekeeperLastName}";

            var rows = messages.Select(m =>
            {
                var number = numbers.GetValueOrDefault(m.ToPhoneHash);
                return new Row(m.SmsId, m.CreatedAt, number != null ? PhonePage.Pretty(number) : m.ToPhoneMasked,
                    number != null ? people.GetValueOrDefault(number) : null, m.Purpose, m.BodyStored);
            }).ToList();
            ViewBag.Filter = filter == null ? null : PhonePage.Pretty(filter);
            ViewBag.FilterDigits = filter?.TrimStart('+');
            return View(rows);
        }

        public sealed record PhoneOwner(string Phone, string Name);
    }
}
