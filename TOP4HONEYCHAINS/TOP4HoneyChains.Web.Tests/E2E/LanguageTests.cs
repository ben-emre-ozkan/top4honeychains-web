using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-51: Turkish is the default language, the selected language wins, and reference data follows it
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class LanguageTests
    {
        private readonly E2EApp _app;

        public LanguageTests(E2EApp app) => _app = app;

        private async Task<string> GetAsync(string url, string? language)
        {
            var client = _app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            if (language != null)
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(language);
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task Without_a_choice_the_site_is_turkish()
        {
            var html = await GetAsync("/Account/Login", null);
            Assert.Contains("<html lang=\"tr\"", html);
        }

        [Theory]
        [InlineData("tr")]
        [InlineData("es")]
        [InlineData("pl")]
        [InlineData("lv")]
        public async Task The_selected_language_is_used(string language)
        {
            var html = await GetAsync("/Account/Login", language);
            Assert.Contains($"<html lang=\"{language}\"", html);
            Assert.DoesNotContain("Management Information System", html);
        }

        [Fact]
        public async Task Reference_data_is_shown_in_the_selected_language()
        {
            var qr = await _app.WithContextAsync(async db =>
            {
                var type = await db.HoneyTypes.OrderBy(h => h.HoneyTypeId).FirstAsync();
                if (!await db.ReferenceTranslations.AnyAsync(t => t.Entity == RefEntity.HoneyType && t.EntityKey == type.HoneyTypeId.ToString() && t.Culture == "lv"))
                    db.ReferenceTranslations.Add(new ReferenceTranslation { Entity = RefEntity.HoneyType, EntityKey = type.HoneyTypeId.ToString(), Field = "Title", Culture = "lv", Value = "LV-medus-tests", UpdatedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
                return type.HoneyTypeId;
            });
            _app.Services.GetRequiredService<TOP4HoneyChains.Web.Services.RefText>().Invalidate();
            var expected = await _app.WithContextAsync(db => db.ReferenceTranslations
                .Where(t => t.Entity == RefEntity.HoneyType && t.EntityKey == qr.ToString() && t.Field == "Title" && t.Culture == "lv").Select(t => t.Value).FirstAsync());
            var admin = await UserClient.LoginAsync(_app, await EnsureAdminAsync());
            var page = await admin.HtmlAsync("/HoneyTypes/Index?culture=lv");
            Assert.Contains(System.Net.WebUtility.HtmlEncode(expected), page);
        }

        private async Task<string> EnsureAdminAsync()
        {
            const string name = "e2e_lang_admin";
            await _app.CreateUserAsync(name, "Administrator");
            return name;
        }
    }
}
