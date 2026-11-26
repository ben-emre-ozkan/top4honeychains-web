using System.Net;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.Messaging;
using TOP4HoneyChains.Web.Services.OrgModel;
using TOP4HoneyChains.Web.Services.OrgModel.TestSet;
using TOP4HoneyChains.Web.Tests.Demo;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-112: the live site's test set on a new installation (db/install and every migration, nothing copied from the development
    // database), with its first administrator. create-test-set runs as on the server: from a site folder with a web.config that holds
    // the site's settings (Production, a real SMS provider that cannot be reached), so every SMS of the run must stay in the test inbox.
    public sealed class TestSetApp : E2EApp
    {
        public static readonly string TestSetDatabase = Database + "_TestSet";
        protected override string DatabaseName => TestSetDatabase;
        public const string Admin = "zz_test_first_admin";

        // the site folder of the command (web.config, App_Data) and its SMS key
        public string SiteFolder { get; } = Path.Combine(Path.GetTempPath(), "t4h-testset-site-" + Guid.NewGuid().ToString("N"));
        public string CodeKey { get; } = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        protected override void AddSettings(Dictionary<string, string?> settings)
        {
            settings["Features:CustodyIntake"] = "true";
            settings["Features:BeekeeperAccounts"] = "true";
            settings["Features:ConsumerAccounts"] = "true";
            settings["Sms:CodeKey"] = CodeKey;
        }

        public override async Task InitializeAsync()
        {
            await DropDatabaseAsync();
            await MasterAsync($"CREATE DATABASE [{TestSetDatabase}]");
            foreach (var file in new[] { "01_schema.sql", "02_reference_data.sql" })
                await DemoReference.RunScriptAsync(ConnectionString, await File.ReadAllTextAsync(DemoReference.RepoFile("db", "install", file)));
            foreach (var file in Directory.GetFiles(DemoReference.RepoFile("db", "migrations"), "*.sql").OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal))
                await DemoReference.RunScriptAsync(ConnectionString, await File.ReadAllTextAsync(file));
            var output = new StringWriter();
            var made = await FirstAdminCommand.RunAsync(new[] { "--user", Admin, "--phone", "+905550000199" }, new ScriptedSecrets(true, Password, Password), output,
                config => config.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = ConnectionString }));
            Assert.True(made == 0, output.ToString());
            Directory.CreateDirectory(SiteFolder);
            WriteWebConfig(true);
        }

        public void WriteWebConfig(bool consumerAccounts)
        {
            var settings = new Dictionary<string, string>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production",
                ["ConnectionStrings__DefaultConnection"] = ConnectionString,
                ["App__PublicBaseUrl"] = "https://top4.example.invalid",
                ["AllowedHosts"] = "top4.example.invalid",
                ["Features__OrgRoleModel"] = "true", ["Features__CustodyIntake"] = "true", ["Features__BeekeeperAccounts"] = "true",
                ["Features__ConsumerAccounts"] = consumerAccounts ? "true" : "false",
                // a real provider that cannot be reached: an SMS that left the server would fail
                ["Sms__Provider"] = "Netgsm", ["Sms__Netgsm__UserCode"] = "zz_test", ["Sms__Netgsm__Password"] = "zz_test_not_a_secret",
                ["Sms__Netgsm__BaseUrl"] = "http://127.0.0.1:9", ["Sms__CodeKey"] = CodeKey,
                ["Logging__LogLevel__Default"] = "Information"
            };
            var lines = settings.Select(s => $"        <environmentVariable name=\"{s.Key}\" value=\"{System.Security.SecurityElement.Escape(s.Value)}\" />");
            File.WriteAllText(Path.Combine(SiteFolder, "web.config"),
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<configuration><location path=\".\" inheritInChildApplications=\"false\"><system.webServer>\n" +
                "  <aspNetCore processPath=\".\\TOP4HoneyChains.Web.exe\" hostingModel=\"inprocess\">\n    <environmentVariables>\n" +
                string.Join("\n", lines) + "\n    </environmentVariables>\n  </aspNetCore>\n</system.webServer></location></configuration>\n");
        }

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
            try { Directory.Delete(SiteFolder, true); } catch (IOException) { }
        }
    }

    [CollectionDefinition("TestSet")]
    public class TestSetCollection : ICollectionFixture<TestSetApp> { }

    [Collection("TestSet")]
    [Trait("Category", "E2E")]
    public class TestSetTests
    {
        private readonly TestSetApp _app;
        public TestSetTests(TestSetApp app) => _app = app;

        private async Task<(int Exit, string Output)> CommandAsync(params string[] args)
        {
            var output = new StringWriter();
            var exit = await TestSetCommand.RunAsync(args, output, contentRoot: _app.SiteFolder);
            return (exit, output.ToString());
        }

        private async Task<int> CountAsync(string sql)
        {
            await using var conn = new SqlConnection(_app.ConnectionString);
            await conn.OpenAsync();
            return Convert.ToInt32(await new SqlCommand(sql, conn).ExecuteScalarAsync());
        }

        private static string NewFolder() => Path.Combine(Path.GetTempPath(), "t4h-testset-out-" + Guid.NewGuid().ToString("N"));

        private static List<string[]> ReadCsv(string file) =>
            File.ReadAllLines(file).Skip(1).Select(l => l.Split(',')).ToList();

        [Fact]
        public async Task Create_refuses_without_a_switch_inside_the_site_folder_and_with_a_password()
        {
            var before = await CountAsync("SELECT COUNT(*) FROM dbo.AspNetUsers");
            try
            {
                _app.WriteWebConfig(consumerAccounts: false);
                var (exit, output) = await CommandAsync(TestSetCommand.CreateName, "--out", NewFolder());
                Assert.Equal(3, exit);
                Assert.Contains("Off: Features__ConsumerAccounts", output);
                Assert.DoesNotContain("Features__OrgRoleModel", output);
            }
            finally
            {
                _app.WriteWebConfig(consumerAccounts: true);
            }
            var inside = await CommandAsync(TestSetCommand.CreateName, "--out", Path.Combine(_app.SiteFolder, "logins"));
            Assert.Equal(2, inside.Exit);
            Assert.Contains("never go into the site folder", inside.Output);
            var password = await CommandAsync(TestSetCommand.CreateName, "--password", "zz_test_x");
            Assert.Equal(2, password.Exit);
            var sets = await CommandAsync(TestSetCommand.CreateName, "--sets", "99");
            Assert.Equal(2, sets.Exit);
            Assert.Equal(before, await CountAsync("SELECT COUNT(*) FROM dbo.AspNetUsers"));
            Assert.False(Directory.Exists(Path.Combine(_app.SiteFolder, "logins")));
        }

        [Fact]
        public async Task The_test_set_is_made_once_kept_apart_and_removed_without_touching_real_records()
        {
            // a real organization with a real member, before the test set
            var realOrg = await _app.CreateOrgAsync("zz_test_real_shop", null, (Capabilities.Retail, null));
            var realUser = await _app.CreateUserAsync("zz_test_real_member");
            await _app.AddMemberAsync(realOrg, realUser, OrgRoles.Staff);
            var auditBefore = await CountAsync("SELECT COUNT(*) FROM dbo.AuditEntries");
            const string RealSms = "SELECT COUNT(*) FROM dbo.SmsMessages WHERE Provider <> 'TestInbox'";
            var realSms = await CountAsync(RealSms);

            // ---- create
            var folder = NewFolder();
            var (exit, output) = await CommandAsync(TestSetCommand.CreateName, "--out", folder, "--sets", "2");
            Assert.True(exit == 0, output);
            Assert.Contains("The test set was created", output);
            Assert.Contains(folder, output);

            // the files: outside the site folder, one new password per account, never printed or logged
            var logins = ReadCsv(Path.Combine(folder, TestSetFiles.LoginsCsv));
            Assert.Equal(19 + 1 + 2, logins.Count);
            foreach (var user in new[] { "selin.aydin", "murat.kaya", "hasan.demir", "fatma.aksoy", "can.yildiz", "gizem.polat", "tolga.sezer", "seda.gunes", "aylin.yurt" })
                Assert.Contains(logins, l => l[0] == user);
            var passwords = logins.Select(l => l[1]).ToList();
            Assert.Equal(passwords.Count, passwords.Distinct().Count());
            Assert.All(passwords, p => Assert.Matches("^[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}$", p));
            Assert.True(File.Exists(Path.Combine(folder, TestSetFiles.LoginsTxt)));
            var sets = ReadCsv(Path.Combine(folder, TestSetFiles.SetsCsv));
            Assert.Equal(2, sets.Count);
            Assert.All(sets, s => Assert.StartsWith("TEST-TK-", s[^1]));
            var siteText = string.Join("\n", Directory.GetFiles(_app.SiteFolder, "*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)).Select(File.ReadAllText));
            foreach (var p in passwords)
            {
                Assert.DoesNotContain(p, output);
                Assert.DoesNotContain(p, siteText);
                Assert.Equal(0, await CountAsync($"SELECT COUNT(*) FROM dbo.SmsMessages WHERE BodyStored LIKE N'%{p}%'"));
                Assert.Equal(0, await CountAsync($"SELECT COUNT(*) FROM dbo.AuditEntries WHERE NewValues LIKE N'%{p}%' OR OldValues LIKE N'%{p}%'"));
            }

            // the starting state of the live-like cast, marked by the flag, with test codes only
            Assert.Equal(5, await CountAsync("SELECT COUNT(*) FROM dbo.Organizations WHERE IsTestData = 1 AND DisplayName LIKE N'TEST %' AND Status = N'Verified'"));
            Assert.Equal(22, await CountAsync("SELECT COUNT(*) FROM dbo.TestSetAccounts"));
            Assert.Equal(1, await CountAsync("SELECT COUNT(*) FROM dbo.FinalHoneyProductions f JOIN dbo.Organizations o ON o.OrgId = f.OwnerOrgId WHERE o.IsTestData = 1 AND f.FinalHoneyProductionIsFinalized = 1"));
            Assert.Equal(1, await CountAsync("SELECT COUNT(*) FROM dbo.HolderStorageRecords WHERE ApprovedAt IS NOT NULL"));
            Assert.Equal(2, await CountAsync("SELECT COUNT(*) FROM dbo.ConsumerSavedBatches"));
            Assert.Equal(1, await CountAsync("SELECT COUNT(*) FROM dbo.RawHoneyProductionPeriods WHERE IsTestData = 1"));
            Assert.Equal(1, await CountAsync("SELECT COUNT(*) FROM dbo.Beekeepers WHERE BeekeeperBusinessNumber = N'TR-BEE-TEST-000001'"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.Beekeepers WHERE BeekeeperBusinessNumber NOT LIKE N'TR-BEE-TEST-%'"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.SealCodes WHERE Code NOT LIKE 'TEST-%'"));
            Assert.Equal(2, await CountAsync("SELECT COUNT(*) FROM dbo.OrgContainers WHERE Code LIKE 'TEST-TK-%'"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.LabReports WHERE ReportNumber NOT LIKE 'TEST-%'"));
            // every SMS of the run stayed in the test inbox (the configured provider was never called)
            Assert.True(await CountAsync("SELECT COUNT(*) FROM dbo.SmsMessages WHERE Provider = 'TestInbox'") > 20);
            Assert.Equal(realSms, await CountAsync(RealSms));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.SmsMessages WHERE Provider = 'TestInbox' AND Status = 'Failed'"));

            // running it again changes nothing
            var again = await CommandAsync(TestSetCommand.CreateName, "--out", NewFolder());
            Assert.Equal(4, again.Exit);
            Assert.Contains("already exists", again.Output);
            Assert.Equal(22, await CountAsync("SELECT COUNT(*) FROM dbo.TestSetAccounts"));

            // ---- the TEST banner on the Honey Story of the test batch
            var qr = await _app.WithContextAsync(db => db.FinalHoneyProductions.Select(f => f.FinalHoneyProductionQR).SingleAsync());
            var anonymous = UserClient.Anonymous(_app);
            Assert.Contains("id=\"test-set-banner\"", await anonymous.HtmlAsync($"/honeystory/{qr}"));
            Assert.Contains("id=\"test-set-banner\"", await anonymous.HtmlAsync($"/honeystory/details/{qr}"));
            Assert.DoesNotContain("id=\"test-set-banner\"", await anonymous.HtmlAsync("/"));

            // ---- the test inbox: test-set accounts and platform administrators only, test numbers only
            string Password(string user) => logins.Single(l => l[0] == user)[1];
            var (officer, signedIn) = await UserClient.SignInAsync(_app, "murat.kaya", Password("murat.kaya"));
            Assert.Equal(HttpStatusCode.Redirect, signedIn.StatusCode);
            Assert.DoesNotContain("/Account/Login", signedIn.Headers.Location?.ToString() ?? "");
            var inbox = WebUtility.HtmlDecode(await officer.HtmlAsync("/TestInbox"));
            Assert.Contains("+90 512 000 00 31 · Ali Yazıcı", inbox);
            Assert.Matches(@"kodu görevliye söyleyin: \d{6}", inbox);
            Assert.Contains("id=\"test-set-banner\"", await officer.HtmlAsync("/MyWork"));
            var (consumer, _) = await UserClient.SignInAsync(_app, "aylin.yurt", Password("aylin.yurt"));
            Assert.Equal(200, await consumer.StatusAsync("/TestInbox"));
            var admin = await UserClient.LoginAsync(_app, TestSetApp.Admin);
            Assert.Equal(200, await admin.StatusAsync("/TestInbox"));
            var real = await UserClient.LoginAsync(_app, "zz_test_real_member");
            Assert.Equal(404, await real.StatusAsync("/TestInbox"));
            Assert.Equal(HttpStatusCode.Redirect, (await anonymous.GetAsync("/TestInbox")).StatusCode);
            // an SMS to a real number (the hosted app's fake provider) is not in the inbox
            using (var scope = _app.Services.CreateScope())
            {
                var messages = scope.ServiceProvider.GetRequiredService<IMessageService>();
                Assert.True((await messages.SendTextAsync(SmsPurpose.DeliveryCorrected, null, null, "+905550000177", "en", new Dictionary<string, string> { ["org"] = "zz", ["date"] = "x", ["containers"] = "x", ["kg"] = "1", ["honey"] = "x" })).Ok);
            }
            Assert.DoesNotContain("*0177", await admin.HtmlAsync("/TestInbox"));
            Assert.Equal(realSms + 1, await CountAsync(RealSms));

            // a consumer who signs up from the test Honey Story belongs to the test set; a test organization's seal codes start with
            // TEST-, and a real organization cannot use TEST-
            var signUp = UserClient.Anonymous(_app);
            var returnUrl = $"/Consumer/Save?qr={qr}";
            var form = await signUp.HtmlAsync("/Consumer/SignUp?returnUrl=" + Uri.EscapeDataString(returnUrl));
            var formTime = System.Text.RegularExpressions.Regex.Match(form, "name=\"formTime\"[^>]*value=\"([^\"]+)\"|value=\"([^\"]+)\"[^>]*name=\"formTime\"");
            await Task.Delay(TimeSpan.FromSeconds(3.5));
            var signedUp = await signUp.PostAsync("/Consumer/SignUp?returnUrl=" + Uri.EscapeDataString(returnUrl), new()
            {
                ["UserName"] = "zz_test_live_consumer", ["Password"] = "Kovan-Bal-Test-77", ["ConfirmPassword"] = "Kovan-Bal-Test-77", ["Language"] = "en",
                ["IsAdult"] = "true", ["AcceptNotice"] = "true", ["website"] = "",
                ["formTime"] = WebUtility.HtmlDecode(formTime.Groups[1].Success ? formTime.Groups[1].Value : formTime.Groups[2].Value)
            }, "/Consumer/SignUp");
            Assert.Equal(HttpStatusCode.Redirect, signedUp.StatusCode);
            Assert.Equal(1, await CountAsync("SELECT COUNT(*) FROM dbo.TestSetAccounts a JOIN dbo.AspNetUsers u ON u.Id = a.UserId WHERE u.UserName = N'zz_test_live_consumer' AND a.Kind = 'Consumer'"));
            using (var scope = _app.Services.CreateScope())
            {
                var pool = scope.ServiceProvider.GetRequiredService<SealPoolService>();
                var association = await _app.WithContextAsync(db => db.Organizations.Where(o => o.IsTestData && o.DisplayName == "TEST Şile Arıcılar Birliği").Select(o => o.OrgId).SingleAsync());
                var (testRange, _) = await pool.AddPlasticRangeAsync(association, "ZZ-", "1", "2", realUser, null);
                Assert.Equal("TEST-ZZ-1", testRange!.FirstCode);
                Assert.StartsWith("TEST-TS-", (await pool.IssueLabelsAsync(association, 1, realUser, null)).FirstCode);
                var (realRange, refused) = await pool.AddPlasticRangeAsync(realOrg, "TEST-ZZ-", "1", "2", realUser, null);
                Assert.Null(realRange);
                Assert.Equal("TEST- is kept for the seals of the test set.", refused);
                Assert.Matches("^TS-0000", (await pool.IssueLabelsAsync(realOrg, 1, realUser, null)).FirstCode);
            }
            // (the real organization's seal labels stay after the removal)

            // ---- remove-test-set: the dry run only counts
            var dry = await CommandAsync(TestSetCommand.RemoveName);
            Assert.True(dry.Exit == 0, dry.Output);
            Assert.Contains("counting only", dry.Output);
            Assert.Contains("Organizations", dry.Output);
            Assert.Contains("audit entries of the test set stay", dry.Output);
            Assert.Equal(5, await CountAsync("SELECT COUNT(*) FROM dbo.Organizations WHERE IsTestData = 1"));

            // a real record that refers to the test set stops the removal and is listed; nothing is removed
            var batchId = await _app.WithContextAsync(db => db.FinalHoneyProductions.Select(f => f.FinalHoneyProductionId).SingleAsync());
            var realRecord = await _app.WithContextAsync(async db =>
            {
                var record = new HolderStorageRecord { FinalHoneyProductionId = batchId, HolderOrgId = realOrg, CcpId = await db.CriticalControlPoints.Where(c => c.Code == "CCP 10").Select(c => c.Id).FirstAsync(),
                    Status = AssignmentStatusIds.Assigned, RecordedByUserId = realUser, RecordedAt = DateTime.UtcNow };
                db.HolderStorageRecords.Add(record);
                await db.SaveChangesAsync();
                return record.RecordId;
            });
            var blocked = await CommandAsync(TestSetCommand.RemoveName, "--commit");
            Assert.Equal(7, blocked.Exit);
            Assert.Contains($"HolderStorageRecords {realRecord}: FinalHoneyProductionId -> FinalHoneyProductions {batchId}", blocked.Output);
            Assert.Equal(23, await CountAsync("SELECT COUNT(*) FROM dbo.TestSetAccounts"));
            await _app.WithContextAsync(db => db.HolderStorageRecords.Where(r => r.RecordId == realRecord).ExecuteDeleteAsync());

            // ---- remove --commit: everything of the test set goes, nothing else; the audit entries stay
            var reports = Directory.GetFiles(Path.Combine(_app.SiteFolder, "App_Data", "lab-reports"));
            Assert.NotEmpty(reports);
            var removed = await CommandAsync(TestSetCommand.RemoveName, "--commit");
            Assert.True(removed.Exit == 0, removed.Output);
            Assert.Contains("The test set was removed", removed.Output);
            foreach (var table in new[] { "TestSetAccounts", "Beekeepers", "Apiaries", "RawHoneyProductions", "LotIntakes", "Containers", "Samples", "LabReports",
                         "FinalHoneyProductions", "Shipments", "HolderStorageRecords", "OrgContainers", "ConsumerAccounts",
                         "ConsumerSavedBatches", "OneTimeCodes", "BeekeeperRegistrations", "ContainerSealEvents", "UserCredentialIssues", "NoticeAcceptances" })
                Assert.True(await CountAsync($"SELECT COUNT(*) FROM dbo.[{table}]") == 0, table);
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.Organizations WHERE IsTestData = 1"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.RawHoneyProductionPeriods"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.SmsMessages WHERE Provider = 'TestInbox'"));
            Assert.Empty(Directory.GetFiles(Path.Combine(_app.SiteFolder, "App_Data", "lab-reports")));
            // the real records stay
            Assert.Equal(realSms + 1, await CountAsync(RealSms));
            Assert.Equal(1, await CountAsync("SELECT COUNT(*) FROM dbo.SealCodes"));
            Assert.Equal(1, await CountAsync($"SELECT COUNT(*) FROM dbo.SealBatches WHERE OrgId = {realOrg}"));
            Assert.Equal(2, await CountAsync("SELECT COUNT(*) FROM dbo.AspNetUsers"));
            Assert.Equal(1, await CountAsync($"SELECT COUNT(*) FROM dbo.Organizations WHERE OrgId = {realOrg}"));
            Assert.Equal(2, await CountAsync("SELECT COUNT(*) FROM dbo.Memberships"));
            Assert.True(await CountAsync("SELECT COUNT(*) FROM dbo.Organizations WHERE IsPlatform = 1") == 1);
            // the audit log: every entry stays, one more records the removal, the chain holds
            Assert.True(await CountAsync("SELECT COUNT(*) FROM dbo.AuditEntries") > auditBefore + 100);
            Assert.Equal(1, await CountAsync($"SELECT COUNT(*) FROM dbo.AuditEntries WHERE Action = N'{TestSetRemover.AuditAction}'"));
            Assert.True((await _app.WithContextAsync(db => AuditChain.VerifyAsync(db))).Ok);

            var none = await CommandAsync(TestSetCommand.RemoveName, "--commit");
            Assert.Equal(0, none.Exit);
            Assert.Contains("There is no test set", none.Output);
            try { Directory.Delete(folder, true); } catch (IOException) { }
        }

        [Fact]
        public async Task An_sms_to_a_test_number_never_reaches_a_real_provider()
        {
            var options = Options.Create(new SmsOptions
            {
                Provider = SmsProviders.Netgsm, CodeKey = _app.CodeKey, DailyLimit = 100000,
                Netgsm = new NetgsmOptions { BaseUrl = "http://127.0.0.1:9", UserCode = "zz_test", Password = "zz_test_not_a_secret" }
            });
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<SmsOptions>>(options);
            services.AddLogging();
            services.AddHttpClient<NetgsmSmsGateway>(c => c.Timeout = TimeSpan.FromSeconds(5));
            services.AddSingleton<FakeSmsStore>();
            services.AddSingleton<FakeSmsGateway>();
            services.AddSingleton<LoggingSmsGateway>();
            await using var provider = services.BuildServiceProvider();
            await _app.WithContextAsync(async db =>
            {
                var sms = new MessageService(db, new SmsGatewayFactory(provider, options), options, TimeProvider.System, NullLogger<MessageService>.Instance);
                var values = new Dictionary<string, string> { ["code"] = "x" };
                // the test number: stored with its full text in the test inbox
                for (var i = 0; i < 3; i++)
                {
                    var sent = await sms.SendCodeAsync(new CodeRequest(SmsPurpose.PhoneVerify, "zz_test", "zz_test_subject_" + i, TestPhones.Number(99, 1), "en", "x", values));
                    Assert.True(sent.Ok, sent.Error);
                    var row = await db.SmsMessages.SingleAsync(m => m.SmsId == sent.SmsId);
                    Assert.Equal(TestPhones.Provider, row.Provider);
                    Assert.Matches(@"\b\d{6}\b", row.BodyStored);
                }
                // a real number goes to the configured provider (here unreachable: the send fails)
                var real = await sms.SendCodeAsync(new CodeRequest(SmsPurpose.PhoneVerify, "zz_test", "zz_test_real", "+905550000188", "en", "x", values));
                Assert.False(real.Ok);
                Assert.Equal(SmsProviders.Netgsm, (await db.SmsMessages.SingleAsync(m => m.SmsId == real.SmsId)).Provider);
                return 0;
            });
        }
    }
}
