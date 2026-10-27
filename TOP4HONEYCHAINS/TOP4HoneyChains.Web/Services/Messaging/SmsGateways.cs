using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.Messaging
{
    // D-78 P1 (D78-SPEC 5.1): the provider layer. One class per provider; IMessageService picks the one configured in
    // Sms:Provider. Gateways never store anything; they only talk to the provider.
    public interface ISmsGateway
    {
        string Name { get; }
        Task<GatewayResult> SendAsync(string e164, string text, bool isCode, CancellationToken ct = default);
        // the delivery status of a sent message, or null when the provider has nothing (yet)
        Task<GatewayStatus?> GetStatusAsync(string providerMessageId, DateTime sentAtUtc, CancellationToken ct = default);
    }

    public record GatewayResult(bool Accepted, string Status, string? ProviderMessageId, string? Error);
    public record GatewayStatus(string Status, string? Error);

    public class SmsGatewayFactory
    {
        private readonly IServiceProvider _services;
        private readonly SmsOptions _options;

        public SmsGatewayFactory(IServiceProvider services, IOptions<SmsOptions> options)
        {
            _services = services;
            _options = options.Value;
        }

        public ISmsGateway Current() => _options.Provider.ToLowerInvariant() switch
        {
            "fake" => _services.GetRequiredService<FakeSmsGateway>(),
            "netgsm" => _services.GetRequiredService<NetgsmSmsGateway>(),
            "iletimerkezi" => _services.GetRequiredService<IletiMerkeziSmsGateway>(),
            _ => _services.GetRequiredService<LoggingSmsGateway>()
        };
    }

    // ---------------------------------------------------------------- fake (development, tests, demos)

    // What the fake provider "sent": full number and text, newest first, kept in memory (D78-SPEC 5.3)
    public class FakeSmsStore
    {
        public record Item(string ProviderMessageId, string To, string Text, DateTime At);

        private readonly ConcurrentQueue<Item> _items = new();
        private const int Max = 500;

        public void Add(Item item)
        {
            _items.Enqueue(item);
            while (_items.Count > Max && _items.TryDequeue(out _)) { }
        }

        public IReadOnlyList<Item> All() => _items.Reverse().ToList();

        public Item? Find(string providerMessageId) => _items.FirstOrDefault(i => i.ProviderMessageId == providerMessageId);

        // the newest message to a number
        public Item? LastTo(string e164) => All().FirstOrDefault(i => i.To == e164);
    }

    public class FakeSmsGateway : ISmsGateway
    {
        private readonly FakeSmsStore _store;
        public FakeSmsGateway(FakeSmsStore store) => _store = store;
        public string Name => SmsProviders.Fake;

        public Task<GatewayResult> SendAsync(string e164, string text, bool isCode, CancellationToken ct = default)
        {
            var id = "fake-" + Guid.NewGuid().ToString("N");
            _store.Add(new FakeSmsStore.Item(id, e164, text, DateTime.UtcNow));
            return Task.FromResult(new GatewayResult(true, SmsStatus.Fake, id, null));
        }

        public Task<GatewayStatus?> GetStatusAsync(string providerMessageId, DateTime sentAtUtc, CancellationToken ct = default) =>
            Task.FromResult<GatewayStatus?>(null);
    }

    // ---------------------------------------------------------------- logging (no provider configured)

    public class LoggingSmsGateway : ISmsGateway
    {
        private readonly ILogger<LoggingSmsGateway> _logger;
        public LoggingSmsGateway(ILogger<LoggingSmsGateway> logger) => _logger = logger;
        public string Name => SmsProviders.Logging;

        public Task<GatewayResult> SendAsync(string e164, string text, bool isCode, CancellationToken ct = default)
        {
            // the text is not logged: it may hold a code or a password
            _logger.LogInformation("SMS not sent (logging provider) to {Phone}, {Length} characters", PhoneMask.Mask(e164), text.Length);
            return Task.FromResult(new GatewayResult(true, SmsStatus.Logged, null, null));
        }

        public Task<GatewayStatus?> GetStatusAsync(string providerMessageId, DateTime sentAtUtc, CancellationToken ct = default) =>
            Task.FromResult<GatewayStatus?>(null);
    }

    // ---------------------------------------------------------------- Netgsm

    // Netgsm REST v2 (public OpenAPI of netgsm/netgsm-sms-js): POST /sms/rest/v2/send with HTTP Basic authentication
    // (user code : password), JSON { msgheader, encoding, iysfilter, messages: [{ msg, no }] }; answer { code, description,
    // jobid }, code "00" = accepted. Turkish numbers are sent as 10 digits (5XXXXXXXXX). Reports: POST /sms/rest/v2/report.
    // Not tested against the live service (no account yet); unit tests use a mocked HTTP handler.
    public class NetgsmSmsGateway : ISmsGateway
    {
        private readonly HttpClient _http;
        private readonly SmsOptions _options;
        private readonly ILogger<NetgsmSmsGateway> _logger;

        public NetgsmSmsGateway(HttpClient http, IOptions<SmsOptions> options, ILogger<NetgsmSmsGateway> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;
        }

        public string Name => SmsProviders.Netgsm;
        private NetgsmOptions O => _options.Netgsm;

        public static string Number(string e164)
        {
            var digits = new string(e164.Where(char.IsDigit).ToArray());
            return digits.StartsWith("90") && digits.Length == 12 ? digits[2..] : "00" + digits;
        }

        private HttpRequestMessage Request(string path, object body)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, O.BaseUrl.TrimEnd('/') + path) { Content = JsonContent.Create(body) };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{O.UserCode}:{O.Password}")));
            return request;
        }

        public async Task<GatewayResult> SendAsync(string e164, string text, bool isCode, CancellationToken ct = default)
        {
            var otp = isCode && O.UseOtpRoute && SmsSegments.Count(text) == 1;
            object body = otp
                ? new Dictionary<string, object?> { ["msgheader"] = _options.Sender, ["msg"] = text, ["no"] = Number(e164), ["appname"] = O.AppName }
                : new Dictionary<string, object?>
                {
                    ["msgheader"] = _options.Sender,
                    ["encoding"] = "TR",
                    ["iysfilter"] = "0",   // informational (transactional) message, no İYS check
                    ["appname"] = O.AppName,
                    ["messages"] = new[] { new Dictionary<string, string> { ["msg"] = text, ["no"] = Number(e164) } }
                };
            try
            {
                using var response = await _http.SendAsync(Request(otp ? "/sms/rest/v2/otp" : "/sms/rest/v2/send", body), ct);
                var json = await ReadJsonAsync(response, ct);
                var code = json?["code"]?.ToString();
                if (code == "00")
                {
                    var id = (json?["jobid"] ?? json?["jobId"])?.ToString();
                    return new GatewayResult(true, SmsStatus.Sent, id, null);
                }
                var error = $"Netgsm {(code ?? ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture))}: {json?["description"]}";
                _logger.LogWarning("Netgsm refused an SMS: {Error}", error);
                return new GatewayResult(false, SmsStatus.Failed, null, error);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "Netgsm could not be reached");
                return new GatewayResult(false, SmsStatus.Failed, null, "Netgsm: " + ex.GetType().Name);
            }
        }

        // Netgsm status: 0 waiting, 1 delivered, 2 expired, 3 and above not delivered (wrong number, rejected, İYS, …)
        public static string MapStatus(int status) => status switch
        {
            0 => SmsStatus.Sent,
            1 => SmsStatus.Delivered,
            2 => SmsStatus.Expired,
            _ => SmsStatus.Failed
        };

        public async Task<GatewayStatus?> GetStatusAsync(string providerMessageId, DateTime sentAtUtc, CancellationToken ct = default)
        {
            var tr = TimeZoneInfo.ConvertTimeFromUtc(sentAtUtc, TurkeyTime());
            var body = new Dictionary<string, object?>
            {
                ["jobids"] = new[] { providerMessageId },
                ["startdate"] = tr.AddHours(-1).ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                ["stopdate"] = tr.AddDays(2).ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                ["appname"] = O.AppName
            };
            try
            {
                using var response = await _http.SendAsync(Request("/sms/rest/v2/report", body), ct);
                var json = await ReadJsonAsync(response, ct);
                var job = json?["jobs"] as JsonArray;
                var first = job?.FirstOrDefault(j => j?["jobid"]?.ToString() == providerMessageId) ?? job?.FirstOrDefault();
                if (first?["status"] is JsonNode s && int.TryParse(s.ToString(), out var status))
                    return new GatewayStatus(MapStatus(status), status >= 3 ? $"Netgsm status {status}, error {first["errorCode"]}" : null);
                return null;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "Netgsm report could not be read");
                return null;
            }
        }

        internal static async Task<JsonNode?> ReadJsonAsync(HttpResponseMessage response, CancellationToken ct)
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            return string.IsNullOrWhiteSpace(text) ? null : JsonNode.Parse(text);
        }

        private static TimeZoneInfo TurkeyTime()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
            catch (TimeZoneNotFoundException) { return TimeZoneInfo.CreateCustomTimeZone("TRT", TimeSpan.FromHours(3), "TRT", "TRT"); }
        }
    }

    // ---------------------------------------------------------------- İleti Merkezi

    // İleti Merkezi v1 JSON API (public manifest https://www.iletimerkezi.com/api/manifest.json): POST /v1/send-sms/json with
    // { request: { authentication: { key, hash }, order: { sender, iys: "0", message: { text, receipents: { number: [...] } } } } }
    // ("receipents" is the API's own spelling); answer { response: { status: { code: 200 }, order: { id } } }.
    // Reports: POST /v1/get-report/json, per-recipient status 110 sending, 111 delivered, 112 not delivered.
    // Not tested against the live service (no account yet); unit tests use a mocked HTTP handler.
    public class IletiMerkeziSmsGateway : ISmsGateway
    {
        private readonly HttpClient _http;
        private readonly SmsOptions _options;
        private readonly ILogger<IletiMerkeziSmsGateway> _logger;

        public IletiMerkeziSmsGateway(HttpClient http, IOptions<SmsOptions> options, ILogger<IletiMerkeziSmsGateway> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;
        }

        public string Name => SmsProviders.IletiMerkezi;
        private IletiMerkeziOptions O => _options.IletiMerkezi;

        public static string Number(string e164) => new(e164.Where(char.IsDigit).ToArray());

        private object Authentication => new Dictionary<string, string?> { ["key"] = O.Key, ["hash"] = O.Hash };

        private Task<HttpResponseMessage> PostAsync(string path, object request, CancellationToken ct) =>
            _http.PostAsync(O.BaseUrl.TrimEnd('/') + path, JsonContent.Create(new Dictionary<string, object> { ["request"] = request }), ct);

        private static int? StatusCode(JsonNode? json)
        {
            var raw = json?["response"]?["status"]?["code"]?.ToString();
            return int.TryParse(raw, out var code) ? code : null;
        }

        public async Task<GatewayResult> SendAsync(string e164, string text, bool isCode, CancellationToken ct = default)
        {
            var request = new Dictionary<string, object>
            {
                ["authentication"] = Authentication,
                ["order"] = new Dictionary<string, object>
                {
                    ["sender"] = _options.Sender,
                    ["iys"] = "0",   // informational (transactional) message
                    ["message"] = new Dictionary<string, object>
                    {
                        ["text"] = text,
                        ["receipents"] = new Dictionary<string, object> { ["number"] = new[] { Number(e164) } }
                    }
                }
            };
            try
            {
                using var response = await PostAsync("/v1/send-sms/json", request, ct);
                var json = await NetgsmSmsGateway.ReadJsonAsync(response, ct);
                var code = StatusCode(json);
                if (code == 200)
                    return new GatewayResult(true, SmsStatus.Sent, json?["response"]?["order"]?["id"]?.ToString(), null);
                var error = $"İleti Merkezi {code?.ToString(CultureInfo.InvariantCulture) ?? ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture)}: {json?["response"]?["status"]?["message"]}";
                _logger.LogWarning("İleti Merkezi refused an SMS: {Error}", error);
                return new GatewayResult(false, SmsStatus.Failed, null, error);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "İleti Merkezi could not be reached");
                return new GatewayResult(false, SmsStatus.Failed, null, "İleti Merkezi: " + ex.GetType().Name);
            }
        }

        public static string MapStatus(int status) => status switch
        {
            111 => SmsStatus.Delivered,
            112 => SmsStatus.Failed,
            _ => SmsStatus.Sent
        };

        public async Task<GatewayStatus?> GetStatusAsync(string providerMessageId, DateTime sentAtUtc, CancellationToken ct = default)
        {
            var request = new Dictionary<string, object>
            {
                ["authentication"] = Authentication,
                ["order"] = new Dictionary<string, object> { ["id"] = providerMessageId, ["page"] = 1, ["rowCount"] = 100 }
            };
            try
            {
                using var response = await PostAsync("/v1/get-report/json", request, ct);
                var json = await NetgsmSmsGateway.ReadJsonAsync(response, ct);
                if (StatusCode(json) != 200)
                    return null;
                var messages = json?["response"]?["order"]?["message"];
                var first = messages is JsonArray a ? a.FirstOrDefault() : messages;
                if (first?["status"] is JsonNode s && int.TryParse(s.ToString(), out var status))
                    return new GatewayStatus(MapStatus(status), status == 112 ? "İleti Merkezi status 112" : null);
                return null;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "İleti Merkezi report could not be read");
                return null;
            }
        }
    }
}
