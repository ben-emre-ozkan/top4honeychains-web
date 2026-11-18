using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TOP4HoneyChains.Web.Services.Messaging
{
    // D-88: the phone page of the fake SMS provider (/phone/{key}/{number}): the test messages of one phone number, shown like a
    // messaging app, so a presenter's own phone can stand in for a beekeeper's or an officer's phone. It exists only while
    // Sms:Provider is Fake and never in Production; the link carries a random key (Sms:PhonePageKey, kept in dotnet user-secrets
    // by run-demo.ps1; without one a new key is made at every start and printed in the console) and shows only that number.
    public sealed class PhonePageKey
    {
        private static readonly Regex Shape = new("^[A-Za-z0-9_-]{16,64}$");

        public string Value { get; }
        // true when no usable key was configured and this start made one
        public bool Generated { get; }

        public PhonePageKey(string? configured)
        {
            if (configured != null && Shape.IsMatch(configured.Trim()))
                Value = configured.Trim();
            else
            {
                Value = NewKey();
                Generated = true;
            }
        }

        public static string NewKey() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(18)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

        public bool Matches(string? key) =>
            key != null && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(Value));
    }

    public static class PhonePage
    {
        public static bool Available(SmsOptions options, IHostEnvironment env) => options.IsFake && !env.IsProduction();

        // the number in the link: digits only with the country code (905550000101); null when it is not one
        public static string? E164(string? digits) =>
            digits != null && Regex.IsMatch(digits, "^[1-9][0-9]{9,14}$") ? "+" + digits : null;

        // +90 555 000 01 01 for a Turkish mobile number, otherwise the E.164 form
        public static string Pretty(string e164) =>
            Regex.IsMatch(e164, @"^\+90\d{10}$") ? $"+90 {e164[3..6]} {e164[6..9]} {e164[9..11]} {e164[11..13]}" : e164;

        public static string Link(string baseUrl, string key, string digits) => $"{baseUrl.TrimEnd('/')}/phone/{key}/{digits}";
    }
}
