using PhoneNumbers;

namespace TOP4HoneyChains.Web.Services
{
    // Beekeeper phone numbers (D-78): validated with libphonenumber and stored in the international E.164 form
    // (+905321234567), so the platform works with numbers from every partner country. A number written without
    // a country code ("0532 123 45 67") is read as a Turkish number. Numbers stored before D-78 are not rewritten:
    // they are only normalized when someone changes them.
    public static class PhoneNumberPolicy
    {
        public const string DefaultRegion = "TR";
        public const string InvalidMessage = "Enter a valid phone number with its country code, for example +90 532 123 45 67.";

        private static readonly PhoneNumberUtil Util = PhoneNumberUtil.GetInstance();

        // Returns the E.164 form, null for an empty input, or sets error when the number is not valid.
        public static string? Normalize(string? input, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(input))
                return null;
            var text = input.Trim();
            if (text.StartsWith("00"))
                text = "+" + text[2..];   // international prefix written as 00
            try
            {
                var number = Util.Parse(text, DefaultRegion);
                if (Util.IsValidNumber(number))
                    return Util.Format(number, PhoneNumberFormat.E164);
            }
            catch (NumberParseException)
            {
                // falls through to the error below
            }
            error = InvalidMessage;
            return null;
        }

        // For display: the international form with spaces (+90 532 123 45 67); anything that does not parse is shown as stored.
        public static string Display(string? stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
                return string.Empty;
            try
            {
                var number = Util.Parse(stored, DefaultRegion);
                return Util.IsValidNumber(number) ? Util.Format(number, PhoneNumberFormat.INTERNATIONAL) : stored;
            }
            catch (NumberParseException)
            {
                return stored;
            }
        }
    }
}
