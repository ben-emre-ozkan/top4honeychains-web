using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-98: a name typed with "Other" is compared ignoring case, Turkish and other accented letters, spaces, punctuation and the
    // word "honey" in the five languages; only sample acceptance officers and registration officers (and the platform
    // administrator) may add a honey type; the official number keeps its label only for Türkiye's numbers.
    public class HoneyTypeCatalogTests
    {
        [Theory]
        [InlineData("Çam Balı", "cam")]
        [InlineData("Pine Honey", "pine")]
        [InlineData("  KESTANE   bal ", "kestane")]
        [InlineData("Kestane-balı", "kestane")]
        [InlineData("IHLAMUR", "ihlamur")]
        [InlineData("ıhlamur", "ihlamur")]
        [InlineData("İhlamur Balı", "ihlamur")]
        [InlineData("Ayçiçeği Balı", "aycicegi")]
        [InlineData("Miel de pino", "pino")]
        [InlineData("Miód gryczany", "gryczany")]
        [InlineData("Priežu medus", "priezu")]
        [InlineData("Ölmez çiçeği", "olmezcicegi")]
        [InlineData("Bal", "bal")]
        [InlineData("", "")]
        public void Names_are_compared_without_case_letters_spaces_or_the_honey_word(string name, string key) =>
            Assert.Equal(key, HoneyTypeCatalog.Normalize(name));

        [Fact]
        public void Turkish_and_English_names_of_one_type_meet_through_the_translation_not_the_letters() =>
            Assert.NotEqual(HoneyTypeCatalog.Normalize("Çam Balı"), HoneyTypeCatalog.Normalize("Pine Honey"));

        private sealed class Current : ICurrentOrg
        {
            private readonly ActiveMembership? _active;
            public Current(string? role, string status = OrgStatus.Verified, bool admin = false)
            {
                IsPlatformAdmin = admin;
                _active = role == null ? null : new ActiveMembership(1, 1, "zz_test org", role, status, false, false, new HashSet<string>());
            }
            public bool Enabled => true;
            public string? UserId => "zz_test_user";
            public string? UserName => "zz_test_user";
            public bool IsPlatformAdmin { get; }
            public Task<ActiveMembership?> GetAsync() => Task.FromResult(_active);
            public Task<List<MembershipOption>> GetMembershipsAsync() => Task.FromResult(new List<MembershipOption>());
        }

        // MayAddAsync and the role check of CheckNewAsync read only the membership
        private static HoneyTypeCatalog Catalog(ICurrentOrg current) => new(null!, current, null!, TimeProvider.System, null!);

        [Theory]
        [InlineData(OrgRoles.SampleAcceptanceOfficer, OrgStatus.Verified, true)]
        [InlineData(OrgRoles.RegistrationOfficer, OrgStatus.Verified, true)]
        [InlineData(OrgRoles.SampleAcceptanceOfficer, OrgStatus.Pending, false)]
        [InlineData(OrgRoles.Staff, OrgStatus.Verified, false)]
        [InlineData(OrgRoles.OrgAdmin, OrgStatus.Verified, false)]
        [InlineData(OrgRoles.ComplianceOfficer, OrgStatus.Verified, false)]
        public async Task Only_the_two_officer_roles_of_a_verified_organization_may_add(string role, string status, bool may) =>
            Assert.Equal(may, await Catalog(new Current(role, status)).MayAddAsync());

        [Fact]
        public async Task The_platform_administrator_may_add() => Assert.True(await Catalog(new Current(null, admin: true)).MayAddAsync());

        [Fact]
        public async Task Staff_is_refused_before_the_name_is_looked_at() =>
            Assert.Equal("Your role cannot add a honey type; choose one from the list.",
                await Catalog(new Current(OrgRoles.Staff)).CheckNewAsync("Zz Lavanta", "Floral"));

        [Theory]
        [InlineData("TR520401200241", "Num.Official")]
        [InlineData(null, "Num.Official")]
        [InlineData("AR-DEMO-000123", "Reg78.RegistryNumber")]
        [InlineData("LV-DEMO-000001", "Reg78.RegistryNumber")]
        public void Only_Turkish_numbers_are_labelled_as_the_holding_identification_number(string? number, string key) =>
            Assert.Equal(key, RegistryNumber.LabelKey(number));

        // the SMS code of a delivery with a new honey type binds the typed name and the class, not only the content
        [Fact]
        public void The_code_of_a_delivery_with_a_new_honey_type_binds_its_name_and_class()
        {
            var content = new IntakeContent("bk", 1, HoneyTypeCatalog.Other, 1, "TR", new DateOnly(2026, 11, 1), "Giresun", "Merkez", 1, "zz", new DateOnly(2028, 1, 1),
                new List<ContainerContent>(), JarCap.SafetyButton);
            IntakeForm Form(string name, string cls) => new() { HoneyTypeId = HoneyTypeCatalog.Other, NewHoneyTypeName = name, NewHoneyTypeClass = cls };
            Assert.Equal(CustodyIntakeService.ConfirmHash(Form("Zz Lavanta", "Floral"), content), CustodyIntakeService.ConfirmHash(Form("Zz Lavanta", "Floral"), content));
            Assert.NotEqual(CustodyIntakeService.ConfirmHash(Form("Zz Lavanta", "Floral"), content), CustodyIntakeService.ConfirmHash(Form("Zz Lavanta", "Pine"), content));
            Assert.NotEqual(CustodyIntakeService.ConfirmHash(Form("Zz Lavanta", "Floral"), content), CustodyIntakeService.ConfirmHash(Form("Zz Kekik", "Floral"), content));
            var known = new IntakeForm { HoneyTypeId = 3 };
            Assert.Equal((content with { HoneyTypeId = 3 }).Hash(), CustodyIntakeService.ConfirmHash(known, content with { HoneyTypeId = 3 }));
        }

        // the registration code covers the beekeeper's honey types only when some are given (older registrations hash as before)
        [Fact]
        public void The_registration_code_covers_the_honey_types()
        {
            var c = new RegistrationService.Checked("TR281234500093", "+905320000493", true, null, null);
            RegistrationForm Form(params int[] ids) => new() { DraftKey = "zz", HoneyTypeIds = ids.ToList() };
            Assert.NotEqual(RegistrationService.ContentHash(Form(), c), RegistrationService.ContentHash(Form(1), c));
            Assert.NotEqual(RegistrationService.ContentHash(Form(1), c), RegistrationService.ContentHash(Form(1, 2), c));
            Assert.Equal(RegistrationService.ContentHash(Form(2, 1), c), RegistrationService.ContentHash(Form(1, 2), c));
            var other = Form(1);
            other.OtherHoneyType = true;
            other.NewHoneyTypeName = "Zz Lavanta";
            other.NewHoneyTypeClass = "Floral";
            Assert.NotEqual(RegistrationService.ContentHash(Form(1), c), RegistrationService.ContentHash(other, c));
        }
    }
}
