namespace TOP4HoneyChains.Web.Services
{
    // D-108: the short explanatory video of each laboratory parameter (CCP 1), shown in the value sheet of the Honey Story.
    // The videos are not in git and not under wwwroot: they live in a folder on the server (setting Videos:Root, default
    // App_Data/parameter-videos) as <lang>/<key>.mp4 and are copied there by tools/ImportParameterVideos.ps1 (keep its key list
    // in step with Keys below). Only the known keys and languages are served; the page asks for the page language, then English,
    // then Turkish, and without any file the sheet shows the text only.
    public class ParameterVideos
    {
        public static readonly string[] Languages = { "tr", "en", "es", "pl", "lv" };

        // key -> the parameter names as stored in the database (CCP 1; a few English/Turkish spellings exist)
        public static readonly IReadOnlyDictionary<string, string[]> Keys = new Dictionary<string, string[]>
        {
            ["moisture"] = new[] { "Moisture", "Nem" },
            ["hmf"] = new[] { "HMF" },
            ["diastase"] = new[] { "Diastase", "Diastaz" },
            ["c4-sugar"] = new[] { "C4 Sugar", "Sugar Balance" },
            ["sucrose"] = new[] { "Sucrose", "Sakkaroz" },
            ["fructose-glucose"] = new[] { "Fruktoz", "Glikoz", "Fructose", "Glucose" },
            ["maltose"] = new[] { "Maltoz", "Maltose" },
            ["pesticides"] = new[] { "Pesticides", "Pestisit" },
            ["antibiotics"] = new[] { "Antibiotics", "Antibiyotik" },
            ["electrical-conductivity"] = new[] { "Electrical Conductivity" },
            ["proline"] = new[] { "Proline", "Prolin" },
            ["ph"] = new[] { "pH" },
            ["free-acidity"] = new[] { "Free Acidity", "Serbest Asitlik" },
            ["insoluble-matter"] = new[] { "Insoluble Matter" },
            ["naphthalene"] = new[] { "Naphthalene", "Naftalin" }
        };

        private static readonly Dictionary<string, string> KeyOfName = Keys
            .SelectMany(k => k.Value.Select(n => (Name: n, k.Key)))
            .ToDictionary(x => x.Name, x => x.Key, StringComparer.OrdinalIgnoreCase);

        private readonly string _root;

        public ParameterVideos(IConfiguration configuration, IWebHostEnvironment env)
        {
            var root = configuration["Videos:Root"];
            _root = string.IsNullOrWhiteSpace(root) ? Path.Combine(env.ContentRootPath, "App_Data", "parameter-videos")
                : Path.IsPathRooted(root) ? root : Path.Combine(env.ContentRootPath, root);
        }

        public static string? KeyOf(string? parameterName) =>
            parameterName != null && KeyOfName.TryGetValue(parameterName.Trim(), out var key) ? key : null;

        public static bool IsKnown(string lang, string key) => Languages.Contains(lang) && Keys.ContainsKey(key);

        // the file of a known key and language, null when it is not there
        public string? File(string lang, string key)
        {
            if (!IsKnown(lang, key))
                return null;
            var path = Path.Combine(_root, lang, key + ".mp4");
            return System.IO.File.Exists(path) ? path : null;
        }

        // the language whose video is shown for the page language: the page language, then English, then Turkish
        public string? LanguageFor(string key, string pageLang)
        {
            foreach (var lang in new[] { pageLang, "en", "tr" }.Distinct())
                if (File(lang, key) != null)
                    return lang;
            return null;
        }
    }
}
