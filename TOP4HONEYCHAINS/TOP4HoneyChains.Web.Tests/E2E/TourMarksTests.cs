using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // P4 follow-up (027, D-84): the first-use tour's "seen" marks are kept per user and role on the server
    public partial class CustodyIntakeTests
    {
        private static string TourSeen(string html) => Regex.Match(html, @"id=""hs-tour""[^>]*data-seen=""(\w+)""").Groups[1].Value;

        [Fact]
        public async Task The_tour_is_marked_seen_on_the_server_per_user_and_role_and_a_new_device_does_not_show_it_again()
        {
            var p = await PlantAsync("tour");
            var userId = await _app.CreateUserAsync("zz_test_tour_staff");
            await _app.AddMemberAsync(p.OrgId, userId, OrgRoles.Staff);
            var first = await UserClient.LoginAsync(_app, "zz_test_tour_staff");
            Assert.Equal("false", TourSeen(await first.HtmlAsync("/MyWork/Index")));

            // without the antiforgery token nothing is stored
            var forged = await first.RawPostAsync("/Account/TourSeen");
            Assert.Equal(HttpStatusCode.BadRequest, forged.StatusCode);
            Assert.Equal("false", TourSeen(await first.HtmlAsync("/MyWork/Index")));

            Assert.Equal(HttpStatusCode.NoContent, (await first.PostAsync("/Account/TourSeen", new(), "/MyWork/Index")).StatusCode);
            Assert.Equal("Staff", await _app.WithContextAsync(db => db.UserSettings.Where(s => s.UserId == userId).Select(s => s.ToursSeen).SingleAsync()));
            // a second device (a new sign-in without the first browser's storage)
            var second = await UserClient.LoginAsync(_app, "zz_test_tour_staff");
            Assert.Equal("true", TourSeen(await second.HtmlAsync("/MyWork/Index")));

            // another role is a new tour; marking it keeps the first one
            await _app.WithContextAsync(async db =>
            {
                var m = await db.Memberships.SingleAsync(x => x.UserId == userId && x.OrgId == p.OrgId);
                m.OrgRole = OrgRoles.FieldInspector;
                return await db.SaveChangesAsync();
            });
            var third = await UserClient.LoginAsync(_app, "zz_test_tour_staff");
            Assert.Equal("false", TourSeen(await third.HtmlAsync("/MyWork/Index")));
            await third.PostAsync("/Account/TourSeen", new(), "/MyWork/Index");
            Assert.Equal("Staff,FieldInspector", await _app.WithContextAsync(db => db.UserSettings.Where(s => s.UserId == userId).Select(s => s.ToursSeen).SingleAsync()));
        }

        [Fact]
        public async Task A_beekeeper_marks_the_portal_tour_seen_through_the_portal_boundary() => await WithPortalAsync(async () =>
        {
            var bee = await PortalBeekeeperAsync("tourbee");
            Assert.Equal("false", TourSeen(await bee.Client.HtmlAsync("/Me")));
            Assert.Equal(HttpStatusCode.NoContent, (await bee.Client.PostAsync("/Account/TourSeen", new(), "/Me")).StatusCode);
            Assert.Equal("true", TourSeen(await bee.Client.HtmlAsync("/Me")));
            return 0;
        });
    }
}
