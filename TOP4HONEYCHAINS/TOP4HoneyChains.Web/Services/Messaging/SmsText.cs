using System.Text.RegularExpressions;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.Messaging
{
    // D-78 P1 (D78-SPEC 5.5): the SMS texts in five languages. ES, PL and LV are our translations, to be reviewed by the
    // partners. Placeholders: {org} {code} {link} {user} {password} {date} {containers} {kg} {honey} {samples} {name} {version}.
    public static class SmsTemplates
    {
        public static readonly string[] Languages = { "tr", "en", "es", "pl", "lv" };
        public const string DefaultLanguage = "tr";

        private static readonly Dictionary<string, Dictionary<string, string>> Texts = new()
        {
            // M1 registration and consent
            [SmsPurpose.Registration] = new()
            {
                ["tr"] = "TOP4HoneyChains: {org} sizi arıcı olarak kaydediyor. Aydınlatma metni: {link} Adınız herkese açık sayfalarda: {name}. Onaylıyorsanız bu kodu görevliye söyleyin: {code} (5 dk).",
                ["en"] = "TOP4HoneyChains: {org} is registering you as a beekeeper. Privacy notice: {link} Your name on public pages: {name}. To agree, tell the officer this code: {code} (5 min).",
                ["es"] = "TOP4HoneyChains: {org} le registra como apicultor. Aviso de privacidad: {link} Su nombre en páginas públicas: {name}. Si está de acuerdo, diga este código al responsable: {code} (5 min).",
                ["pl"] = "TOP4HoneyChains: {org} rejestruje Cię jako pszczelarza. Informacja o danych: {link} Imię i nazwisko na stronach publicznych: {name}. Jeśli się zgadzasz, podaj kod: {code} (5 min).",
                ["lv"] = "TOP4HoneyChains: {org} jūs reģistrē kā biškopi. Privātuma paziņojums: {link} Jūsu vārds publiskajās lapās: {name}. Ja piekrītat, pasakiet šo kodu darbiniekam: {code} (5 min)."
            },
            // M2 credentials
            [SmsPurpose.Credentials] = new()
            {
                ["tr"] = "TOP4HoneyChains: kullanıcı adınız {user}, geçici şifreniz {password}. 72 saat içinde {link} adresinden giriş yapıp yeni şifrenizi belirleyin.",
                ["en"] = "TOP4HoneyChains: user name {user}, temporary password {password}. Sign in at {link} within 72 h and choose a new password.",
                ["es"] = "TOP4HoneyChains: usuario {user}, contraseña temporal {password}. Entre en {link} antes de 72 h y elija una contraseña nueva.",
                ["pl"] = "TOP4HoneyChains: login {user}, hasło tymczasowe {password}. Zaloguj się na {link} w ciągu 72 h i ustaw nowe hasło.",
                ["lv"] = "TOP4HoneyChains: lietotājvārds {user}, pagaidu parole {password}. 72 h laikā pieslēdzieties {link} un izvēlieties jaunu paroli."
            },
            // M3 delivery confirmation (used from P2)
            [SmsPurpose.IntakeConfirm] = new()
            {
                ["tr"] = "TOP4HoneyChains: {org} {date} tarihinde {containers}, {kg} kg {honey} teslim aldı; {samples} numune mühürlendi. Doğruysa kodu görevliye söyleyin: {code} (5 dk).",
                ["en"] = "TOP4HoneyChains: {org} received on {date}: {containers}, {kg} kg {honey}; {samples} samples sealed. If correct, tell the officer: {code} (5 min).",
                ["es"] = "TOP4HoneyChains: {org} recibió el {date}: {containers}, {kg} kg de {honey}; {samples} muestras precintadas. Si es correcto, diga al responsable: {code} (5 min).",
                ["pl"] = "TOP4HoneyChains: {org} przyjął {date}: {containers}, {kg} kg {honey}; zaplombowane próbki: {samples}. Jeśli się zgadza, podaj kod: {code} (5 min).",
                ["lv"] = "TOP4HoneyChains: {org} {date} saņēma: {containers}, {kg} kg {honey}; noplombēti paraugi: {samples}. Ja viss pareizi, pasakiet kodu: {code} (5 min)."
            },
            // M4 password reset
            [SmsPurpose.PasswordReset] = new()
            {
                ["tr"] = "TOP4HoneyChains: şifre sıfırlama kodunuz {code} (5 dk geçerli). Bu isteği siz yapmadıysanız mesajı dikkate almayın.",
                ["en"] = "TOP4HoneyChains: your password reset code is {code} (valid 5 min). If you did not ask for it, ignore this message.",
                ["es"] = "TOP4HoneyChains: su código para restablecer la contraseña es {code} (válido 5 min). Si no lo pidió, ignore este mensaje.",
                ["pl"] = "TOP4HoneyChains: kod do zmiany hasła: {code} (ważny 5 min). Jeśli to nie Ty, zignoruj tę wiadomość.",
                ["lv"] = "TOP4HoneyChains: paroles atjaunošanas kods: {code} (derīgs 5 min). Ja to neprasījāt, ignorējiet šo ziņu."
            },
            // M5 notice confirmation
            [SmsPurpose.NoticeConfirm] = new()
            {
                ["tr"] = "TOP4HoneyChains: {code} kodu, {org} aydınlatma metnini (sürüm {version}) okuduğunuzu onaylar. 5 dk geçerli.",
                ["en"] = "TOP4HoneyChains: code {code} confirms that you read the privacy notice of {org} (version {version}). Valid 5 min.",
                ["es"] = "TOP4HoneyChains: el código {code} confirma que leyó el aviso de privacidad de {org} (versión {version}). Válido 5 min.",
                ["pl"] = "TOP4HoneyChains: kod {code} potwierdza zapoznanie się z informacją o danych {org} (wersja {version}). Ważny 5 min.",
                ["lv"] = "TOP4HoneyChains: kods {code} apstiprina, ka izlasījāt {org} privātuma paziņojumu (versija {version}). Derīgs 5 min."
            },
            // M5 without the notice part: phone check
            [SmsPurpose.PhoneVerify] = new()
            {
                ["tr"] = "TOP4HoneyChains: telefon doğrulama kodunuz {code} (5 dk geçerli).",
                ["en"] = "TOP4HoneyChains: your phone verification code is {code} (valid 5 min).",
                ["es"] = "TOP4HoneyChains: su código de verificación del teléfono es {code} (válido 5 min).",
                ["pl"] = "TOP4HoneyChains: kod weryfikacji telefonu: {code} (ważny 5 min).",
                ["lv"] = "TOP4HoneyChains: tālruņa verifikācijas kods: {code} (derīgs 5 min)."
            }
        };

        private static readonly Dictionary<string, (string Yes, string No)> YesNo = new()
        {
            ["tr"] = ("EVET", "HAYIR"), ["en"] = ("YES", "NO"), ["es"] = ("SÍ", "NO"), ["pl"] = ("TAK", "NIE"), ["lv"] = ("JĀ", "NĒ")
        };

        private static readonly Regex Placeholder = new(@"\{([a-z]+)\}");

        public static IEnumerable<string> Purposes => Texts.Keys;

        public static string Language(string? language)
        {
            var lang = (language ?? "").Trim().ToLowerInvariant();
            if (lang.Length > 2)
                lang = lang[..2];
            return Languages.Contains(lang) ? lang : DefaultLanguage;
        }

        public static string Template(string purpose, string? language) =>
            Texts.TryGetValue(purpose, out var byLang) ? byLang[Language(language)]
                : throw new ArgumentException($"No SMS template for purpose {purpose}.", nameof(purpose));

        public static string YesNoWord(bool yes, string? language)
        {
            var w = YesNo[Language(language)];
            return yes ? w.Yes : w.No;
        }

        // Fills the placeholders; an unknown placeholder stays visible so a missing value is noticed in tests
        public static string Render(string purpose, string? language, IReadOnlyDictionary<string, string> values) =>
            Placeholder.Replace(Template(purpose, language), m => values.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);

        public static IEnumerable<string> PlaceholdersOf(string text) => Placeholder.Matches(text).Select(m => m.Groups[1].Value);
    }

    // How many billed parts a text needs: GSM 7-bit (160 / 153), the providers' Turkish encoding (155 / 150) or Unicode (70 / 67)
    public static class SmsSegments
    {
        private const string Gsm = "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà";
        private const string GsmExtension = "^{}\\[~]|€\f";
        private const string Turkish = "ğĞıİşŞçÇöÖüÜ";

        public enum Encoding { Gsm7, Turkish, Unicode }

        public static Encoding EncodingOf(string text)
        {
            if (text.All(c => Gsm.Contains(c) || GsmExtension.Contains(c)))
                return Encoding.Gsm7;
            if (text.All(c => Gsm.Contains(c) || GsmExtension.Contains(c) || Turkish.Contains(c)))
                return Encoding.Turkish;
            return Encoding.Unicode;
        }

        public static int Count(string text)
        {
            var encoding = EncodingOf(text);
            var length = encoding == Encoding.Unicode ? text.Length : text.Sum(c => GsmExtension.Contains(c) ? 2 : 1);
            var (single, part) = encoding switch
            {
                Encoding.Gsm7 => (160, 153),
                Encoding.Turkish => (155, 150),
                _ => (70, 67)
            };
            if (length == 0)
                return 1;
            return length <= single ? 1 : (length + part - 1) / part;
        }
    }

    public static class PhoneMask
    {
        // "+905321234567" -> "+90*******4567": country code and the last four digits stay visible
        public static string Mask(string e164)
        {
            var digits = new string(e164.Where(char.IsDigit).ToArray());
            if (digits.Length <= 6)
                return "+" + new string('*', digits.Length);
            var cc = 2;
            try
            {
                var n = PhoneNumbers.PhoneNumberUtil.GetInstance().Parse(e164, null);
                cc = n.CountryCode.ToString().Length;
            }
            catch (PhoneNumbers.NumberParseException)
            {
            }
            return "+" + digits[..cc] + new string('*', digits.Length - cc - 4) + digits[^4..];
        }
    }
}
