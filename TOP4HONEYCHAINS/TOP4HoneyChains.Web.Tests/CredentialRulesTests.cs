using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-78 P1 (D78-SPEC 4.3): temporary passwords and the password policy
    public class CredentialRulesTests
    {
        [Fact]
        public void Temporary_passwords_are_ten_unambiguous_characters_and_vary()
        {
            var all = Enumerable.Range(0, 500).Select(_ => CredentialService.TemporaryPassword()).ToList();
            Assert.All(all, p =>
            {
                Assert.Equal(10, p.Length);
                Assert.DoesNotMatch("[0O1lI]", p);
            });
            Assert.Equal(all.Count, all.Distinct().Count());
        }

        [Theory]
        [InlineData("1234567890", false)]
        [InlineData("PASSWORD123", false)]
        [InlineData("aaaaaaaaaaaa", false)]
        [InlineData("ababababab", false)]
        [InlineData("My-zz_test_user-2026", false)]
        [InlineData("Bal-Kovan-2026-zz", true)]
        public async Task Policy_refuses_the_user_name_and_common_passwords(string password, bool ok)
        {
            var result = await new PasswordPolicyValidator().ValidateAsync(null!, new ApplicationUser { UserName = "zz_test_user" }, password);
            Assert.Equal(ok, result.Succeeded);
        }

        [Fact]
        public async Task A_short_user_name_is_only_refused_as_the_whole_password()
        {
            var v = new PasswordPolicyValidator();
            Assert.True((await v.ValidateAsync(null!, new ApplicationUser { UserName = "ab" }, "Tab-Kovan-2026")).Succeeded);
            Assert.False((await v.ValidateAsync(null!, new ApplicationUser { UserName = "abcdefghijk" }, "abcdefghijk")).Succeeded);
        }
    }
}
