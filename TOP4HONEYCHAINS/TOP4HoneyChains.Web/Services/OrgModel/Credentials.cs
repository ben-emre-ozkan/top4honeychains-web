using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.Messaging;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P1 (D78-SPEC 4.3): sign-in details issued by whoever registers the user. The temporary password is generated here,
    // sent by SMS (M2) and never shown to the issuer; it is valid 72 hours and must be changed at the first sign-in.
    public class CredentialService
    {
        public static readonly TimeSpan Validity = TimeSpan.FromHours(72);
        // no 0/O, 1/l/I: 55 symbols, 10 characters = about 58 bits
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        public const int TemporaryLength = 10;

        public record IssueResult(bool Ok, string? Error);
        public record Pending(UserCredentialIssue Issue, bool Expired);

        private readonly TOP4HoneyChainsContext _context;
        private readonly UserManager<ApplicationUser> _users;
        private readonly IMessageService _messages;
        private readonly TimeProvider _clock;

        public CredentialService(TOP4HoneyChainsContext context, UserManager<ApplicationUser> users, IMessageService messages, TimeProvider clock)
        {
            _context = context;
            _users = users;
            _messages = messages;
            _clock = clock;
        }

        private DateTime Now => _clock.GetUtcNow().UtcDateTime;

        public static string TemporaryPassword() =>
            new(Enumerable.Range(0, TemporaryLength).Select(_ => Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]).ToArray());

        // a random password nobody knows, for an account created before its sign-in details are issued
        public static string Unusable() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        // Sets a new temporary password and sends it with the user name by SMS. Earlier temporary passwords stop working.
        public async Task<IssueResult> IssueBySmsAsync(ApplicationUser user, string phone, string? language, string? issuerUserId, string signInLink)
        {
            var (e164, phoneError) = _messages.CheckPhone(phone);
            if (e164 == null)
                return new IssueResult(false, phoneError);
            var password = TemporaryPassword();
            var reset = await _users.ResetPasswordAsync(user, await _users.GeneratePasswordResetTokenAsync(user), password);
            if (!reset.Succeeded)
                return new IssueResult(false, string.Join(" ", reset.Errors.Select(e => e.Description)));

            var sms = await _messages.SendTextAsync(SmsPurpose.Credentials, "User", user.Id, e164, language,
                new Dictionary<string, string> { ["user"] = user.UserName!, ["password"] = password, ["link"] = signInLink },
                new[] { password }, issuerUserId);
            if (!sms.Ok)
                return new IssueResult(false, sms.Error);

            // the number is confirmed when the user signs in with the password it received
            user.PhoneNumber = e164;
            user.PhoneNumberConfirmed = false;
            await _users.UpdateAsync(user);
            await _users.SetLockoutEndDateAsync(user, null);
            await _users.ResetAccessFailedCountAsync(user);
            _context.UserCredentialIssues.Add(new UserCredentialIssue
            {
                UserId = user.Id, IssuedByUserId = issuerUserId, IssuedAt = Now, Channel = CredentialChannel.Sms, SmsId = sms.SmsId, ExpiresAt = Now + Validity
            });
            await _context.SaveChangesAsync();
            return new IssueResult(true, null);
        }

        // An administrator set the password by hand (no SMS provider yet, or a test account): it must be changed at first sign-in
        public async Task RecordManualAsync(string userId, string? issuerUserId)
        {
            _context.UserCredentialIssues.Add(new UserCredentialIssue
            {
                UserId = userId, IssuedByUserId = issuerUserId, IssuedAt = Now, Channel = CredentialChannel.Manual, ExpiresAt = Now + Validity
            });
            await _context.SaveChangesAsync();
        }

        // the latest issue while its password has not been changed yet
        public async Task<Pending?> PendingAsync(string userId)
        {
            var latest = await _context.UserCredentialIssues.AsNoTracking().Where(i => i.UserId == userId)
                .OrderByDescending(i => i.IssueId).FirstOrDefaultAsync();
            return latest == null || latest.PasswordChangedAt != null ? null : new Pending(latest, Now > latest.ExpiresAt);
        }

        // The user chose a password: the issue is done; an SMS issue also proves the phone number
        public async Task CompleteAsync(ApplicationUser user)
        {
            var open = await _context.UserCredentialIssues.Where(i => i.UserId == user.Id && i.PasswordChangedAt == null).ToListAsync();
            if (open.Count == 0)
                return;
            foreach (var i in open)
                i.PasswordChangedAt = Now;
            if (open.Any(i => i.Channel == CredentialChannel.Sms) && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                user.PhoneNumberConfirmed = true;
                await _users.UpdateAsync(user);
                await MarkPhoneVerifiedAsync(user.Id, null);
            }
            await _context.SaveChangesAsync();
        }

        public async Task MarkPhoneVerifiedAsync(string userId, long? codeId)
        {
            var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null)
                _context.UserSettings.Add(settings = new UserSetting { UserId = userId });
            settings.PhoneVerifiedAt = Now;
            settings.PhoneCodeId = codeId;
            await _context.SaveChangesAsync();
        }
    }

    // D-78 P1 (D78-SPEC 4.3 step 6): at least 10 characters (Identity options), not the user name, not a common password.
    // Applies to new passwords only; existing passwords stay valid.
    public class PasswordPolicyValidator : IPasswordValidator<ApplicationUser>
    {
        private static readonly HashSet<string> Common = new(StringComparer.OrdinalIgnoreCase)
        {
            "1234567890", "12345678910", "0123456789", "0987654321", "1111111111", "1234512345", "1122334455", "1q2w3e4r5t", "1qaz2wsx3edc",
            "qwertyuiop", "asdfghjkl1", "zxcvbnm123", "password12", "password123", "password1!", "passw0rd12", "parola1234", "sifre12345",
            "şifre12345", "sifresifre", "123456789a", "a123456789", "abcdefghij", "abcd123456", "qwerty1234", "qwerty12345", "iloveyou12",
            "admin12345", "administrator", "welcome123", "letmein123", "changeme12", "galatasaray", "fenerbahce", "besiktas1903",
            "fenerbahce1907", "galatasaray1905", "trabzonspor", "istanbul34", "ankara0606", "turkiye1923", "top4honey1", "top4honeychains",
            "honeyhoney", "balbalbal1", "aricilik12", "contraseña1", "contrasena", "haslo12345", "parole1234"
        };

        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
        {
            var errors = new List<IdentityError>();
            if (!string.IsNullOrEmpty(password))
            {
                var name = user.UserName ?? "";
                if (name.Length > 0 && (name.Length >= 4 ? password.Contains(name, StringComparison.OrdinalIgnoreCase) : string.Equals(password, name, StringComparison.OrdinalIgnoreCase)))
                    errors.Add(new IdentityError { Code = "PasswordContainsUserName", Description = "The password must not contain the user name." });
                if (Common.Contains(password) || password.Distinct().Count() <= 2)
                    errors.Add(new IdentityError { Code = "PasswordTooCommon", Description = "This password is too common. Choose another one." });
            }
            return Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
        }
    }
}
