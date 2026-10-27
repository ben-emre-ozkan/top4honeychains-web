using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.Messaging;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-78 P1 (D78-SPEC 5): SMS texts, segment counting, the code primitives and the configuration rules
    public class SmsTextTests
    {
        private static readonly Dictionary<string, string> Sample = new()
        {
            ["org"] = "Dereli İlçe Tarım Müdürlüğü", ["code"] = "123456", ["link"] = "https://top4honeychains.intalalab.net/privacy",
            ["user"] = "TR-BEE-2026-000068", ["password"] = "Kp7mWx3qRt", ["date"] = "27.10.2026", ["containers"] = "3 teneke",
            ["kg"] = "75", ["honey"] = "Kestane Balı", ["samples"] = "3", ["name"] = "EVET", ["version"] = "platform-2026-11-v1"
        };

        [Fact]
        public void Every_purpose_has_all_five_languages_with_the_same_placeholders()
        {
            Assert.Equal(6, SmsTemplates.Purposes.Count());
            foreach (var purpose in SmsTemplates.Purposes)
            {
                var en = SmsTemplates.PlaceholdersOf(SmsTemplates.Template(purpose, "en")).OrderBy(x => x).ToList();
                if (purpose != SmsPurpose.Credentials)
                    Assert.Contains("code", en);
                foreach (var lang in SmsTemplates.Languages)
                {
                    var text = SmsTemplates.Template(purpose, lang);
                    Assert.StartsWith("TOP4HoneyChains: ", text);
                    Assert.Equal(en, SmsTemplates.PlaceholdersOf(text).OrderBy(x => x).ToList());
                }
            }
        }

        [Fact]
        public void Render_fills_every_placeholder()
        {
            foreach (var purpose in SmsTemplates.Purposes)
                foreach (var lang in SmsTemplates.Languages)
                    Assert.DoesNotContain("{", SmsTemplates.Render(purpose, lang, Sample));
        }

        [Theory]
        [InlineData(null, "tr")]
        [InlineData("en-US", "en")]
        [InlineData("LV", "lv")]
        [InlineData("de", "tr")]
        public void Unknown_languages_fall_back_to_turkish(string? given, string expected) => Assert.Equal(expected, SmsTemplates.Language(given));

        [Fact]
        public void Yes_and_no_words_per_language()
        {
            Assert.Equal("EVET", SmsTemplates.YesNoWord(true, "tr"));
            Assert.Equal("NIE", SmsTemplates.YesNoWord(false, "pl"));
            Assert.Equal("JĀ", SmsTemplates.YesNoWord(true, "lv"));
        }

        // D78-SPEC 5.5: M1, M2 and M3 are two segments in Turkish, M4 and M5 one
        [Theory]
        [InlineData(SmsPurpose.Registration, 2)]
        [InlineData(SmsPurpose.IntakeConfirm, 2)]
        [InlineData(SmsPurpose.Credentials, 2)]
        [InlineData(SmsPurpose.NoticeConfirm, 1)]
        [InlineData(SmsPurpose.PasswordReset, 1)]
        [InlineData(SmsPurpose.PhoneVerify, 1)]
        public void Turkish_texts_have_the_expected_segment_count(string purpose, int segments)
        {
            var values = new Dictionary<string, string>(Sample) { ["org"] = "Dereli İlçe Müdürlüğü", ["link"] = "top4honeychains.intalalab.net/p" };
            Assert.Equal(segments, SmsSegments.Count(SmsTemplates.Render(purpose, "tr", values)));
        }

        [Fact]
        public void Segments_follow_the_encoding()
        {
            Assert.Equal(SmsSegments.Encoding.Gsm7, SmsSegments.EncodingOf("Code 123456"));
            Assert.Equal(SmsSegments.Encoding.Turkish, SmsSegments.EncodingOf("şifre kodunuz"));
            Assert.Equal(SmsSegments.Encoding.Unicode, SmsSegments.EncodingOf("ważny kod"));
            Assert.Equal(1, SmsSegments.Count(new string('a', 160)));
            Assert.Equal(2, SmsSegments.Count(new string('a', 161)));
            Assert.Equal(1, SmsSegments.Count(new string('ş', 155)));
            Assert.Equal(2, SmsSegments.Count(new string('ş', 156)));
            Assert.Equal(1, SmsSegments.Count(new string('ą', 70)));
            Assert.Equal(2, SmsSegments.Count(new string('ą', 71)));
            Assert.Equal(2, SmsSegments.Count(new string('€', 81)));   // extension characters count twice
        }
    }

    public class CodeRulesTests
    {
        private static readonly byte[] Key = new byte[32];

        [Fact]
        public void Codes_have_six_digits_and_vary()
        {
            var codes = Enumerable.Range(0, 200).Select(_ => CodeRules.NewCode()).ToList();
            Assert.All(codes, c => Assert.True(CodeRules.IsWellFormed(c), c));
            Assert.True(codes.Distinct().Count() > 150);
        }

        [Fact]
        public void A_code_is_stored_only_as_a_salted_hash()
        {
            var salt = new byte[16];
            salt[0] = 7;
            var hash = CodeRules.Hash(Key, salt, "042195");
            Assert.Equal(64, hash.Length);
            Assert.DoesNotContain("042195", hash);
            Assert.True(CodeRules.Matches(Key, salt, "042195", hash));
            Assert.False(CodeRules.Matches(Key, salt, "042196", hash));
            Assert.False(CodeRules.Matches(Key, new byte[16], "042195", hash));   // another salt, another hash
        }

        [Theory]
        [InlineData("12345", false)]
        [InlineData("1234567", false)]
        [InlineData("12a456", false)]
        [InlineData("٠١٢٣٤٥", false)]   // non-ASCII digits are refused
        [InlineData("000123", true)]
        public void Well_formed_codes(string code, bool ok) => Assert.Equal(ok, CodeRules.IsWellFormed(code));

        [Fact]
        public void Secrets_are_masked_in_the_stored_text()
        {
            var text = "user TR-BEE-1, temporary password Kp7mWx3qRt, code 123456";
            Assert.Equal("user TR-BEE-1, temporary password ******, code ******", CodeRules.Masked(text, new[] { "Kp7mWx3qRt", "123456" }));
        }

        [Fact]
        public void Phone_numbers_are_masked_and_hashed()
        {
            Assert.Equal("+90******4567", PhoneMask.Mask("+905321234567"));
            Assert.Equal("+371****5678", PhoneMask.Mask("+37120345678"));
            var a = CodeRules.PhoneHash(Key, "+905321234567");
            Assert.Equal(64, a.Length);
            Assert.NotEqual(a, CodeRules.PhoneHash(Key, "+905321234568"));
        }

        [Fact]
        public void Content_hash_changes_with_the_content()
        {
            Assert.NotEqual(CodeRules.ContentHash("a|1"), CodeRules.ContentHash("a|2"));
            Assert.Equal(CodeRules.ContentHash("x"), CodeRules.ContentHash("x"));
        }
    }

    public class SmsOptionsTests
    {
        [Fact]
        public void Fake_provider_is_refused_in_production_unless_allowed()
        {
            var o = new SmsOptions { Provider = "Fake", CodeKey = "a2V5" };
            Assert.Contains(o.Validate(isDevelopment: false), e => e.Contains("Fake"));
            Assert.Empty(o.Validate(isDevelopment: true));
            o.AllowFake = true;
            Assert.Empty(o.Validate(isDevelopment: false));
        }

        [Fact]
        public void Code_key_and_provider_credentials_are_required_outside_development()
        {
            Assert.Contains(new SmsOptions { Provider = "Logging" }.Validate(false), e => e.Contains("CodeKey"));
            Assert.Contains(new SmsOptions { Provider = "Netgsm", CodeKey = "a2V5" }.Validate(false), e => e.Contains("Netgsm"));
            Assert.Contains(new SmsOptions { Provider = "IletiMerkezi", CodeKey = "a2V5" }.Validate(false), e => e.Contains("IletiMerkezi"));
            Assert.Contains(new SmsOptions { Provider = "Twilio" }.Validate(true), e => e.Contains("unknown"));
            Assert.Contains(new SmsOptions { Sender = "TOOLONGSENDERNAME" }.Validate(true), e => e.Contains("Sender"));
            Assert.Empty(new SmsOptions { Provider = "Netgsm", CodeKey = "a2V5", Netgsm = { UserCode = "u", Password = "p" } }.Validate(false));
        }

        [Fact]
        public void Allowed_countries_are_parsed()
        {
            Assert.Equal(new[] { "LV", "TR" }, new SmsOptions { AllowedCountries = "tr, lv" }.AllowedCountrySet.OrderBy(x => x));
        }
    }

    // The provider gateways against a mocked HTTP handler (no provider account exists yet; D78-SPEC 5.1)
    public class SmsGatewayTests
    {
        private sealed class Handler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, string, HttpResponseMessage> _answer;
            public List<(HttpRequestMessage Request, string Body)> Calls { get; } = new();
            public Handler(Func<HttpRequestMessage, string, HttpResponseMessage> answer) => _answer = answer;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            {
                var body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(ct);
                Calls.Add((request, body));
                return _answer(request, body);
            }
        }

        private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
            new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

        private static SmsOptions Options() => new()
        {
            Provider = "Netgsm", Sender = "TOP4HONEY",
            Netgsm = { UserCode = "8503000000", Password = "secret", BaseUrl = "https://api.example.invalid" },
            IletiMerkezi = { Key = "k-123", Hash = "h-456", BaseUrl = "https://im.example.invalid" }
        };

        private static NetgsmSmsGateway Netgsm(Handler h, SmsOptions? o = null) =>
            new(new HttpClient(h), Microsoft.Extensions.Options.Options.Create(o ?? Options()), NullLogger<NetgsmSmsGateway>.Instance);

        private static IletiMerkeziSmsGateway Ileti(Handler h) =>
            new(new HttpClient(h), Microsoft.Extensions.Options.Options.Create(Options()), NullLogger<IletiMerkeziSmsGateway>.Instance);

        [Fact]
        public async Task Netgsm_send_posts_the_rest_v2_request_with_basic_auth()
        {
            var h = new Handler((_, _) => Json("{\"code\":\"00\",\"jobid\":\"17377215342605050417149344\",\"description\":\"queued\"}"));
            var result = await Netgsm(h).SendAsync("+905321234567", "TOP4HoneyChains: şifre kodunuz 123456", isCode: true);

            Assert.True(result.Accepted);
            Assert.Equal(SmsStatus.Sent, result.Status);
            Assert.Equal("17377215342605050417149344", result.ProviderMessageId);
            var (request, body) = Assert.Single(h.Calls);
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://api.example.invalid/sms/rest/v2/send", request.RequestUri!.ToString());
            Assert.Equal("Basic", request.Headers.Authorization!.Scheme);
            Assert.Equal("8503000000:secret", Encoding.UTF8.GetString(Convert.FromBase64String(request.Headers.Authorization.Parameter!)));
            var json = JsonNode.Parse(body)!;
            Assert.Equal("TOP4HONEY", json["msgheader"]!.ToString());
            Assert.Equal("TR", json["encoding"]!.ToString());
            Assert.Equal("0", json["iysfilter"]!.ToString());
            Assert.Equal("5321234567", json["messages"]![0]!["no"]!.ToString());
            Assert.Equal("TOP4HoneyChains: şifre kodunuz 123456", json["messages"]![0]!["msg"]!.ToString());
        }

        [Fact]
        public async Task Netgsm_error_codes_are_failures()
        {
            var h = new Handler((_, _) => Json("{\"code\":\"30\",\"description\":\"Invalid username\"}", HttpStatusCode.NotAcceptable));
            var result = await Netgsm(h).SendAsync("+905321234567", "x", false);
            Assert.False(result.Accepted);
            Assert.Equal(SmsStatus.Failed, result.Status);
            Assert.Contains("Netgsm 30", result.Error);
        }

        [Fact]
        public async Task Netgsm_unreachable_is_a_failure_not_an_exception()
        {
            var h = new Handler((_, _) => throw new HttpRequestException("no route"));
            var result = await Netgsm(h).SendAsync("+905321234567", "x", false);
            Assert.False(result.Accepted);
        }

        [Fact]
        public async Task Netgsm_otp_route_is_used_only_when_enabled_for_one_segment_codes()
        {
            var o = Options();
            o.Netgsm.UseOtpRoute = true;
            var h = new Handler((_, _) => Json("{\"code\":\"00\",\"jobId\":\"9\",\"description\":\"ok\"}"));
            var gw = Netgsm(h, o);
            Assert.True((await gw.SendAsync("+905321234567", "TOP4HoneyChains: code 123456", isCode: true)).Accepted);
            await gw.SendAsync("+905321234567", "TOP4HoneyChains: info", isCode: false);
            Assert.EndsWith("/sms/rest/v2/otp", h.Calls[0].Request.RequestUri!.ToString());
            Assert.Equal("5321234567", JsonNode.Parse(h.Calls[0].Body)!["no"]!.ToString());
            Assert.EndsWith("/sms/rest/v2/send", h.Calls[1].Request.RequestUri!.ToString());
        }

        [Theory]
        [InlineData(0, SmsStatus.Sent)]
        [InlineData(1, SmsStatus.Delivered)]
        [InlineData(2, SmsStatus.Expired)]
        [InlineData(3, SmsStatus.Failed)]
        [InlineData(16, SmsStatus.Failed)]
        public void Netgsm_report_statuses(int status, string expected) => Assert.Equal(expected, NetgsmSmsGateway.MapStatus(status));

        [Fact]
        public async Task Netgsm_report_reads_the_job_status()
        {
            var h = new Handler((_, _) => Json("{\"code\":\"00\",\"description\":\"ok\",\"jobs\":[{\"jobid\":\"77\",\"number\":\"5321234567\",\"status\":1,\"operator\":10,\"msglen\":1}]}"));
            var status = await Netgsm(h).GetStatusAsync("77", DateTime.UtcNow);
            Assert.Equal(SmsStatus.Delivered, status!.Status);
            Assert.EndsWith("/sms/rest/v2/report", h.Calls[0].Request.RequestUri!.ToString());
            Assert.Equal("77", JsonNode.Parse(h.Calls[0].Body)!["jobids"]![0]!.ToString());
        }

        [Fact]
        public async Task Ileti_merkezi_send_posts_the_v1_json_order()
        {
            var h = new Handler((_, _) => Json("{\"response\":{\"status\":{\"code\":200,\"message\":\"İşlem başarılı\"},\"order\":{\"id\":\"312891245\"}}}"));
            var result = await Ileti(h).SendAsync("+905321234567", "TOP4HoneyChains: kod 123456", true);

            Assert.True(result.Accepted);
            Assert.Equal("312891245", result.ProviderMessageId);
            var (request, body) = Assert.Single(h.Calls);
            Assert.Equal("https://im.example.invalid/v1/send-sms/json", request.RequestUri!.ToString());
            var r = JsonNode.Parse(body)!["request"]!;
            Assert.Equal("k-123", r["authentication"]!["key"]!.ToString());
            Assert.Equal("h-456", r["authentication"]!["hash"]!.ToString());
            Assert.Equal("TOP4HONEY", r["order"]!["sender"]!.ToString());
            Assert.Equal("0", r["order"]!["iys"]!.ToString());
            Assert.Equal("905321234567", r["order"]!["message"]!["receipents"]!["number"]![0]!.ToString());
        }

        [Fact]
        public async Task Ileti_merkezi_status_codes_other_than_200_are_failures()
        {
            var h = new Handler((_, _) => Json("{\"response\":{\"status\":{\"code\":\"401\",\"message\":\"Üyelik bilgileri hatalı\"}}}"));
            var result = await Ileti(h).SendAsync("+905321234567", "x", false);
            Assert.False(result.Accepted);
            Assert.Contains("401", result.Error);
        }

        [Fact]
        public async Task Ileti_merkezi_report_maps_the_recipient_status()
        {
            var h = new Handler((_, _) => Json("{\"response\":{\"status\":{\"code\":200},\"order\":{\"id\":\"5\",\"message\":[{\"number\":\"+905321234567\",\"status\":111}]}}}"));
            Assert.Equal(SmsStatus.Delivered, (await Ileti(h).GetStatusAsync("5", DateTime.UtcNow))!.Status);
            Assert.Equal(SmsStatus.Failed, IletiMerkeziSmsGateway.MapStatus(112));
            Assert.Equal(SmsStatus.Sent, IletiMerkeziSmsGateway.MapStatus(110));
        }

        [Fact]
        public async Task Fake_gateway_keeps_the_full_message()
        {
            var store = new FakeSmsStore();
            var result = await new FakeSmsGateway(store).SendAsync("+905321234567", "code 123456", true);
            Assert.Equal(SmsStatus.Fake, result.Status);
            Assert.Equal("code 123456", store.Find(result.ProviderMessageId!)!.Text);
            Assert.Equal("+905321234567", store.LastTo("+905321234567")!.To);
        }
    }
}
