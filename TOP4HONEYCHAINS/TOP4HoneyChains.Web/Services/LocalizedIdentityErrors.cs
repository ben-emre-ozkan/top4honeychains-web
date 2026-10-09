using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace TOP4HoneyChains.Web.Services
{
    // D-75: the account errors of ASP.NET Identity (password rules, taken user names, expired reset links) in the user's
    // language. The English text is the resource key, as for the other server messages.
    public class LocalizedIdentityErrors : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<SharedResource> _l;

        public LocalizedIdentityErrors(IStringLocalizer<SharedResource> localizer) => _l = localizer;

        private IdentityError E(string code, string key, params object[] args) => new() { Code = code, Description = _l[key, args].Value };

        public override IdentityError DefaultError() => E(nameof(DefaultError), "An unknown error has occurred.");
        public override IdentityError ConcurrencyFailure() => E(nameof(ConcurrencyFailure), "The record was changed by someone else; reload the page and try again.");
        public override IdentityError PasswordMismatch() => E(nameof(PasswordMismatch), "Incorrect password.");
        public override IdentityError InvalidToken() => E(nameof(InvalidToken), "The link is invalid or has expired. Request a new password reset.");
        public override IdentityError InvalidUserName(string? userName) => E(nameof(InvalidUserName), "The user name {0} is invalid: use letters, digits and - . _ @ + only.", userName ?? "");
        public override IdentityError InvalidEmail(string? email) => E(nameof(InvalidEmail), "The e-mail address {0} is invalid.", email ?? "");
        public override IdentityError DuplicateUserName(string userName) => E(nameof(DuplicateUserName), "The user name {0} is already taken.", userName);
        public override IdentityError DuplicateEmail(string email) => E(nameof(DuplicateEmail), "The e-mail address {0} is already used.", email);
        public override IdentityError PasswordTooShort(int length) => E(nameof(PasswordTooShort), "The password must be at least {0} characters long.", length);
        public override IdentityError PasswordRequiresNonAlphanumeric() => E(nameof(PasswordRequiresNonAlphanumeric), "The password must contain at least one symbol.");
        public override IdentityError PasswordRequiresDigit() => E(nameof(PasswordRequiresDigit), "The password must contain at least one digit.");
        public override IdentityError PasswordRequiresLower() => E(nameof(PasswordRequiresLower), "The password must contain at least one lowercase letter.");
        public override IdentityError PasswordRequiresUpper() => E(nameof(PasswordRequiresUpper), "The password must contain at least one uppercase letter.");
        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => E(nameof(PasswordRequiresUniqueChars), "The password must contain at least {0} different characters.", uniqueChars);
    }
}
