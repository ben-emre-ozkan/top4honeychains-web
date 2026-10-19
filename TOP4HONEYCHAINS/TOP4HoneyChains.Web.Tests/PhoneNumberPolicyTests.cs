using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-78: beekeeper phone numbers are validated and stored in the international E.164 form
    public class PhoneNumberPolicyTests
    {
        [Theory]
        [InlineData("0532 123 45 67", "+905321234567")]      // Turkish number without a country code
        [InlineData("+90 (532) 123-45-67", "+905321234567")]
        [InlineData("5321234567", "+905321234567")]
        [InlineData("+48 512 345 678", "+48512345678")]       // Poland
        [InlineData("0048 512 345 678", "+48512345678")]      // international prefix 00
        [InlineData("+371 2123 4567", "+37121234567")]        // Latvia
        [InlineData("+54 9 11 2345 6789", "+5491123456789")]  // Argentina (mobile)
        public void Valid_numbers_are_stored_in_E164(string input, string expected)
        {
            Assert.Equal(expected, PhoneNumberPolicy.Normalize(input, out var error));
            Assert.Null(error);
        }

        [Theory]
        [InlineData("12")]
        [InlineData("+90 123")]
        [InlineData("phone")]
        public void Invalid_numbers_are_refused(string input)
        {
            Assert.Null(PhoneNumberPolicy.Normalize(input, out var error));
            Assert.Equal(PhoneNumberPolicy.InvalidMessage, error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void An_empty_number_is_allowed(string? input)
        {
            Assert.Null(PhoneNumberPolicy.Normalize(input, out var error));
            Assert.Null(error);
        }

        [Theory]
        [InlineData("+905321234567", "+90 532 123 45 67")]
        [InlineData("05321234567", "+90 532 123 45 67")]   // stored before D-78: shown readable, not rewritten
        [InlineData("not a number", "not a number")]
        public void Stored_numbers_are_displayed_in_the_international_form(string stored, string shown)
        {
            Assert.Equal(shown, PhoneNumberPolicy.Display(stored));
        }
    }
}
