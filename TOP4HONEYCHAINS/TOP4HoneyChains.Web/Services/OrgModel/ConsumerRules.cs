using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-87: a consumer account is a separate account kind. It never holds a role, a membership or issued sign-in details;
    // every page that could give one refuses it with this message.
    public static class ConsumerRules
    {
        public const string NoRoleForConsumers = "A consumer account cannot be given a role or an organization membership.";

        public static Task<bool> IsConsumerAsync(TOP4HoneyChainsContext db, string? userId) =>
            userId == null ? Task.FromResult(false) : db.ConsumerAccounts.AsNoTracking().AnyAsync(a => a.UserId == userId);

        // the account kinds shown on the user list
        public const string Staff = "staff", Beekeeper = "beekeeper", Consumer = "consumer";
        public static readonly string[] Kinds = { Staff, Beekeeper, Consumer };
    }

    // D-87: a cookie that only says "a consumer signed in on this browser" (no id, readable by script). The Honey Story asks the
    // private state endpoint only when it is present, so anonymous visitors make no extra request. The session itself stays in
    // the HttpOnly authentication cookie; a stale marker only costs one request that answers "not signed in".
    public static class ConsumerMarker
    {
        public const string Name = "t4h.consumer";

        public static CookieOptions Options(bool secure) => new()
        {
            HttpOnly = false, Secure = secure, SameSite = SameSiteMode.Lax, Path = "/", IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(14)
        };
    }
}
