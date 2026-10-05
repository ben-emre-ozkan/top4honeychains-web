namespace TOP4HoneyChains.Web.Services
{
    // Turkish provinces by plate code (01-81). The enterprise registry number is TR + plate code + 10 digits
    // (D-29), so a verified number tells the province of the beekeeper's enterprise.
    public static class Provinces
    {
        private static readonly string[] Names =
        {
            "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin", "Aydın", "Balıkesir",
            "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli",
            "Diyarbakır", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari",
            "Hatay", "Isparta", "Mersin", "İstanbul", "İzmir", "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
            "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla", "Muş", "Nevşehir",
            "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas", "Tekirdağ", "Tokat",
            "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van", "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman",
            "Kırıkkale", "Batman", "Şırnak", "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
        };

        public static string? ByPlate(int plate) => plate is >= 1 and <= 81 ? Names[plate - 1] : null;

        // "Düzce" -> 81 (Turkish casing rules); 0 when unknown
        public static int PlateOf(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return 0;
            var tr = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");
            var i = Array.FindIndex(Names, n => string.Compare(n, name.Trim(), tr, System.Globalization.CompareOptions.IgnoreCase) == 0);
            return i + 1;
        }

        // "TR061234567890" -> "Ankara"; null when the number does not have the TR + 2 digits form
        public static string? FromRegistryNumber(string? number)
        {
            if (number == null || number.Length < 4 || !number.StartsWith("TR", StringComparison.OrdinalIgnoreCase))
                return null;
            return int.TryParse(number.AsSpan(2, 2), out var plate) ? ByPlate(plate) : null;
        }
    }
}
