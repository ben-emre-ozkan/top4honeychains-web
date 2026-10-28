using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Tests.Demo
{
    // D-86: the demo data of the four partner countries. Every name is marked [DEMO]; phone numbers are test numbers that the demo
    // never sends to (fake SMS provider); registry numbers are demo numbers (AR/PL/LV-DEMO-nnnnnn) or Turkish test numbers.
    public record DemoRegion(string Code, string Name, string[] Districts);

    public record DemoOrg(string Key, string Name, string[] Capabilities, string? Accreditation = null, string? City = null);

    public record DemoAccount(string Key, string OrgKey, string Role);

    public record DemoBeekeeper(string Key, string FirstName, string LastName, DateOnly BirthDate, string Phone, string RegistryNumber,
        decimal Latitude, decimal Longitude, string District, string Locality, int Hives, bool PublicName, bool Photo, bool Portal);

    public record DemoCountry(
        string Code, string Language, string Currency, string CountryName, DemoRegion[] Regions, DemoOrg[] Orgs,
        DemoBeekeeper[] Beekeepers, string StaffPhonePrefix, string HoneyType, string BatchTitle, string Locality, decimal Price,
        string Botanical)
    {
        public string Prefix => Code.ToLowerInvariant();
        public DemoRegion Region => Regions[0];
        public DemoOrg Org(string key) => Orgs.Single(o => o.Key == key);
    }

    public static class DemoCountries
    {
        // organization keys
        public const string Authority = "authority", Association = "association", Plant = "plant", Lab = "lab", Shop = "shop";

        // the accounts of every country: user name = <country>.<key> (e.g. ar.reception); the Compliance Officer has a membership in
        // every organization of the country; the platform administrator has the Administrator role
        public static readonly DemoAccount[] Accounts =
        {
            new("admin", "", "PlatformAdmin"),
            new("registration", Authority, OrgRoles.RegistrationOfficer),
            new("association", Association, OrgRoles.OrgAdmin),
            new("inspector", Association, OrgRoles.FieldInspector),
            new("reception", Plant, OrgRoles.SampleAcceptanceOfficer),
            new("staff", Plant, OrgRoles.Staff),
            new("quality", Plant, OrgRoles.QualityApprover),
            new("plantadmin", Plant, OrgRoles.OrgAdmin),
            new("analyst", Lab, OrgRoles.Analyst),
            new("labmanager", Lab, OrgRoles.LabManager),
            new("shop", Shop, OrgRoles.Staff),
            new("shopquality", Shop, OrgRoles.QualityApprover),
            new("compliance", "*", OrgRoles.ComplianceOfficer)
        };

        private static DemoOrg[] Orgs(string authority, string association, string plant, string lab, string labAcc, string shop, string city) => new[]
        {
            new DemoOrg(Authority, authority, new[] { Capabilities.BeekeeperRegistration }, City: city),
            new DemoOrg(Association, association, new[] { Capabilities.Association }, City: city),
            new DemoOrg(Plant, plant, new[] { Capabilities.Processing, Capabilities.Packing, Capabilities.Wholesale }, City: city),
            new DemoOrg(Lab, lab, new[] { Capabilities.Laboratory }, labAcc, city),
            new DemoOrg(Shop, shop, new[] { Capabilities.Retail }, City: city)
        };

        public static readonly DemoCountry[] All =
        {
            new("AR", "es", "ARS", "Argentina",
                new[] { new DemoRegion("A1", "Entre Ríos (AR)", new[] { "Paraná", "Gualeguaychú", "Concordia", "Diamante" }),
                        new DemoRegion("A2", "Buenos Aires (AR)", new[] { "Pergamino", "Tandil", "Azul" }) },
                Orgs("[DEMO] Registro Apícola de Entre Ríos", "[DEMO] Cooperativa Apícola del Litoral", "[DEMO] Mieles del Paraná S.A.",
                     "[DEMO] Laboratorio Apícola Santa Fe", "DEMO-AR-LAB-001", "[DEMO] Almacén Natural Rosario", "Paraná"),
                new[]
                {
                    new DemoBeekeeper("b1", "[DEMO] Lucía", "Fernández", new DateOnly(1979, 3, 14), "+5493434550101", "AR-DEMO-000101", -31.7413m, -60.5115m, "Paraná", "Colonia Avellaneda", 60, true, false, true),
                    new DemoBeekeeper("b2", "[DEMO] Martín", "Gómez", new DateOnly(1985, 9, 2), "+5493434550102", "AR-DEMO-000102", -33.0094m, -58.5172m, "Gualeguaychú", "Pueblo Belgrano", 40, false, false, false),
                    new DemoBeekeeper("b3", "[DEMO] Sofía", "Benítez", new DateOnly(1991, 6, 21), "", "AR-DEMO-000103", -32.0731m, -60.6389m, "Diamante", "Strobel", 25, false, true, false)
                },
                "+54934345502", "Flower Honey", "[DEMO] Miel multifloral del Litoral 2026", "Colonia Avellaneda", 5200m, "Multifloral (Prosopis, Eucalyptus)"),
            new("PL", "pl", "PLN", "Polska",
                new[] { new DemoRegion("P1", "Lubelskie (PL)", new[] { "Lublin", "Puławy", "Kraśnik", "Zamość" }),
                        new DemoRegion("P2", "Podkarpackie (PL)", new[] { "Rzeszów", "Krosno", "Sanok" }) },
                Orgs("[DEMO] Rejestr Pasiek – Lublin", "[DEMO] Lubelski Związek Pszczelarzy", "[DEMO] Pasieka Lubelska Sp. z o.o.",
                     "[DEMO] Laboratorium Miodu Puławy", "DEMO-PL-LAB-001", "[DEMO] Sklep Zielony Koszyk Lublin", "Lublin"),
                new[]
                {
                    new DemoBeekeeper("b1", "[DEMO] Anna", "Kowalska", new DateOnly(1976, 4, 9), "+48512000101", "PL-DEMO-000101", 51.2860m, 22.4210m, "Lublin", "Jastków", 55, true, false, true),
                    new DemoBeekeeper("b2", "[DEMO] Piotr", "Wiśniewski", new DateOnly(1988, 11, 17), "+48512000102", "PL-DEMO-000102", 51.4160m, 21.9690m, "Puławy", "Końskowola", 35, false, false, false),
                    new DemoBeekeeper("b3", "[DEMO] Katarzyna", "Zielińska", new DateOnly(1993, 2, 28), "", "PL-DEMO-000103", 50.9750m, 22.2400m, "Kraśnik", "Urzędów", 20, false, true, false)
                },
                "+485120002", "Flower Honey", "[DEMO] Miód wielokwiatowy lubelski 2026", "Jastków", 38m, "Multifloral (rapeseed, linden)"),
            new("LV", "lv", "EUR", "Latvija",
                new[] { new DemoRegion("L1", "Zemgale (LV)", new[] { "Jelgava", "Bauska", "Dobele" }),
                        new DemoRegion("L2", "Vidzeme (LV)", new[] { "Cēsis", "Valmiera", "Sigulda" }) },
                Orgs("[DEMO] Biškopju reģistrs Zemgale", "[DEMO] Zemgales biškopju biedrība", "[DEMO] Medus Darbnīca SIA",
                     "[DEMO] Jelgavas medus laboratorija", "DEMO-LV-LAB-001", "[DEMO] Veikals Bišu Māja Rīga", "Jelgava"),
                new[]
                {
                    new DemoBeekeeper("b1", "[DEMO] Ilze", "Ozola", new DateOnly(1981, 7, 30), "+37120000101", "LV-DEMO-000101", 56.6110m, 23.7200m, "Jelgava", "Svēte", 45, true, false, true),
                    new DemoBeekeeper("b2", "[DEMO] Jānis", "Kalniņš", new DateOnly(1972, 12, 5), "+37120000102", "LV-DEMO-000102", 56.4080m, 24.1940m, "Bauska", "Mežotne", 30, false, false, false),
                    new DemoBeekeeper("b3", "[DEMO] Līga", "Bērziņa", new DateOnly(1995, 5, 12), "", "LV-DEMO-000103", 56.6230m, 23.2780m, "Dobele", "Auri", 18, false, true, false)
                },
                "+371200002", "Flower Honey", "[DEMO] Zemgales ziedu medus 2026", "Svēte", 9.5m, "Multifloral (clover, linden)"),
            new("TR", "tr", "TRY", "Türkiye",
                Array.Empty<DemoRegion>(),
                Orgs("[DEMO] Muğla İl Tarım Kayıt Birimi", "[DEMO] Muğla Arıcılar Birliği", "[DEMO] Ege Bal İşleme A.Ş.",
                     "[DEMO] Ege Gıda Analiz Laboratuvarı", "DEMO-TR-LAB-001", "[DEMO] Doğal Market İzmir", "Muğla"),
                new[]
                {
                    new DemoBeekeeper("b1", "[DEMO] Ayşe", "Yılmaz", new DateOnly(1977, 8, 19), "+905550009101", "TR480000000101", 37.1570m, 28.3700m, "Menteşe", "Yerkesik", 80, true, false, true),
                    new DemoBeekeeper("b2", "[DEMO] Mehmet", "Kaya", new DateOnly(1969, 1, 23), "+905550009102", "TR480000000102", 37.0340m, 27.4300m, "Bodrum", "Mumcular", 50, false, false, false),
                    new DemoBeekeeper("b3", "[DEMO] Fatma", "Demir", new DateOnly(1990, 10, 3), "", "TR480000000103", 36.8550m, 28.2700m, "Marmaris", "Çetibeli", 30, false, true, false)
                },
                "+9055500092", "Thyme Honey", "[DEMO] Ege kekik balı 2026", "Yerkesik", 650m, "Thyme (Thymbra spicata)")
        };

        // Türkiye uses the real provinces: Muğla (plate 48)
        public const string TurkishProvince = "Muğla";
    }
}
