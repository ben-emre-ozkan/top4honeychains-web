using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-65: the redesigned landing and sign-in pages
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class PublicPagesTests
    {
        private readonly E2EApp _app;

        public PublicPagesTests(E2EApp app) => _app = app;

        private async Task<string> GetAsync(string url, string language = "en")
        {
            var client = _app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(language);
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.Content.ReadAsStringAsync();
        }

        // no CDN, no Google: every script, stylesheet, image, font and frame is served by the app
        private static void AssertNoExternalRequests(string html)
        {
            foreach (Match m in Regex.Matches(html, @"<(script|link|img|source|iframe)\b[^>]*>", RegexOptions.IgnoreCase))
            {
                if (Regex.IsMatch(m.Value, @"rel=""canonical"""))
                    continue;
                Assert.DoesNotMatch(@"(src|href|srcset)=""(https?:)?//", m.Value);
            }
            Assert.DoesNotContain("googleapis", html);
            Assert.DoesNotContain("landingassets", html);
        }

        // D-78 P4 (D78-SPEC 4.11): two labelled logo groups in every language; partners are never shown as funders
        [Theory]
        [InlineData("tr", "Proje paydaşları", "Fonlayan kurumlar")]
        [InlineData("en", "Project partners", "Funding organizations")]
        [InlineData("es", "Socios del proyecto", "Entidades financiadoras")]
        [InlineData("pl", "Partnerzy projektu", "Instytucje finansujące")]
        [InlineData("lv", "Projekta partneri", "Finansētāji")]
        public async Task The_landing_shows_partners_and_funders_as_separate_groups(string language, string partners, string funders)
        {
            var html = WebUtility.HtmlDecode(await GetAsync("/", language));
            var partnerGroup = Regex.Match(html, "<section class=\"ld-logo-group\" id=\"partner-logos\".*?</section>", RegexOptions.Singleline).Value;
            var funderGroup = Regex.Match(html, "<section class=\"ld-logo-group ld-funders\" id=\"funder-logos\".*?</section>", RegexOptions.Singleline).Value;
            Assert.Contains(partners, partnerGroup);
            Assert.Contains(funders, funderGroup);
            var alts = Regex.Matches(partnerGroup, "<img [^>]*alt=\"([^\"]+)\"").Select(m => m.Groups[1].Value).ToList();
            Assert.Equal(9, alts.Count);
            Assert.Equal("Boğaziçi University", alts[0]);   // the coordinator first
            Assert.All(alts, a => Assert.False(string.IsNullOrWhiteSpace(a)));
            Assert.Contains("alt=\"ICT-AGRI-FOOD ERA-NET Cofund\"", funderGroup);
            Assert.Contains("ICT-AGRI-FOOD", funderGroup);
            Assert.DoesNotContain("/img/partners/", funderGroup);
            Assert.DoesNotContain("ICT-AGRI-FOOD", partnerGroup);
        }

        [Theory]
        [InlineData("tr", "Giriş yap")]
        [InlineData("en", "Sign in")]
        [InlineData("es", "Iniciar sesión")]
        [InlineData("pl", "Zaloguj się")]
        [InlineData("lv", "Pieteikties")]
        public async Task The_landing_is_in_the_selected_language(string language, string signIn)
        {
            var html = WebUtility.HtmlDecode(await GetAsync("/", language));
            Assert.Contains($"<html lang=\"{language}\"", html);
            Assert.Contains(signIn, html);
            Assert.DoesNotContain("Land.", html);   // no untranslated key
        }

        [Fact]
        public async Task The_landing_has_its_sections_and_no_external_requests()
        {
            var html = await GetAsync("/");
            AssertNoExternalRequests(html);
            foreach (var id in new[] { "id=\"chain\"", "id=\"trust\"", "id=\"for-whom\"", "id=\"story\"", "data-hero-comb", "data-honey-line", "href=\"/Account/Login\"", "href=\"/verify-report\"" })
                Assert.Contains(id, html);
            Assert.Equal(5, Regex.Matches(html, "class=\"ld-step ").Count);
            Assert.Contains("property=\"og:title\"", html);
            Assert.Contains("prefers-reduced-motion", await (await _app.CreateClient().GetAsync("/css/landing.css")).Content.ReadAsStringAsync());
            // counters are aggregates only and never show zero
            Assert.DoesNotMatch(@"<dd>0</dd>", html);
        }

        [Theory]
        [InlineData("/Account/Login")]
        [InlineData("/Account/ForgotPassword")]
        [InlineData("/Account/ResetPassword?token=zz_test&email=zz.test%40example.invalid")]
        public async Task The_sign_in_pages_share_the_new_look(string url)
        {
            var html = await GetAsync(url);
            Assert.Contains("class=\"au-panel\"", html);
            Assert.Contains("data-honeycomb", html);
            AssertNoExternalRequests(html);
        }

        // D-67: branded error pages with a way back; the 500 page shows a short reference
        [Theory]
        [InlineData("/zz-test-no-such-page", 404, "This page isn't here")]
        [InlineData("/Account/AccessDenied", 403, "You can't open this page")]
        [InlineData("/Dashboard/Status?code=500", 500, "Something went wrong on our side")]
        public async Task Error_pages_are_branded(string url, int code, string title)
        {
            // unknown addresses need sign-in first (deny by default, D-47), so the 404 page is checked signed in
            HttpResponseMessage response;
            if (code == 404)
            {
                const string name = "e2e_err_admin";
                await _app.CreateUserAsync(name, "Administrator");
                response = await (await UserClient.LoginAsync(_app, name)).GetAsync(url);
            }
            else
            {
                var client = _app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
                response = await client.GetAsync(url);
            }
            Assert.Equal(code == 403 ? HttpStatusCode.OK : (HttpStatusCode)code, response.StatusCode);
            var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            Assert.Contains($"data-code=\"{code}\"", html);
            Assert.Contains(title, html);
            Assert.Contains("src=\"/img/brand/logo-emblem.png\"", html);   // the project's emblem (2026-11-26)
            AssertNoExternalRequests(html);
            if (code == 500)
                Assert.Matches("id=\"error-reference\">E-[A-Z2-9]{8}<", html);
        }

        [Fact]
        public async Task The_login_form_keeps_what_it_did()
        {
            var html = await GetAsync("/Account/Login");
            Assert.Matches("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"", html);
            foreach (var field in new[] { "name=\"UserName\"", "name=\"Password\"", "name=\"RememberMe\"", "href=\"/Account/ForgotPassword\"", "autocomplete=\"current-password\"" })
                Assert.Contains(field, html);

            // a wrong password gives the same message as before (no hint whether the user exists or is locked)
            var client = UserClient.Anonymous(_app);
            var response = await client.PostAsync("/Account/Login", new() { ["UserName"] = "zz_test_nobody", ["Password"] = "wrong" }, "/Account/Login");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Invalid user name or password.", WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));

            // the return URL in the query stays on the form's address
            Assert.Contains("action=\"/Account/Login?ReturnUrl=%2FMyWork\"", await GetAsync("/Account/Login?ReturnUrl=%2FMyWork"));
        }

        // after sign-in a local return URL is followed; an external one falls back to the start page
        [Theory]
        [InlineData("e2e_ret_local", "%2FShipments%2FIndex", "/Shipments/Index")]
        [InlineData("e2e_ret_external", "https%3A%2F%2Fzz-test.example.invalid%2Fphish", "/MyWork")]
        [InlineData("e2e_ret_protocol", "%2F%2Fzz-test.example.invalid%2Fphish", "/MyWork")]
        public async Task Sign_in_follows_only_a_local_return_url(string name, string returnUrl, string expected)
        {
            await _app.CreateUserAsync(name, "Administrator");
            var client = UserClient.Anonymous(_app);
            var url = "/Account/Login?ReturnUrl=" + returnUrl;
            var response = await client.PostAsync(url, new() { ["UserName"] = name, ["Password"] = E2EApp.Password }, url);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location?.ToString());
        }
    }
}
