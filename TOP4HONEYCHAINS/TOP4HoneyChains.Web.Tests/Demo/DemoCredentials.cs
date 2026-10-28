using System.Text;
using Microsoft.Data.SqlClient;

namespace TOP4HoneyChains.Web.Tests.Demo
{
    // D-86: the credential sheet of every country (and one with all countries for the coordinator), written OUTSIDE the repository
    // into the partner test pack: <pack>\<country>\credentials.txt. Passwords exist only there and in the demo database (hashed).
    public static class DemoCredentials
    {
        public static readonly Dictionary<string, string> Folders = new()
        {
            ["AR"] = "AR-Argentina", ["PL"] = "PL-Poland", ["LV"] = "LV-Latvia", ["TR"] = "TR-Turkiye", ["EN"] = "EN-Coordinator"
        };

        private static readonly Dictionary<string, string> CountryNames = new() { ["AR"] = "Argentina", ["PL"] = "Poland", ["LV"] = "Latvia", ["TR"] = "Türkiye" };

        private record Texts(string Title, string Private, string Sms, string Portal, string Role, string Org, string User, string Password, string Admin, string Beekeeper, string AllOrgs);

        private static readonly Dictionary<string, Texts> Language = new()
        {
            ["en"] = new("Demo test accounts", "KEEP THIS SHEET PRIVATE. Demo accounts with test data only. Give each password only to the tester who plays the role.",
                "The demo sends no real SMS: codes and sign-in details appear in \"Demo SMS inbox\" (link at the bottom of every page).",
                "The beekeeper signs in to the portal with the business number as user name.",
                "Role", "Organization", "User name", "Password", "Platform administrator", "Beekeeper (portal)", "all organizations of the country"),
            ["es"] = new("Cuentas de prueba de la demostración", "MANTENGA ESTA HOJA EN PRIVADO. Cuentas de demostración solo con datos de prueba. Entregue cada contraseña solo a quien prueba ese rol.",
                "La demostración no envía SMS reales: los códigos y los datos de acceso aparecen en «Bandeja de SMS de demostración» (enlace al pie de cada página).",
                "El apicultor entra al portal con su número de explotación como nombre de usuario.",
                "Rol", "Organización", "Usuario", "Contraseña", "Administrador de la plataforma", "Apicultor (portal)", "todas las organizaciones del país"),
            ["pl"] = new("Konta testowe wersji demonstracyjnej", "ZACHOWAJ TEN ARKUSZ W TAJEMNICY. Konta demonstracyjne tylko z danymi testowymi. Każde hasło przekaż tylko osobie testującej daną rolę.",
                "Wersja demonstracyjna nie wysyła prawdziwych SMS-ów: kody i dane logowania pojawiają się w „Skrzynka SMS (demo)” (link na dole każdej strony).",
                "Pszczelarz loguje się do portalu numerem gospodarstwa jako nazwą użytkownika.",
                "Rola", "Organizacja", "Nazwa użytkownika", "Hasło", "Administrator platformy", "Pszczelarz (portal)", "wszystkie organizacje kraju"),
            ["lv"] = new("Demonstrācijas testa konti", "GLABĀJIET ŠO LAPU SLEPENĪBĀ. Demonstrācijas konti tikai ar testa datiem. Katru paroli dodiet tikai tam, kurš testē attiecīgo lomu.",
                "Demonstrācija nesūta īstas īsziņas: kodi un pieteikšanās dati parādās „Demo īsziņu pastkastīte” (saite katras lapas apakšā).",
                "Biškopis piesakās portālā ar saimniecības numuru kā lietotājvārdu.",
                "Loma", "Organizācija", "Lietotājvārds", "Parole", "Platformas administrators", "Biškopis (portāls)", "visas valsts organizācijas"),
            ["tr"] = new("Demo test hesapları", "BU SAYFAYI GİZLİ TUTUN. Yalnızca test verisi içeren demo hesapları. Her parolayı yalnızca o rolü deneyen kişiye verin.",
                "Demo gerçek SMS göndermez: kodlar ve giriş bilgileri \"Demo SMS kutusu\"nda görünür (her sayfanın altındaki bağlantı).",
                "Arıcı portala işletme numarasını kullanıcı adı olarak girer.",
                "Rol", "Kurum", "Kullanıcı adı", "Parola", "Platform yöneticisi", "Arıcı (portal)", "ülkenin tüm kurumları")
        };

        private static readonly Dictionary<string, string> EnglishRoles = new()
        {
            ["OrgAdmin"] = "Organization admin", ["Staff"] = "Staff", ["FieldInspector"] = "Field inspector", ["SampleAcceptanceOfficer"] = "Sample acceptance officer",
            ["QualityApprover"] = "Quality Assurance Officer", ["Analyst"] = "Analyst", ["LabManager"] = "Lab manager",
            ["RegistrationOfficer"] = "Beekeeper registration officer", ["ComplianceOfficer"] = "Compliance Officer"
        };

        private static async Task<Dictionary<(string Role, string Culture), string>> RoleNamesAsync(string connectionString)
        {
            var names = new Dictionary<(string, string), string>();
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("SELECT EntityKey, Culture, Value FROM dbo.ReferenceTranslations WHERE Entity = 'OrgRole' AND Field = 'Name'", conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                names[(r.GetString(0), r.GetString(1))] = r.GetString(2);
            return names;
        }

        public static async Task WriteAsync(string pack, DemoSeed.Result seed, string date, string connectionString)
        {
            var roles = await RoleNamesAsync(connectionString);
            string RoleName(DemoSeed.Credential c, string lang, Texts t) =>
                c.Role == "PlatformAdmin" ? t.Admin
                : c.AccountKey == "beekeeper" ? t.Beekeeper
                : lang == "en" ? EnglishRoles[c.Role] : roles.GetValueOrDefault((c.Role, lang), EnglishRoles[c.Role]);

            void Sheet(StringBuilder sb, string code, string lang)
            {
                var t = Language[lang];
                var en = Language["en"];
                var rows = seed.Credentials.Where(c => c.Country == code).ToList();
                sb.AppendLine($"=== {CountryNames[code]} ({code}) ===");
                var table = rows.Select(c => (Role: RoleName(c, lang, t), Org: c.OrgName == "*" ? t.AllOrgs : c.OrgName, c.UserName, c.Password)).ToList();
                int w1 = Math.Max(t.Role.Length, table.Max(x => x.Role.Length)), w2 = Math.Max(t.Org.Length, table.Max(x => x.Org.Length)),
                    w3 = Math.Max(t.User.Length, table.Max(x => x.UserName.Length));
                sb.AppendLine($"{t.Role.PadRight(w1)}  {t.Org.PadRight(w2)}  {t.User.PadRight(w3)}  {t.Password}");
                sb.AppendLine(new string('-', w1 + w2 + w3 + 25));
                foreach (var x in table)
                    sb.AppendLine($"{x.Role.PadRight(w1)}  {x.Org.PadRight(w2)}  {x.UserName.PadRight(w3)}  {x.Password}");
                sb.AppendLine();
            }

            string Header(string lang, string scope)
            {
                var t = Language[lang];
                var sb = new StringBuilder();
                sb.AppendLine($"TOP4HoneyChains – {t.Title} – {scope} – {date}");
                if (lang != "en")
                    sb.AppendLine($"TOP4HoneyChains – {Language["en"].Title}");
                sb.AppendLine();
                sb.AppendLine(t.Private);
                if (lang != "en")
                    sb.AppendLine(Language["en"].Private);
                sb.AppendLine();
                sb.AppendLine(t.Sms);
                sb.AppendLine(t.Portal);
                sb.AppendLine();
                return sb.ToString();
            }

            var utf8 = new UTF8Encoding(true);
            foreach (var (code, lang) in new[] { ("AR", "es"), ("PL", "pl"), ("LV", "lv"), ("TR", "tr") })
            {
                var sb = new StringBuilder(Header(lang, CountryNames[code]));
                Sheet(sb, code, lang);
                var dir = Path.Combine(pack, Folders[code]);
                Directory.CreateDirectory(dir);
                await File.WriteAllTextAsync(Path.Combine(dir, "credentials.txt"), sb.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n"), utf8);
            }
            var all = new StringBuilder(Header("en", "all partner countries (coordinator)"));
            foreach (var code in new[] { "AR", "PL", "LV", "TR" })
                Sheet(all, code, "en");
            var coordinator = Path.Combine(pack, Folders["EN"]);
            Directory.CreateDirectory(coordinator);
            await File.WriteAllTextAsync(Path.Combine(coordinator, "credentials.txt"), all.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n"), utf8);
        }
    }
}
