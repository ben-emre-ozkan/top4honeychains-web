using System.Security.Cryptography;
using System.Text;

namespace TOP4HoneyChains.Web.Services.Messaging
{
    // D-78 P1 (D78-SPEC 5.1): SMS configuration, section "Sms". Credentials and the code key live in server-side configuration
    // (user-secrets locally), never in the repository.
    public static class SmsProviders
    {
        public const string Fake = "Fake";
        public const string Logging = "Logging";
        public const string Netgsm = "Netgsm";
        public const string IletiMerkezi = "IletiMerkezi";
        public static readonly string[] All = { Fake, Logging, Netgsm, IletiMerkezi };
    }

    public class SmsOptions
    {
        public string Provider { get; set; } = SmsProviders.Logging;
        // the registered sender name (originator), at most 11 characters
        public string Sender { get; set; } = "TOP4HONEY";
        // secret key (base64, 32 random bytes) for the HMAC of codes and phone numbers
        public string? CodeKey { get; set; }
        // hard cap on segments per UTC day for the whole platform
        public int DailyLimit { get; set; } = 100;
        // ISO country codes of the numbers that may receive SMS (O-13: TR only)
        public string AllowedCountries { get; set; } = "TR";
        // the fake provider outside Development (demo servers only)
        public bool AllowFake { get; set; }
        // with the fake provider: show the last message to the officer on the page (demos without a phone)
        public bool DemoPanel { get; set; }
        // minutes between delivery-report polls of a real provider; 0 switches polling off
        public int StatusPollMinutes { get; set; } = 5;

        public NetgsmOptions Netgsm { get; set; } = new();
        public IletiMerkeziOptions IletiMerkezi { get; set; } = new();

        public bool IsFake => string.Equals(Provider, SmsProviders.Fake, StringComparison.OrdinalIgnoreCase);

        public IReadOnlySet<string> AllowedCountrySet =>
            AllowedCountries.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim().ToUpperInvariant()).ToHashSet();

        // Development without a configured key uses a fixed development key; elsewhere the key is required (Validate)
        public byte[] KeyBytes()
        {
            if (!string.IsNullOrWhiteSpace(CodeKey))
            {
                try { return Convert.FromBase64String(CodeKey); }
                catch (FormatException) { return Encoding.UTF8.GetBytes(CodeKey); }
            }
            return SHA256.HashData(Encoding.UTF8.GetBytes("t4h-development-only-sms-key"));
        }

        // Configuration errors that stop the application from starting (Program.cs)
        public IEnumerable<string> Validate(bool isDevelopment)
        {
            if (!SmsProviders.All.Contains(Provider, StringComparer.OrdinalIgnoreCase))
                yield return $"Sms:Provider '{Provider}' is unknown (allowed: {string.Join(", ", SmsProviders.All)}).";
            if (IsFake && !isDevelopment && !AllowFake)
                yield return "Sms:Provider=Fake is refused outside Development unless Sms:AllowFake is true.";
            if (!isDevelopment && string.IsNullOrWhiteSpace(CodeKey))
                yield return "Sms:CodeKey is required (32 random bytes, base64) outside Development.";
            if (string.IsNullOrWhiteSpace(Sender) || Sender.Length > 11)
                yield return "Sms:Sender must have 1 to 11 characters.";
            if (DailyLimit <= 0)
                yield return "Sms:DailyLimit must be greater than 0.";
            if (string.Equals(Provider, SmsProviders.Netgsm, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(Netgsm.UserCode) || string.IsNullOrWhiteSpace(Netgsm.Password)))
                yield return "Sms:Netgsm:UserCode and Sms:Netgsm:Password are required for the Netgsm provider.";
            if (string.Equals(Provider, SmsProviders.IletiMerkezi, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(IletiMerkezi.Key) || string.IsNullOrWhiteSpace(IletiMerkezi.Hash)))
                yield return "Sms:IletiMerkezi:Key and Sms:IletiMerkezi:Hash are required for the İleti Merkezi provider.";
        }
    }

    public class NetgsmOptions
    {
        public string BaseUrl { get; set; } = "https://api.netgsm.com.tr";
        public string? UserCode { get; set; }
        public string? Password { get; set; }
        public string? AppName { get; set; }
        // one-segment code messages through the OTP route (/sms/rest/v2/otp), if the account has it
        public bool UseOtpRoute { get; set; }
    }

    public class IletiMerkeziOptions
    {
        public string BaseUrl { get; set; } = "https://api.iletimerkezi.com";
        public string? Key { get; set; }
        public string? Hash { get; set; }
    }
}
