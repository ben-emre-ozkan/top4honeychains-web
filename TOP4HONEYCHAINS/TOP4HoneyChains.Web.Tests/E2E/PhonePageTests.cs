using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Controllers;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.Messaging;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-88: the phone page of the fake SMS provider: only with the fake provider, never in Production, only with the key, and only the
    // messages of the number in the link
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class PhonePageTests
    {
        private readonly E2EApp _app;

        public PhonePageTests(E2EApp app) => _app = app;

        private Task<long> SmsAsync(string e164, string text) => _app.WithContextAsync(async db =>
        {
            var options = _app.Services.GetRequiredService<IOptions<SmsOptions>>().Value;
            var sms = new SmsMessage
            {
                Purpose = "Info", ToPhoneMasked = "+90 555 *** ** " + e164[^2..], ToPhoneHash = CodeRules.PhoneHash(options.KeyBytes(), e164),
                Language = "tr", BodyStored = text, Segments = 1, Provider = SmsProviders.Fake, Status = SmsStatus.Fake, CreatedAt = DateTime.UtcNow
            };
            db.SmsMessages.Add(sms);
            await db.SaveChangesAsync();
            return sms.SmsId;
        });

        [Fact]
        public async Task The_page_shows_only_the_messages_of_its_number_and_needs_the_key()
        {
            var key = _app.Services.GetRequiredService<PhonePageKey>().Value;
            await SmsAsync("+905550000971", "zz_test code 123456 for this phone");
            await SmsAsync("+905550000972", "zz_test message for another phone");
            var anonymous = UserClient.Anonymous(_app);

            var page = await anonymous.GetAsync($"/phone/{key}/905550000971");
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);
            var html = WebUtility.HtmlDecode(await page.Content.ReadAsStringAsync());
            Assert.Contains("+90 555 000 09 71", html);
            Assert.DoesNotContain("id=\"page-topbar\"", html);   // no TOP4 menus
            Assert.Contains("no-store", page.Headers.CacheControl?.ToString() ?? "");
            Assert.Equal("no-referrer", page.Headers.GetValues("Referrer-Policy").Single());

            var json = await anonymous.GetAsync($"/phone/{key}/905550000971/messages");
            Assert.Equal(HttpStatusCode.OK, json.StatusCode);
            var texts = JsonDocument.Parse(await json.Content.ReadAsStringAsync()).RootElement.EnumerateArray().Select(m => m.GetProperty("text").GetString()).ToList();
            Assert.Contains("zz_test code 123456 for this phone", texts);
            Assert.DoesNotContain("zz_test message for another phone", texts);

            // a wrong key, a key of the right length but another value, or a malformed number: 404, nothing shown
            foreach (var url in new[] { $"/phone/wrong-key-0000000000/905550000971", $"/phone/{new string('A', key.Length)}/905550000971/messages",
                                        $"/phone/{key}/0555", $"/phone/{key}/+905550000971/messages" })
            {
                var r = await anonymous.GetAsync(url);
                Assert.True(r.StatusCode == HttpStatusCode.NotFound, $"{url}: {(int)r.StatusCode} {r.Headers.Location}");
            }
        }

        private sealed class Env : IWebHostEnvironment
        {
            public string EnvironmentName { get; set; } = "Production";
            public string ApplicationName { get; set; } = "TOP4HoneyChains.Web";
            public string WebRootPath { get; set; } = "";
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
            public string ContentRootPath { get; set; } = "";
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }

        [Theory]
        [InlineData("Production", "Fake")]       // never in Production, even with the fake provider
        [InlineData("Development", "Logging")]   // only with the fake provider
        [InlineData("Development", "Netgsm")]
        public async Task The_page_is_off_outside_development_and_without_the_fake_provider(string environment, string provider)
        {
            var key = new PhonePageKey("zz-test-key-0123456789");
            var controller = new PhoneController(null, Options.Create(new SmsOptions { Provider = provider }), new Env { EnvironmentName = environment }, key);
            Assert.IsType<NotFoundResult>(controller.Index(key.Value, "905550000971"));
            Assert.IsType<NotFoundResult>(await controller.Messages(key.Value, "905550000971"));
        }

        [Fact]
        public void A_missing_or_unusable_key_is_replaced_by_a_random_one()
        {
            var a = new PhonePageKey(null);
            var b = new PhonePageKey("short");
            Assert.True(a.Generated && b.Generated);
            Assert.NotEqual(a.Value, b.Value);
            Assert.True(a.Value.Length >= 24);
            var kept = new PhonePageKey("zz-test-key-0123456789");
            Assert.False(kept.Generated);
            Assert.True(kept.Matches("zz-test-key-0123456789"));
            Assert.False(kept.Matches("zz-test-key-0123456788"));
            Assert.False(kept.Matches(null));
        }
    }
}
