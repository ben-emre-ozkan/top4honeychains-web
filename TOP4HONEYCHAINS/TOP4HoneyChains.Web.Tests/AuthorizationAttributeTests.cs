using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-47: every controller action states its access explicitly: [Authorize] with a Policy or Roles (on the
    // action or its controller), or [AllowAnonymous]. Anonymous actions are limited to a known list.
    public class AuthorizationAttributeTests
    {
        // Public by design: sign-in, password reset, public Honey Story, storefront, report verification, language, consumer sign-up and the consumer's private story state (D-87)
        private static readonly HashSet<string> PublicActions = new()
        {
            "Account.Login", "Account.ForgotPassword", "Account.ResetPassword", "Account.AccessDenied",
            "Dashboard.Error", "Dashboard.Status", "Home.Index", "Home.SetLanguage",
            "HoneyStory.ParameterDetails", "HoneyStory.Index", "HoneyStory.BatchQRCode", "HoneyStory.Image", "HoneyStory.Details",
            "Storefront.Index", "Storefront.Qr", "Verify.Index", "Notices.Public", "Consumer.SignUp", "Consumer.State", "Account.SendResetCode", "Account.ResetWithCode",
            // D-78 P4: the scene gallery, answered only in Development (404 elsewhere)
            "Dev.Scenes",
            // D-88: the phone page of the fake SMS provider: only with the fake provider, never in Production, only with the key (404 otherwise)
            "Phone.Index", "Phone.Messages"
        };

        public static IEnumerable<object[]> Actions() =>
            typeof(Program).Assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName && m.GetCustomAttribute<NonActionAttribute>() == null)
                    .Select(m => new object[] { t.Name.Replace("Controller", "") + "." + m.Name + "(" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) + ")" }))
                .DistinctBy(x => (string)x[0]);

        private static MethodInfo Find(string action)
        {
            var controller = action[..action.IndexOf('.')] + "Controller";
            var type = typeof(Program).Assembly.GetTypes().Single(t => t.Name == controller);
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .First(m => action == type.Name.Replace("Controller", "") + "." + m.Name + "(" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
        }

        [Theory]
        [MemberData(nameof(Actions))]
        public void Every_action_has_an_explicit_policy_or_is_a_known_public_page(string action)
        {
            var method = Find(action);
            var type = method.DeclaringType!;
            var anonymous = method.GetCustomAttribute<AllowAnonymousAttribute>() ?? type.GetCustomAttribute<AllowAnonymousAttribute>(true);
            var authorize = method.GetCustomAttributes<AuthorizeAttribute>().Concat(type.GetCustomAttributes<AuthorizeAttribute>(true)).ToList();
            var name = action[..action.IndexOf('(')];

            if (anonymous != null)
            {
                Assert.True(PublicActions.Contains(name), $"{action} is anonymous but not in the list of public pages");
                Assert.True(method.GetCustomAttribute<AuthorizeAttribute>() == null || method.GetCustomAttribute<AllowAnonymousAttribute>() != null,
                    $"{action}: [Authorize] on the action is ignored because the controller is [AllowAnonymous]");
                return;
            }
            Assert.True(authorize.Count > 0, $"{action} has no [Authorize] or [AllowAnonymous]");
            Assert.True(authorize.All(a => !string.IsNullOrEmpty(a.Policy) || !string.IsNullOrEmpty(a.Roles)),
                $"{action} uses [Authorize] without a Policy or Roles");
        }

        [Fact]
        public void The_list_of_public_pages_has_no_stale_entries()
        {
            var actions = Actions().Select(a => ((string)a[0])[..((string)a[0]).IndexOf('(')]).ToHashSet();
            Assert.All(PublicActions, p => Assert.Contains(p, actions));
        }
    }
}
