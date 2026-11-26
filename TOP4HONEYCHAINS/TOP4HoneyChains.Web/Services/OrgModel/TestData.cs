using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TOP4HoneyChains.Web.Data;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-112: what belongs to the live site's test set (create-test-set). Test data is marked by a flag, never by a name:
    // Organizations.IsTestData, RawHoneyProductionPeriods.IsTestData and the rows of TestSetAccounts. Everything else follows from
    // them: a batch of a test organization, a beekeeper registered only by test organizations, a laboratory report of a test laboratory.
    public static class TestData
    {
        private const string PageItem = "D112.TestData";

        // the page shows records of the test set (the TEST banner, TestSetBannerTagHelper)
        public static void MarkPage(HttpContext http) => http.Items[PageItem] = true;
        public static bool PageMarked(HttpContext http) => http.Items.ContainsKey(PageItem);

        public static Task<bool> IsTestOrgAsync(TOP4HoneyChainsContext db, int? orgId) =>
            orgId == null ? Task.FromResult(false) : db.Organizations.AnyAsync(o => o.OrgId == orgId && o.IsTestData);

        public static Task<bool> IsTestBatchAsync(TOP4HoneyChainsContext db, int batchId) =>
            db.FinalHoneyProductions.AnyAsync(f => f.FinalHoneyProductionId == batchId && db.Organizations.Any(o => o.OrgId == f.OwnerOrgId && o.IsTestData));

        // registered, and only by organizations of the test set
        public static async Task<bool> IsTestBeekeeperAsync(TOP4HoneyChainsContext db, string? beekeeperId) =>
            beekeeperId != null
            && await db.BeekeeperRegistrations.AnyAsync(r => r.BeekeeperId == beekeeperId)
            && !await db.BeekeeperRegistrations.AnyAsync(r => r.BeekeeperId == beekeeperId && !db.Organizations.Any(o => o.OrgId == r.RegisteringOrgId && o.IsTestData));

        // an account of the test set: made by create-test-set (or signed up from a test Honey Story), or a member of a test organization
        // a consumer who signs up from the Honey Story of a test batch (the qr in the return address) or with a test number
        public static async Task MarkConsumerAsync(TOP4HoneyChainsContext db, string userId, string? phone, string? returnUrl)
        {
            var test = Messaging.TestPhones.IsTest(phone);
            if (!test && returnUrl != null
                && System.Text.RegularExpressions.Regex.Match(returnUrl, @"(?:qr=|/honeystory/)([0-9a-fA-F-]{36})") is { Success: true } m
                && Guid.TryParse(m.Groups[1].Value, out var qr))
                test = await db.FinalHoneyProductions.AnyAsync(f => f.FinalHoneyProductionQR == qr && db.Organizations.Any(o => o.OrgId == f.OwnerOrgId && o.IsTestData));
            if (!test || await db.TestSetAccounts.AnyAsync(a => a.UserId == userId))
                return;
            db.TestSetAccounts.Add(new Models.Entities.TestSetAccount { UserId = userId, Kind = Models.Entities.TestSetAccountKind.Consumer, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        public static async Task<bool> IsTestAccountAsync(TOP4HoneyChainsContext db, IMemoryCache cache, string? userId)
        {
            if (userId == null)
                return false;
            return await cache.GetOrCreateAsync("test-set-account:" + userId, async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return await db.TestSetAccounts.AsNoTracking().AnyAsync(a => a.UserId == userId)
                       || await db.Memberships.AsNoTracking().AnyAsync(m => m.UserId == userId && m.Status == Models.Entities.MembershipStatus.Active
                                                                            && db.Organizations.Any(o => o.OrgId == m.OrgId && o.IsTestData));
            });
        }
    }
}
