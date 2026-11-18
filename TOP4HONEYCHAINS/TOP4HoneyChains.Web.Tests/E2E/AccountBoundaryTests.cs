using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-87: the fallback policy only requires a signed-in user, so the account-kind boundary is what keeps beekeeper and consumer
    // accounts away from the staff pages. Every controller action the app maps is requested (GET or POST, as declared) by a
    // consumer and by a beekeeper account; outside the explicit allowlists below, and outside the public pages, the answer must be
    // 403. A new action is covered automatically; widening an allowlist needs a change here.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class AccountBoundaryTests
    {
        private readonly E2EApp _app;

        public AccountBoundaryTests(E2EApp app) => _app = app;

        private static readonly HashSet<string> ConsumerAllowed = new(StringComparer.OrdinalIgnoreCase)
        {
            "Account.Logout", "Account.AccessDenied", "Account.Animations", "Account.TourSeen", "Home.SetLanguage",
            "Notices.Accept", "Notices.SendCode", "Notices.Public"
        };

        private static readonly HashSet<string> BeekeeperAllowed = new(StringComparer.OrdinalIgnoreCase)
        {
            "Account.Logout", "Account.FirstPassword", "Account.AccessDenied", "Account.Animations", "Account.TourSeen", "Home.SetLanguage",
            "Notices.Accept", "Notices.SendCode", "Notices.Public"
        };

        public record Probe(string Controller, string Action, string Method, string Url, bool IsPublic);

        // every mapped controller action with one concrete URL per HTTP method
        private List<Probe> Probes()
        {
            var probes = new List<Probe>();
            foreach (var endpoint in _app.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>())
            {
                if (endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not { } action)
                    continue;
                var isPublic = endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null || endpoint.Metadata.GetMetadata<IAuthorizeData>() == null;
                var url = action.AttributeRouteInfo?.Template is string template ? "/" + Fill(endpoint.RoutePattern) : $"/{action.ControllerName}/{action.ActionName}";
                var methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? new[] { "GET" };
                foreach (var method in methods.Where(m => m is "GET" or "POST").DefaultIfEmpty("GET"))
                    probes.Add(new Probe(action.ControllerName, action.ActionName, method, url, isPublic));
            }
            return probes.DistinctBy(p => (p.Method, p.Url)).ToList();
        }

        // route values that satisfy the constraints, so the request reaches routing's match (not a 404)
        private static string Fill(RoutePattern pattern) =>
            string.Join("/", pattern.PathSegments.Select(s => string.Concat(s.Parts.Select(part => part switch
            {
                RoutePatternLiteralPart l => l.Content,
                RoutePatternSeparatorPart sep => sep.Content,
                RoutePatternParameterPart p => p.ParameterPolicies.Any(x => x.Content?.Contains("guid") == true) || p.Name.Contains("qr", StringComparison.OrdinalIgnoreCase)
                    ? Guid.NewGuid().ToString() : "1",
                _ => ""
            }))));

        // POSTs carry a valid anti-forgery token of the signed-in user (taken from a page they may open), so they reach the boundary
        // like a real form would; without a token they would already be refused with 400
        private async Task<List<string>> NotBlockedAsync(UserClient client, HashSet<string> allowed, string area, string tokenFrom)
        {
            var wrong = new List<string>();
            foreach (var probe in Probes().Where(p => !p.IsPublic && p.Controller != area && !allowed.Contains(p.Controller + "." + p.Action)))
            {
                var response = probe.Method == "POST" ? await client.PostAsync(probe.Url, new(), tokenFrom) : await client.GetAsync(probe.Url);
                if (response.StatusCode != HttpStatusCode.Forbidden)
                    wrong.Add($"{probe.Method} {probe.Url} -> {(int)response.StatusCode}");
            }
            return wrong;
        }

        private async Task<T> WithFlagsAsync<T>(Func<Task<T>> work)
        {
            var config = _app.Services.GetRequiredService<IConfiguration>();
            config["Features:ConsumerAccounts"] = "true";
            config["Features:BeekeeperAccounts"] = "true";
            try { return await work(); }
            finally
            {
                config["Features:ConsumerAccounts"] = "false";
                config["Features:BeekeeperAccounts"] = "false";
            }
        }

        public static async Task<string> CreateConsumerAsync(E2EApp app, string userName)
        {
            var userId = await app.CreateUserAsync(userName);
            await app.WithContextAsync(async db =>
            {
                db.ConsumerAccounts.Add(new ConsumerAccount
                {
                    UserId = userId, Pseudonym = "C-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(), Language = "en",
                    AgeConfirmedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow
                });
                return await db.SaveChangesAsync();
            });
            return userId;
        }

        [Fact]
        public async Task A_consumer_account_gets_403_on_every_action_outside_its_area_and_the_public_pages() => await WithFlagsAsync(async () =>
        {
            await CreateConsumerAsync(_app, "zz_test_bnd_consumer");
            var consumer = await UserClient.LoginAsync(_app, "zz_test_bnd_consumer");
            var probes = Probes();
            Assert.True(probes.Count > 150, $"only {probes.Count} actions found");
            var wrong = await NotBlockedAsync(consumer, ConsumerAllowed, "Consumer", "/");
            Assert.True(wrong.Count == 0, "not blocked for a consumer: " + string.Join("; ", wrong));
            // the public pages stay open
            Assert.Equal(200, await consumer.StatusAsync("/"));
            return 0;
        });

        [Fact]
        public async Task A_beekeeper_account_gets_403_on_every_action_outside_its_portal_and_the_public_pages() => await WithFlagsAsync(async () =>
        {
            var userId = await _app.CreateUserAsync("zz_test_bnd_bee");
            await _app.WithContextAsync(async db =>
            {
                var beekeeperId = Guid.NewGuid().ToString();
                db.Beekeepers.Add(new Beekeeper
                {
                    BeekeeperId = beekeeperId, BeekeeperFirstName = "zz_test", BeekeeperLastName = "Boundary", BeekeeperBirthDate = new DateOnly(1980, 1, 1),
                    BeekeeperBusinessNumber = "ZZ-BND-0001", IsActive = true
                });
                db.BeekeeperAccounts.Add(new BeekeeperAccount { BeekeeperId = beekeeperId, UserId = userId, LinkedAt = DateTime.UtcNow, LinkedByUserId = "seed", Status = BeekeeperAccountStatus.Active });
                return await db.SaveChangesAsync();
            });
            var beekeeper = await UserClient.LoginAsync(_app, "zz_test_bnd_bee");
            var wrong = await NotBlockedAsync(beekeeper, BeekeeperAllowed, "Me", "/Me/Account");
            Assert.True(wrong.Count == 0, "not blocked for a beekeeper: " + string.Join("; ", wrong));
            Assert.Equal(200, await beekeeper.StatusAsync("/"));
            return 0;
        });

        [Fact]
        public async Task Consumer_accounts_cannot_sign_in_while_the_switch_is_off()
        {
            await CreateConsumerAsync(_app, "zz_test_bnd_off");
            var (_, response) = await UserClient.SignInAsync(_app, "zz_test_bnd_off", E2EApp.Password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);   // the sign-in page again, with the message
            Assert.DoesNotContain("/Consumer", response.Headers.Location?.ToString() ?? "");
        }
    }
}
