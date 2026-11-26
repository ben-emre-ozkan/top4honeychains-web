using System.Globalization;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.Messaging
{
    // D-112: the phone numbers of the live site's test set. +90 512 000 xx xx are Turkish pager numbers: libphonenumber accepts them
    // (type PAGER), so every form takes them, but the pager service has ended and no one can receive an SMS on them. An SMS to such a
    // number never reaches a provider, whatever Sms:Provider says: MessageService stores it with the provider "TestInbox" and the full
    // text, and the signed-in test inbox (/TestInbox) shows it. Only these numbers are ever diverted, so the inbox only ever holds SMS
    // to test numbers.
    public static class TestPhones
    {
        public const string Prefix = "+90512000";
        public const string Provider = "TestInbox";

        public static bool IsTest(string? e164) =>
            e164 != null && e164.Length == Prefix.Length + 4 && e164.StartsWith(Prefix, StringComparison.Ordinal) && e164[1..].All(char.IsAsciiDigit);

        // the number of a group (00 = the shared cast, 01..99 = a tester's set) and a person in it
        public static string Number(int group, int person) =>
            Prefix + group.ToString("D2", CultureInfo.InvariantCulture) + person.ToString("D2", CultureInfo.InvariantCulture);

        // every number of the block (for the inbox: the stored phone hash back to the number)
        public static IEnumerable<string> All() => Enumerable.Range(0, 10000).Select(i => Prefix + i.ToString("D4", CultureInfo.InvariantCulture));
    }

    // what "sends" an SMS to a test number: nothing leaves the server
    public sealed class TestInboxGateway : ISmsGateway
    {
        public static readonly TestInboxGateway Instance = new();
        public string Name => TestPhones.Provider;

        public Task<GatewayResult> SendAsync(string e164, string text, bool isCode, CancellationToken ct = default) =>
            Task.FromResult(new GatewayResult(true, SmsStatus.Fake, "testinbox-" + Guid.NewGuid().ToString("N"), null));

        public Task<GatewayStatus?> GetStatusAsync(string providerMessageId, DateTime sentAtUtc, CancellationToken ct = default) =>
            Task.FromResult<GatewayStatus?>(null);
    }
}
