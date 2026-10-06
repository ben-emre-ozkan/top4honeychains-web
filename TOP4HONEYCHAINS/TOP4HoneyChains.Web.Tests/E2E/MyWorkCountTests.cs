using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-66: GET /MyWork/Count gives the numbers of My work for the active membership, for the menu badge
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class MyWorkCountTests
    {
        private readonly E2EApp _app;

        public MyWorkCountTests(E2EApp app) => _app = app;

        // a receiver with two shipments to confirm (Staff), and an association membership with nothing to do
        private async Task<(string User, int Receiving, int Association)> SeedAsync()
        {
            var sender = await _app.CreateOrgAsync("CT Sender", null, (Capabilities.Processing, null), (Capabilities.Packing, null), (Capabilities.Wholesale, null));
            var receiver = await _app.CreateOrgAsync("CT Receiver", null, (Capabilities.Wholesale, null));
            var association = await _app.CreateOrgAsync("CT Association", null, (Capabilities.Association, null));
            await _app.WithContextAsync(async db =>
            {
                var wholesaler = new Wholesaler { WholesalerTitle = "CT Wholesaler", WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(wholesaler);
                await db.SaveChangesAsync();
                var batch = new FinalHoneyProduction
                {
                    FinalHoneyProductionTitle = "CT Batch", FinalHoneyProductionDate = DateTime.Today, FinalHoneyProductionTotalAmount = 50, FinalHoneyProductionIsFinalized = true,
                    FinalHoneyProductionQR = Guid.NewGuid(), OwnerOrgId = sender, FinalHoneyProductionWholesalerId = wholesaler.WholesalerId
                };
                db.FinalHoneyProductions.Add(batch);
                await db.SaveChangesAsync();
                for (var i = 0; i < 2; i++)
                    db.Shipments.Add(new Shipment { FinalHoneyProductionId = batch.FinalHoneyProductionId, FromOrgId = sender, ToOrgId = receiver, Quantity = 5, ShippedByUserId = "seed", ShippedAt = DateTime.UtcNow });
                return await db.SaveChangesAsync();
            });
            var user = await _app.CreateUserAsync("e2e_count_user");
            await _app.AddMemberAsync(receiver, user, OrgRoles.Staff);
            await _app.AddMemberAsync(association, user, OrgRoles.OrgAdmin);
            var memberships = await _app.WithContextAsync(db => db.Memberships.Where(m => m.UserId == user).ToDictionaryAsync(m => m.OrgId, m => m.MembershipId));
            return ("e2e_count_user", memberships[receiver], memberships[association]);
        }

        private static async Task<(int Total, Dictionary<string, int> ByKind)> CountAsync(UserClient user)
        {
            var response = await user.GetAsync("/MyWork/Count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("no-store", response.Headers.CacheControl?.ToString() ?? "");
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var byKind = json.RootElement.GetProperty("byKind").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetInt32());
            return (json.RootElement.GetProperty("total").GetInt32(), byKind);
        }

        // the page's groups and items, as the count sees them
        private static Dictionary<string, int> PageCounts(string html) =>
            Regex.Matches(html, @"<section[^>]*class=""[^""]*\bwork-group\b[^""]*""[^>]*data-kind=""(\w+)""[^>]*>(.*?)</section>", RegexOptions.Singleline)
                .ToDictionary(m => m.Groups[1].Value, m => Regex.Matches(m.Groups[2].Value, @"class=""(?:[^""]*\s)?work-item(?=[\s""])").Count);

        [Fact]
        public async Task The_count_needs_sign_in()
        {
            var client = _app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var response = await client.GetAsync("/MyWork/Count");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location?.ToString() ?? "");
        }

        [Fact]
        public async Task The_count_follows_the_active_membership_and_matches_the_page()
        {
            var (name, receiving, association) = await SeedAsync();
            var user = await UserClient.LoginAsync(_app, name);

            await user.PostAsync("/OrgWorkspace/Switch", new() { ["membershipId"] = receiving.ToString() }, "/MyWork/Index");
            var (total, byKind) = await CountAsync(user);
            Assert.Equal(2, total);
            Assert.Equal(2, byKind["Confirm"]);
            var page = PageCounts(await user.HtmlAsync("/MyWork/Index"));
            Assert.Equal(byKind, page);
            Assert.Equal(total, page.Values.Sum());

            await user.PostAsync("/OrgWorkspace/Switch", new() { ["membershipId"] = association.ToString() }, "/MyWork/Index");
            (total, byKind) = await CountAsync(user);
            Assert.Equal(0, total);
            Assert.Empty(byKind);
            Assert.Empty(PageCounts(await user.HtmlAsync("/MyWork/Index")));
        }
    }
}
