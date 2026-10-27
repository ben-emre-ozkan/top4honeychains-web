using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // One signed-in user: own cookie jar, anti-forgery tokens taken from the pages, flash messages read back
    public sealed class UserClient
    {
        private readonly HttpClient _http;
        public string Name { get; }

        private UserClient(HttpClient http, string name)
        {
            _http = http;
            // the assertions use the English texts (D-51: Turkish is the default language)
            _http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
            Name = name;
        }

        public static UserClient Anonymous(E2EApp app) =>
            new(app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true }), "anonymous");

        public static async Task<UserClient> LoginAsync(E2EApp app, string userName)
        {
            var client = new UserClient(app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true }), userName);
            var response = await client.PostAsync("/Account/Login", new() { ["UserName"] = userName, ["Password"] = E2EApp.Password }, "/Account/Login");
            Assert.True(response.StatusCode == HttpStatusCode.Redirect, $"login of {userName} failed: {(int)response.StatusCode}");
            Assert.DoesNotContain("/Account/Login", response.Headers.Location?.ToString() ?? "");
            return client;
        }

        // D-78: sign in with any password and return the answer (a temporary password, a wrong one, an expired one)
        public static async Task<(UserClient Client, HttpResponseMessage Response)> SignInAsync(E2EApp app, string userName, string password)
        {
            var client = new UserClient(app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true }), userName);
            var response = await client.PostAsync("/Account/Login", new() { ["UserName"] = userName, ["Password"] = password }, "/Account/Login");
            return (client, response);
        }

        public async Task<HttpResponseMessage> GetAsync(string url) => await _http.GetAsync(url);

        public async Task<string> HtmlAsync(string url)
        {
            var response = await _http.GetAsync(url);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"{Name}: GET {url} returned {(int)response.StatusCode}");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<int> StatusAsync(string url) => (int)(await _http.GetAsync(url)).StatusCode;

        private async Task<string> TokenAsync(string fromUrl)
        {
            var html = await (await _http.GetAsync(fromUrl)).Content.ReadAsStringAsync();
            var m = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");
            if (!m.Success)
                m = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
            Assert.True(m.Success, $"{Name}: no anti-forgery token on {fromUrl}");
            return m.Groups[1].Value;
        }

        // a form with repeated names (checkbox lists)
        public async Task<HttpResponseMessage> PostPairsAsync(string url, IEnumerable<KeyValuePair<string, string>> pairs, string tokenFrom)
        {
            var list = pairs.ToList();
            list.Add(new("__RequestVerificationToken", await TokenAsync(tokenFrom)));
            return await _http.PostAsync(url, new FormUrlEncodedContent(list));
        }

        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> form, string tokenFrom)
        {
            form["__RequestVerificationToken"] = await TokenAsync(tokenFrom);
            return await _http.PostAsync(url, new FormUrlEncodedContent(form));
        }

        public async Task<HttpResponseMessage> PostMultipartAsync(string url, Dictionary<string, string> form, (string Name, string FileName, byte[] Content, string Type)? file, string tokenFrom)
        {
            var content = new MultipartFormDataContent();
            form["__RequestVerificationToken"] = await TokenAsync(tokenFrom);
            foreach (var (k, v) in form)
                content.Add(new StringContent(v), k);
            if (file is { } f)
            {
                var bytes = new ByteArrayContent(f.Content);
                bytes.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(f.Type);
                content.Add(bytes, f.Name, f.FileName);
            }
            return await _http.PostAsync(url, content);
        }

        // Follows the redirect after a POST and returns the flash message shown there ("" when none)
        public async Task<string> FlashAfterAsync(HttpResponseMessage response)
        {
            Assert.True(response.StatusCode == HttpStatusCode.Redirect, $"{Name}: expected a redirect, got {(int)response.StatusCode}");
            var html = await (await _http.GetAsync(response.Headers.Location!.ToString())).Content.ReadAsStringAsync();
            var m = Regex.Match(html, "id=\"flash-(?:error|success)\"[^>]*>(.*?)<button", RegexOptions.Singleline);
            if (!m.Success)   // pages that show messages in a dialog
                m = Regex.Match(html, "text: \"(.*?)\",", RegexOptions.Singleline);
            return m.Success ? WebUtility.HtmlDecode(Regex.Replace(m.Groups[1].Value, "<[^>]+>", "")).Trim() : "";
        }

        public async Task<string> PostFlashAsync(string url, Dictionary<string, string> form, string tokenFrom) =>
            await FlashAfterAsync(await PostAsync(url, form, tokenFrom));
    }

    public static class Pdf
    {
        // A minimal PDF (signature checked by the upload validator)
        public static readonly byte[] Bytes = System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj<</Type/Catalog>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF\n");
        public static (string, string, byte[], string) File(string field) => (field, "e2e.pdf", Bytes, "application/pdf");
    }
}
