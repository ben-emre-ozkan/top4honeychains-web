using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Memory;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace TOP4HoneyChains.Web.Services.OrgModel.TestSet
{
    // D-112 (owner request, 26 Nov 2026; a deliberate exception to D-88): the live site's test set, made and removed with one command
    // each, on the server, in the site folder (docs/KURULUM-TR.md B9):
    //   TOP4HoneyChains.Web.exe create-test-set [--out <folder>] [--sets N]
    //   TOP4HoneyChains.Web.exe remove-test-set [--commit]
    // Both read the site's settings like the site: appsettings*.json, the <environmentVariables> of the site's web.config, then the
    // environment variables of the window. create-test-set runs the application in memory (no network port), prints a short result
    // and writes the logins (a new random password per account) and the sets to the --out folder, never into the site folder; no
    // password is printed or logged. remove-test-set only counts without --commit.
    public static class TestSetCommand
    {
        public const string CreateName = "create-test-set";
        public const string RemoveName = "remove-test-set";
        public const int DefaultSets = 6;

        public static readonly string[] FeatureSwitches = { "OrgRoleModel", "CustodyIntake", "BeekeeperAccounts", "ConsumerAccounts" };

        public static bool IsCommand(string arg) => arg is CreateName or RemoveName;

        public sealed record Options(string Name, string? Out, int Sets, bool Commit, string[] HostArgs, string? Error);

        public static Options Parse(string[] args)
        {
            var name = args.Length > 0 ? args[0] : "";
            string? outDir = null, error = null;
            var sets = DefaultSets;
            var commit = false;
            var rest = new List<string>();
            for (var i = 1; i < args.Length; i++)
            {
                var a = args[i];
                string? Next() => i + 1 < args.Length ? args[++i] : null;
                if (a.Equals("--out", StringComparison.OrdinalIgnoreCase) && name == CreateName)
                    outDir = Next() ?? (error = "Give a folder after --out.");
                else if (a.Equals("--sets", StringComparison.OrdinalIgnoreCase) && name == CreateName)
                {
                    var n = Next();
                    if (!int.TryParse(n, NumberStyles.None, CultureInfo.InvariantCulture, out sets) || sets < 1 || sets > TestSetCreator.MaxSets)
                        error = $"--sets takes a number from 1 to {TestSetCreator.MaxSets}.";
                }
                else if (a.Equals("--commit", StringComparison.OrdinalIgnoreCase) && name == RemoveName)
                    commit = true;
                else if (a.StartsWith("--password", StringComparison.OrdinalIgnoreCase))
                    error = "No password is given to this command: every account gets a new random password.";
                else
                    rest.Add(a);   // e.g. --environment Production, passed to the configuration
            }
            return new Options(name, outDir, sets, commit, rest.ToArray(), error);
        }

        // the site's own settings: the <environmentVariables> of web.config in the site folder (IIS gives them only to the site)
        public static Dictionary<string, string?> WebConfigSettings(string contentRoot)
        {
            var settings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var file = Path.Combine(contentRoot, "web.config");
            if (!File.Exists(file))
                return settings;
            foreach (var e in XDocument.Load(file).Descendants("environmentVariable"))
                if (e.Attribute("name")?.Value is { Length: > 0 } n)
                    settings[n.Replace("__", ":")] = e.Attribute("value")?.Value;
            return settings;
        }

        // the folder for the logins: --out, or a new folder in the Documents folder of the person who runs it; never in the site folder
        public static (string? Folder, string? Error) OutFolder(string? requested, string contentRoot)
        {
            var folder = requested;
            if (string.IsNullOrWhiteSpace(folder))
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (string.IsNullOrEmpty(documents))
                    return (null, "This account has no Documents folder. Give a folder with --out.");
                folder = Path.Combine(documents, "TOP4-test-set-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture));
            }
            folder = Path.GetFullPath(folder);
            var root = Path.GetFullPath(contentRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if ((folder.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar).StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return (null, "The logins never go into the site folder. Give another folder with --out (the default is your Documents folder).");
            if (Directory.Exists(folder) && Directory.EnumerateFileSystemEntries(folder).Any())
                return (null, "The --out folder is not empty. Give a new or empty folder.");
            return (folder, null);
        }

        public static async Task<int> RunAsync(string[] args, TextWriter output, Action<IConfigurationBuilder>? configure = null, string? contentRoot = null)
        {
            var o = output;
            var options = Parse(args);
            if (options.Error != null)
            {
                o.WriteLine(options.Error);
                o.WriteLine($"Usage: TOP4HoneyChains.Web.exe {CreateName} [--out <folder>] [--sets <1-{TestSetCreator.MaxSets}, default {DefaultSets}>]");
                o.WriteLine($"       TOP4HoneyChains.Web.exe {RemoveName} [--commit]");
                return 2;
            }
            var root = contentRoot ?? AppContext.BaseDirectory;
            // the messages of this command are English on every server (docs/KURULUM-TR.md quotes them)
            var culture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            try
            {
                var site = WebConfigSettings(root);
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? site.GetValueOrDefault("ASPNETCORE_ENVIRONMENT");
                // the application's own assembly (its controllers and views), also when another program runs the command (the tests)
                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    Args = options.HostArgs, ContentRootPath = root, EnvironmentName = environment, ApplicationName = typeof(AppSetup).Assembly.GetName().Name
                });
                // web.config below the environment variables of the window
                var sources = builder.Configuration.Sources;
                var at = sources.ToList().FindIndex(s => s is EnvironmentVariablesConfigurationSource);
                sources.Insert(at < 0 ? sources.Count : at, new MemoryConfigurationSource { InitialData = site });
                configure?.Invoke(builder.Configuration);
                builder.Logging.ClearProviders();
                var connection = builder.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connection))
                {
                    o.WriteLine("No connection string. Run the command in the site folder (web.config) or set ConnectionStrings__DefaultConnection as for the site.");
                    return 3;
                }
                return options.Name == CreateName
                    ? await CreateAsync(builder, options, root, o)
                    : await RemoveAsync(builder, options, root, connection, o);
            }
            catch (SqlException e)
            {
                o.WriteLine("The database could not be used: " + e.Message);
                return 3;
            }
            finally
            {
                CultureInfo.CurrentUICulture = culture;
            }
        }

        // ---------------------------------------------------------------- create-test-set

        private static async Task<int> CreateAsync(WebApplicationBuilder builder, Options options, string root, TextWriter o)
        {
            var off = FeatureSwitches.Where(f => !builder.Configuration.GetValue<bool>("Features:" + f)).ToList();
            if (off.Count > 0)
            {
                o.WriteLine($"The test set needs all four feature switches. Off: {string.Join(", ", off.Select(f => "Features__" + f))}. Nothing was created.");
                return 3;
            }
            var (folder, folderError) = OutFolder(options.Out, root);
            if (folder == null)
            {
                o.WriteLine(folderError + " Nothing was created.");
                return 2;
            }

            // the application in memory: no network port, any host name, no console log
            builder.WebHost.UseTestServer();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["AllowedHosts"] = "*" });
            var localization = AppSetup.AddServices(builder);
            await using var app = builder.Build();
            // the pipeline sets the process-wide translation of country names; the one before comes back afterwards (the tests host
            // the site in the same process)
            var translate = Origin.Translate;
            AppSetup.UsePipeline(app, localization);
            try
            {
                return await CreateWithAppAsync(app, builder, options, folder, o);
            }
            finally
            {
                Origin.Translate = translate;
            }
        }

        private static async Task<int> CreateWithAppAsync(WebApplication app, WebApplicationBuilder builder, Options options, string folder, TextWriter o)
        {

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
                var orgs = await db.Organizations.CountAsync(x => x.IsTestData);
                var accounts = await db.TestSetAccounts.CountAsync();
                if (orgs > 0 || accounts > 0)
                {
                    o.WriteLine($"The test set already exists ({orgs} organizations, {accounts} accounts). Nothing was changed. To make a new one, remove it first with {RemoveName} --commit.");
                    return 4;
                }
                if (!await db.Organizations.AnyAsync(x => x.IsPlatform))
                {
                    o.WriteLine("The platform organization is missing: run db/install and every script of db/migrations first (docs/KURULUM-TR.md, part B). Nothing was created.");
                    return 3;
                }
                var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var taken = new List<string>();
                foreach (var name in TestSetCreator.UserNames(options.Sets))
                    if (await users.FindByNameAsync(name) != null)
                        taken.Add(name);
                if (taken.Count > 0)
                {
                    o.WriteLine($"These user names are already used by real accounts: {string.Join(", ", taken)}. Nothing was created.");
                    return 5;
                }
            }

            await app.StartAsync();
            TestSetResult result;
            try
            {
                var creator = new TestSetCreator(app.Services, () => app.GetTestServer().CreateHandler(), line => o.WriteLine("  " + line));
                o.WriteLine($"Creating the test set ({options.Sets} set(s)); this takes a minute or two.");
                result = await creator.CreateAsync(options.Sets);
            }
            catch (Exception e) when (e is InvalidOperationException or DbUpdateException or HttpRequestException)
            {
                o.WriteLine("The test set could not be completed: " + e.Message);
                o.WriteLine($"Remove what was made with {RemoveName} --commit, then run {CreateName} again.");
                return 6;
            }
            finally
            {
                await app.StopAsync();
            }

            var baseUrl = (builder.Configuration["App:PublicBaseUrl"] ?? "").TrimEnd('/');
            TestSetFiles.Write(folder, result, baseUrl);
            o.WriteLine($"The test set was created: 5 TEST organizations, {result.Logins.Count} accounts, {result.Sets.Count} set(s), batch \"{result.BatchTitle}\".");
            o.WriteLine("Logins and sets: " + folder);
            o.WriteLine("Share these files only with the testers. Remove the test set (remove-test-set --commit) before real users start.");
            return 0;
        }

        // ---------------------------------------------------------------- remove-test-set

        private static async Task<int> RemoveAsync(WebApplicationBuilder builder, Options options, string root, string connection, TextWriter o)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure<FeatureFlags>(builder.Configuration.GetSection("Features"));
            builder.Services.AddLocalization(x => x.ResourcesPath = "Resources");
            builder.Services.AddScoped<ICurrentOrg, CurrentOrg>();
            builder.Services.AddScoped<AuditInterceptor>();
            builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(connection));
            builder.Services.AddDbContext<TOP4HoneyChainsContext>((sp, x) => x.UseSqlServer(connection).AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
            builder.Services.AddAppIdentity();
            await using var app = builder.Build();
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
            var report = await new TestSetRemover(db, root).RunAsync(options.Commit);
            if (report.Counts.Count == 0)
            {
                o.WriteLine("There is no test set in this database. Nothing was changed.");
                return 0;
            }
            o.WriteLine($"Test set: {report.Total} rows in {report.Counts.Count} tables.");
            foreach (var c in report.Counts)
                o.WriteLine($"  {c.Table,-36} {c.Rows,6}");
            if (report.Blocks.Count > 0)
            {
                o.WriteLine($"Real records refer to the test set, so nothing was removed ({report.Blocks.Count} shown; table, key, column -> test record):");
                foreach (var b in report.Blocks)
                    o.WriteLine($"  {b.Table} {b.Key}: {b.Column} -> {b.RefersTo} {b.RefersToKey}");
                o.WriteLine("Check these records in the application (they were made by real accounts) and run the command again.");
                return 7;
            }
            o.WriteLine($"{report.AuditEntriesKept} audit entries of the test set stay: the audit log is append-only.");
            if (!report.Removed)
            {
                o.WriteLine($"Nothing was removed (counting only). Run {RemoveName} --commit to remove the test set.");
                return 0;
            }
            o.WriteLine($"The test set was removed ({report.FilesDeleted} stored files deleted). An audit entry records the removal.");
            return 0;
        }
    }

    // the --out folder: logins.csv, logins.txt and sets.csv (UTF-8; the CSV files for a spreadsheet and for the testers' guide)
    public static class TestSetFiles
    {
        public const string LoginsCsv = "test-set-logins.csv";
        public const string LoginsTxt = "test-set-logins.txt";
        public const string SetsCsv = "test-set-sets.csv";

        private static string Csv(params object?[] values) =>
            string.Join(",", values.Select(v => Convert.ToString(v, CultureInfo.InvariantCulture) ?? "").Select(v =>
                v.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + v.Replace("\"", "\"\"") + "\"" : v));

        public static void Write(string folder, TestSetResult r, string baseUrl)
        {
            Directory.CreateDirectory(folder);
            var utf8 = new UTF8Encoding(true);
            var logins = new StringBuilder();
            logins.AppendLine(Csv("username", "password", "person", "organization", "role", "login order"));
            foreach (var l in r.Logins)
                logins.AppendLine(Csv(l.UserName, l.Password, l.Person, l.Organization, l.Set is int s ? $"{l.Role} (set {s})" : l.Role, l.Order));
            File.WriteAllText(Path.Combine(folder, LoginsCsv), logins.ToString(), utf8);

            var sets = new StringBuilder();
            sets.AppendLine(Csv("set", "consumer username", "consumer", "beekeeper to register", "birth date", "registry number", "beekeeper phone", "apiary",
                "seal labels", "plastic seals", "association container"));
            foreach (var s in r.Sets)
                sets.AppendLine(Csv(s.No, s.ConsumerUser, s.ConsumerName, s.BeekeeperName, s.BeekeeperBirth, s.RegistryNumber, s.BeekeeperPhone, s.Apiary,
                    s.SealLabels, s.PlasticSeals, s.Container));
            File.WriteAllText(Path.Combine(folder, SetsCsv), sets.ToString(), utf8);

            var t = new StringBuilder();
            t.AppendLine("TOP4HoneyChains - test set of the live site (D-112)");
            t.AppendLine("====================================================");
            t.AppendLine();
            t.AppendLine($"Address:    {(baseUrl.Length > 0 ? baseUrl : "(the site's address)")}");
            t.AppendLine($"SMS:        every SMS to a test number (+90 512 000 ..) stays on the server. Read it in the test SMS inbox: {baseUrl}/TestInbox");
            t.AppendLine("            (signed in with any account below; platform administrators can open it too).");
            t.AppendLine($"History:    two deliveries of the registered beekeeper (lots {string.Join(", ", r.Lots.Select(l => "#" + l))}), batch \"{r.BatchTitle}\" (#{r.BatchId}) finalized,");
            t.AppendLine("            40 kg in stock at TEST Çarşı Market.");
            t.AppendLine($"            Honey Story: {baseUrl}/honeystory/{r.Qr.ToString().ToUpperInvariant()}");
            if (r.CompanyLabels != null)
                t.AppendLine($"Company:    seal labels {r.CompanyLabels} of TEST Örnek Bal İşleme A.Ş.");
            t.AppendLine();
            t.AppendLine($"{"Order",-5} {"User name",-18} {"Password",-20} {"Person",-16} {"Role",-38} Organization");
            t.AppendLine($"{new string('-', 5)} {new string('-', 18)} {new string('-', 20)} {new string('-', 16)} {new string('-', 38)} {new string('-', 12)}");
            foreach (var l in r.Logins)
                t.AppendLine($"{l.Order,-5} {l.UserName,-18} {l.Password,-20} {l.Person,-16} {(l.Set is int s ? $"{l.Role} (set {s})" : l.Role),-38} {l.Organization}");
            t.AppendLine();
            t.AppendLine("Sets (one per tester): the consumer account above, a beekeeper to register live, seal labels, plastic seals and a container of TEST Şile Arıcılar Birliği");
            foreach (var s in r.Sets)
            {
                t.AppendLine($"  Set {s.No}: consumer {s.ConsumerUser} ({s.ConsumerName})");
                t.AppendLine($"         beekeeper to register: {s.BeekeeperName}, born {s.BeekeeperBirth}, registry number {s.RegistryNumber}, phone {s.BeekeeperPhone}");
                t.AppendLine($"         apiary {s.Apiary}");
                t.AppendLine($"         seal labels {s.SealLabels}; plastic seals {s.PlasticSeals}; container {s.Container}");
            }
            t.AppendLine();
            t.AppendLine("Four-eyes pairs (always two different people): delivery hasan.demir -> CCP 0 mehmet.ozturk; lab report can.yildiz -> deniz.koc;");
            t.AppendLine("control sheets, New batch and finalizing zeynep.arslan -> mehmet.ozturk; shop storage gizem.polat -> okan.kurt.");
            t.AppendLine();
            t.AppendLine("Keep this folder private and share it only with the testers. Remove the test set before real users start");
            t.AppendLine("(TOP4HoneyChains.Web.exe remove-test-set --commit, docs/KURULUM-TR.md B9).");
            File.WriteAllText(Path.Combine(folder, LoginsTxt), t.ToString(), utf8);
        }
    }
}
