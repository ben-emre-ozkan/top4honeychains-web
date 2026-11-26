using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-98: the Identity settings of the application, shared by the web application and the create-first-admin command
    public static class AppIdentity
    {
        public static IdentityBuilder AddAppIdentity(this IServiceCollection services) =>
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 10;   // D-78 (D78-SPEC 4.3): at least 10 characters for new passwords; plus PasswordPolicyValidator

                // Lock an account for 15 minutes after 5 failed sign-ins (brute-force protection)
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<LocalizedIdentityErrors>()   // D-75: account errors in the user's language
            .AddPasswordValidator<PasswordPolicyValidator>()   // D-78: not the user name, not a common password
            .AddDefaultTokenProviders();
    }

    public record FirstAdminResult(bool Ok, string Message, string? UserId = null);

    // D-98: creates the first platform administrator of a new installation. Works only while no account is in the Administrator
    // role; applies the password rules of the application; gives the platform organization's OrgAdmin membership that migration 004
    // gives every administrator (creating the platform organization like 004 when it is missing) and writes an audit entry.
    public class FirstAdminService
    {
        public const string AuditAction = "CreateFirstAdmin";

        private readonly UserManager<ApplicationUser> _users;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly TOP4HoneyChainsContext _context;

        public FirstAdminService(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, TOP4HoneyChainsContext context)
        {
            _users = users;
            _roles = roles;
            _context = context;
        }

        public async Task<bool> AdministratorExistsAsync() =>
            await _roles.RoleExistsAsync(CurrentOrg.AdminRole) && (await _users.GetUsersInRoleAsync(CurrentOrg.AdminRole)).Count > 0;

        public async Task<FirstAdminResult> CreateAsync(string? userName, string? phone, string? email, string password)
        {
            if (await AdministratorExistsAsync())
                return Fail("An administrator already exists. This command only sets up a new database; further administrators are added by an administrator under User management.");

            userName = userName?.Trim();
            if (string.IsNullOrEmpty(userName))
                return Fail("Give the user name with --user.");
            if (string.IsNullOrWhiteSpace(phone))
                return Fail("Give the administrator's mobile phone number with --phone, for example --phone +905321234567.");
            var e164 = PhoneNumberPolicy.Normalize(phone, out var phoneError);
            if (e164 == null)
                return Fail(phoneError ?? PhoneNumberPolicy.InvalidMessage);
            email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            if (email != null && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
                return Fail("The e-mail address is not valid.");
            if (await _users.FindByNameAsync(userName) != null)
                return Fail("A user with this name already exists.");

            var user = new ApplicationUser { UserName = userName, Email = email, PhoneNumber = e164 };

            // the password rules first, so nothing is written for a refused password
            var errors = new List<string>();
            foreach (var validator in _users.UserValidators)
                errors.AddRange((await validator.ValidateAsync(_users, user)).Errors.Select(e => e.Description));
            foreach (var validator in _users.PasswordValidators)
                errors.AddRange((await validator.ValidateAsync(_users, user, password)).Errors.Select(e => e.Description));
            if (errors.Count > 0)
                return Fail(string.Join(" ", errors.Distinct()));

            if (!await _roles.RoleExistsAsync(CurrentOrg.AdminRole))
            {
                var role = await _roles.CreateAsync(new IdentityRole(CurrentOrg.AdminRole));
                if (!role.Succeeded)
                    return Fail(string.Join(" ", role.Errors.Select(e => e.Description)));
            }

            var created = await _users.CreateAsync(user, password);
            if (!created.Succeeded)
                return Fail(string.Join(" ", created.Errors.Select(e => e.Description)));
            try
            {
                var inRole = await _users.AddToRoleAsync(user, CurrentOrg.AdminRole);
                if (!inRole.Succeeded)
                    throw new InvalidOperationException(string.Join(" ", inRole.Errors.Select(e => e.Description)));

                // migration 004: the platform organization with its capability, and an OrgAdmin membership for every administrator
                var platform = await _context.Organizations.FirstOrDefaultAsync(o => o.IsPlatform);
                if (platform == null)
                {
                    platform = new Organization { DisplayName = "Platform administration", Status = OrgStatus.Verified, IsPlatform = true, VerifiedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
                    _context.Organizations.Add(platform);
                    await _context.SaveChangesAsync();
                }
                if (!await _context.OrgCapabilities.AnyAsync(c => c.OrgId == platform.OrgId && c.Capability == Capabilities.PlatformAdmin))
                    _context.OrgCapabilities.Add(new OrgCapability { OrgId = platform.OrgId, Capability = Capabilities.PlatformAdmin, AddedAt = DateTime.UtcNow });
                if (!await _context.Memberships.AnyAsync(m => m.UserId == user.Id && m.OrgId == platform.OrgId && m.OrgRole == OrgRoles.OrgAdmin))
                    _context.Memberships.Add(new Membership { UserId = user.Id, OrgId = platform.OrgId, OrgRole = OrgRoles.OrgAdmin, Status = MembershipStatus.Active, GrantedAt = DateTime.UtcNow });
                await _context.SaveChangesAsync();

                await AuditChain.AppendAsync(_context, new[]
                {
                    new AuditRecord(DateTime.UtcNow, user.Id, user.UserName, platform.OrgId, AuditAction, "AspNetUsers", user.Id, null,
                        JsonSerializer.Serialize(new { user.UserName, Role = CurrentOrg.AdminRole, Membership = OrgRoles.OrgAdmin, Via = FirstAdminCommand.Name }),
                        null, null)
                });
                return new FirstAdminResult(true, $"The administrator {user.UserName} was created with an Organization admin membership in \"{platform.DisplayName}\".", user.Id);
            }
            catch
            {
                // no half-made administrator: without the account the command can run again
                _context.ChangeTracker.Clear();
                await _context.Memberships.Where(m => m.UserId == user.Id).ExecuteDeleteAsync();
                await _users.DeleteAsync(user);
                throw;
            }
        }

        private static FirstAdminResult Fail(string message) => new(false, message);
    }

    // Reads a secret from the person at the console without showing it
    public interface ISecretReader
    {
        bool Interactive { get; }
        string Read(string prompt);
    }

    public sealed class ConsoleSecretReader : ISecretReader
    {
        public bool Interactive => !Console.IsInputRedirected;

        public string Read(string prompt)
        {
            Console.Write(prompt);
            var text = new System.Text.StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (text.Length > 0)
                        text.Length--;
                    continue;
                }
                if (!char.IsControl(key.KeyChar))
                    text.Append(key.KeyChar);
            }
            Console.WriteLine();
            return text.ToString();
        }
    }

    // D-98: TOP4HoneyChains.Web.exe create-first-admin --user <name> --phone <+90...> [--email <address>]
    // Asks twice for the password without showing it (never from a file, an argument or redirected input), creates the first
    // platform administrator with FirstAdminService and exits without starting the web server. It reads the configuration like the
    // application (appsettings.json, appsettings.<environment>.json, environment variables) but needs only the connection string;
    // the SMS settings are not read.
    public static class FirstAdminCommand
    {
        public const string Name = "create-first-admin";

        public static async Task<int> RunAsync(string[] args, ISecretReader secrets, TextWriter? output = null,
            Action<IConfigurationBuilder>? configure = null)
        {
            var o = output ?? Console.Out;
            string? user = null, phone = null, email = null;
            var rest = new List<string>();
            for (var i = 0; i < args.Length; i++)
            {
                var a = args[i];
                string? Next() => i + 1 < args.Length ? args[++i] : null;
                if (a.StartsWith("--password", StringComparison.OrdinalIgnoreCase) || a.StartsWith("/password", StringComparison.OrdinalIgnoreCase))
                {
                    o.WriteLine("The password is never given on the command line. Leave it out: the command asks for it.");
                    return 2;
                }
                if (a.Equals("--user", StringComparison.OrdinalIgnoreCase)) user = Next();
                else if (a.Equals("--phone", StringComparison.OrdinalIgnoreCase)) phone = Next();
                else if (a.Equals("--email", StringComparison.OrdinalIgnoreCase)) email = Next();
                else rest.Add(a);   // e.g. --environment Production, passed to the configuration
            }
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(phone))
            {
                o.WriteLine($"Usage: TOP4HoneyChains.Web.exe {Name} --user <user name> --phone <mobile number, e.g. +905321234567> [--email <address>]");
                o.WriteLine("The password is asked for twice and not shown. Run it in the site folder with the site's connection string (docs/KURULUM-TR.md, part B).");
                return 2;
            }
            if (!secrets.Interactive)
            {
                o.WriteLine("Run this command in an interactive window: the password is typed in, never read from a file or a pipe.");
                return 2;
            }

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = rest.ToArray(), ContentRootPath = AppContext.BaseDirectory });
            configure?.Invoke(builder.Configuration);
            builder.Logging.ClearProviders();
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connection))
            {
                o.WriteLine("No connection string. Set ConnectionStrings__DefaultConnection (or appsettings.Production.json) as for the site.");
                return 3;
            }
            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure<FeatureFlags>(builder.Configuration.GetSection("Features"));
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));
            builder.Services.AddScoped<ICurrentOrg, CurrentOrg>();
            builder.Services.AddScoped<AuditInterceptor>();
            builder.Services.AddDbContext<TOP4HoneyChainsContext>((sp, options) => options.UseSqlServer(connection).AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
            builder.Services.AddAppIdentity();
            builder.Services.AddScoped<FirstAdminService>();
            await using var app = builder.Build();

            using var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<FirstAdminService>();
            // the messages of this command are English on every server (docs/KURULUM-TR.md quotes them)
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("en");
            try
            {
                // refuse before the password is asked for
                if (await service.AdministratorExistsAsync())
                {
                    o.WriteLine("An administrator already exists. This command only sets up a new database; further administrators are added by an administrator under User management.");
                    return 4;
                }
                var password = secrets.Read("Password: ");
                if (password != secrets.Read("Password again: "))
                {
                    o.WriteLine("The two passwords are different. Nothing was created.");
                    return 5;
                }
                var result = await service.CreateAsync(user, phone, email, password);
                o.WriteLine(result.Ok ? result.Message + " Sign in with it and verify the phone number under Profile." : result.Message + " Nothing was created.");
                return result.Ok ? 0 : 6;
            }
            catch (SqlException e)
            {
                o.WriteLine("The database could not be used: " + e.Message);
                o.WriteLine("Check the connection string and that db/install and db/migrations have run (docs/KURULUM-TR.md, part B).");
                return 3;
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentUICulture = culture;
            }
        }
    }
}
