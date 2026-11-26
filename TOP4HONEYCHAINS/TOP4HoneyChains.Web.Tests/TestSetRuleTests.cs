using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.Messaging;
using TOP4HoneyChains.Web.Services.OrgModel;
using TOP4HoneyChains.Web.Services.OrgModel.TestSet;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-112: the rules of the live site's test set that need no database
    public class TestSetRuleTests
    {
        [Fact]
        public void Test_numbers_are_valid_pager_numbers_of_one_block()
        {
            var number = TestPhones.Number(0, 31);
            Assert.Equal("+905120000031", number);
            Assert.True(TestPhones.IsTest(number));
            // every form takes them (libphonenumber: a valid Turkish number)
            Assert.Equal(number, PhoneNumberPolicy.Normalize("+90 512 000 00 31", out _));
            Assert.Equal(number, PhoneNumberPolicy.Normalize("0512 000 00 31", out _));
            Assert.All(new[] { "+905550000101", "+90512000003", "+9051200000311", "+905121000031", null, "" }, n => Assert.False(TestPhones.IsTest(n)));
            Assert.Equal(10000, TestPhones.All().Distinct().Count());
            Assert.All(TestPhones.All().Take(50), n => Assert.True(TestPhones.IsTest(n)));
        }

        [Fact]
        public void Registry_numbers_keep_the_official_format()
        {
            Assert.Equal("TR349999000031", TestSetCreator.RegistryNumber(0, 31));
            Assert.True(RegistryNumber.IsWellFormed(TestSetCreator.RegistryNumber(12, 1)));
        }

        [Fact]
        public void Every_password_is_new_and_strong()
        {
            var passwords = Enumerable.Range(0, 200).Select(_ => TestSetCreator.NewPassword()).ToList();
            Assert.Equal(200, passwords.Distinct().Count());
            Assert.All(passwords, p => Assert.Matches("^[A-HJ-NP-Za-km-np-z2-9]{4}(-[A-HJ-NP-Za-km-np-z2-9]{4}){3}$", p));
        }

        [Fact]
        public void The_cast_has_the_local_user_names_and_enough_sets()
        {
            var names = TestSetCreator.UserNames(TestSetCreator.MaxSets);
            foreach (var user in new[] { "selin.aydin", "murat.kaya", "hasan.demir", "zeynep.arslan", "mehmet.ozturk", "can.yildiz", "deniz.koc", "gizem.polat", "okan.kurt", "seda.gunes" })
                Assert.Contains(user, names);
            Assert.Equal(19 + TestSetCreator.MaxSets, names.Count);
            Assert.Equal(19 + TestSetCommand.DefaultSets, TestSetCreator.UserNames(TestSetCommand.DefaultSets).Count);
            Assert.Equal("TEST Şile Arıcılar Birliği", TestSetCreator.OrgName(TestSetCreator.Cast().GetProperty("orgs")[2]));
        }

        [Fact]
        public void The_command_line_is_read_like_create_first_admin()
        {
            var o = TestSetCommand.Parse(new[] { TestSetCommand.CreateName, "--out", @"C:\x", "--sets", "3", "--environment", "Production" });
            Assert.Null(o.Error);
            Assert.Equal(@"C:\x", o.Out);
            Assert.Equal(3, o.Sets);
            Assert.Equal(new[] { "--environment", "Production" }, o.HostArgs);
            Assert.Equal(TestSetCommand.DefaultSets, TestSetCommand.Parse(new[] { TestSetCommand.CreateName }).Sets);
            Assert.NotNull(TestSetCommand.Parse(new[] { TestSetCommand.CreateName, "--sets", "0" }).Error);
            Assert.NotNull(TestSetCommand.Parse(new[] { TestSetCommand.CreateName, "--sets", "13" }).Error);
            Assert.NotNull(TestSetCommand.Parse(new[] { TestSetCommand.CreateName, "--password", "x" }).Error);
            Assert.True(TestSetCommand.Parse(new[] { TestSetCommand.RemoveName, "--commit" }).Commit);
            Assert.False(TestSetCommand.Parse(new[] { TestSetCommand.RemoveName }).Commit);
            Assert.True(TestSetCommand.IsCommand("create-test-set") && TestSetCommand.IsCommand("remove-test-set") && !TestSetCommand.IsCommand("create-first-admin"));
        }

        [Fact]
        public void The_logins_never_go_into_the_site_folder()
        {
            var site = Path.Combine(Path.GetTempPath(), "t4h-site-" + Guid.NewGuid().ToString("N"));
            Assert.Null(TestSetCommand.OutFolder(Path.Combine(site, "App_Data", "x"), site).Folder);
            Assert.Null(TestSetCommand.OutFolder(site, site).Folder);
            var (folder, error) = TestSetCommand.OutFolder(site + "-logins", site);
            Assert.Null(error);
            Assert.Equal(site + "-logins", folder);
            // the default: a new folder in the Documents folder of the person who runs it
            var (documents, _) = TestSetCommand.OutFolder(null, site);
            Assert.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), documents);
        }

        [Fact]
        public void The_site_settings_come_from_web_config()
        {
            var site = Path.Combine(Path.GetTempPath(), "t4h-site-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(site);
            try
            {
                File.WriteAllText(Path.Combine(site, "web.config"),
                    "<configuration><system.webServer><aspNetCore processPath=\"x\"><environmentVariables>" +
                    "<environmentVariable name=\"Features__OrgRoleModel\" value=\"true\" /><environmentVariable name=\"ASPNETCORE_ENVIRONMENT\" value=\"Production\" />" +
                    "</environmentVariables></aspNetCore></system.webServer></configuration>");
                var settings = TestSetCommand.WebConfigSettings(site);
                Assert.Equal("true", settings["Features:OrgRoleModel"]);
                Assert.Equal("Production", settings["ASPNETCORE_ENVIRONMENT"]);
                Assert.Empty(TestSetCommand.WebConfigSettings(Path.Combine(site, "none")));
            }
            finally
            {
                Directory.Delete(site, true);
            }
        }

        [Fact]
        public void Children_are_deleted_before_the_records_they_refer_to()
        {
            var order = TestSetRemover.ChildrenFirst(new[] { "Organizations", "Memberships", "RawHoneyProductions", "Containers", "LotIntakes" },
                new[] { ("Memberships", "Organizations"), ("RawHoneyProductions", "Organizations"), ("LotIntakes", "RawHoneyProductions"), ("Containers", "LotIntakes"), ("Containers", "RawHoneyProductions") });
            Assert.True(order.IndexOf("Containers") < order.IndexOf("LotIntakes"));
            Assert.True(order.IndexOf("LotIntakes") < order.IndexOf("RawHoneyProductions"));
            Assert.True(order.IndexOf("RawHoneyProductions") < order.IndexOf("Organizations"));
            Assert.True(order.IndexOf("Memberships") < order.IndexOf("Organizations"));
            Assert.Throws<InvalidOperationException>(() => TestSetRemover.ChildrenFirst(new[] { "A", "B" }, new[] { ("A", "B"), ("B", "A") }));
        }
    }
}
