namespace TOP4HoneyChains.Web.Services
{
    // Approximate centre of each province (the provincial capital, WGS84 degrees) and its geographic region,
    // by plate code in the order of Provinces. Used to mark the beekeeper's area on the storefront map: the
    // marker is never placed at an apiary. The database holds no coordinates for provinces or districts.
    public static class ProvinceMap
    {
        public record Centre(double Latitude, double Longitude, string Region);

        public static class Regions
        {
            public const string Marmara = "Marmara";
            public const string Aegean = "Aegean";
            public const string Mediterranean = "Mediterranean";
            public const string CentralAnatolia = "CentralAnatolia";
            public const string BlackSea = "BlackSea";
            public const string EasternAnatolia = "EasternAnatolia";
            public const string SoutheasternAnatolia = "SoutheasternAnatolia";

            public static readonly string[] All = { Marmara, Aegean, Mediterranean, CentralAnatolia, BlackSea, EasternAnatolia, SoutheasternAnatolia };
        }

        private const string Mar = Regions.Marmara, Ege = Regions.Aegean, Akd = Regions.Mediterranean, Ica = Regions.CentralAnatolia,
            Kar = Regions.BlackSea, Dog = Regions.EasternAnatolia, Gda = Regions.SoutheasternAnatolia;

        private static readonly Centre[] Centres =
        {
            new(37.00, 35.32, Akd), new(37.76, 38.28, Gda), new(38.76, 30.54, Ege), new(39.72, 43.05, Dog), new(40.65, 35.83, Kar),   // 01-05
            new(39.93, 32.85, Ica), new(36.89, 30.71, Akd), new(41.18, 41.82, Kar), new(37.85, 27.85, Ege), new(39.65, 27.88, Mar),   // 06-10
            new(40.14, 29.98, Mar), new(38.88, 40.50, Dog), new(38.40, 42.11, Dog), new(40.74, 31.61, Kar), new(37.72, 30.29, Akd),   // 11-15
            new(40.19, 29.06, Mar), new(40.15, 26.41, Mar), new(40.60, 33.62, Ica), new(40.55, 34.95, Kar), new(37.78, 29.09, Ege),   // 16-20
            new(37.91, 40.24, Gda), new(41.68, 26.56, Mar), new(38.67, 39.22, Dog), new(39.75, 39.49, Dog), new(39.90, 41.27, Dog),   // 21-25
            new(39.78, 30.52, Ica), new(37.07, 37.38, Gda), new(40.91, 38.39, Kar), new(40.46, 39.48, Kar), new(37.58, 43.74, Dog),   // 26-30
            new(36.20, 36.16, Akd), new(37.76, 30.55, Akd), new(36.80, 34.64, Akd), new(41.01, 28.98, Mar), new(38.42, 27.14, Ege),   // 31-35
            new(40.60, 43.10, Dog), new(41.38, 33.78, Kar), new(38.73, 35.49, Ica), new(41.74, 27.22, Mar), new(39.15, 34.16, Ica),   // 36-40
            new(40.77, 29.92, Mar), new(37.87, 32.48, Ica), new(39.42, 29.98, Ege), new(38.35, 38.31, Dog), new(38.61, 27.43, Ege),   // 41-45
            new(37.58, 36.94, Akd), new(37.31, 40.74, Gda), new(37.22, 28.36, Ege), new(38.75, 41.51, Dog), new(38.62, 34.71, Ica),   // 46-50
            new(37.97, 34.68, Ica), new(40.98, 37.88, Kar), new(41.02, 40.52, Kar), new(40.78, 30.40, Mar), new(41.29, 36.33, Kar),   // 51-55
            new(37.93, 41.94, Gda), new(42.03, 35.15, Kar), new(39.75, 37.02, Ica), new(40.98, 27.51, Mar), new(40.31, 36.55, Kar),   // 56-60
            new(41.00, 39.72, Kar), new(39.11, 39.55, Dog), new(37.17, 38.79, Gda), new(38.68, 29.41, Ege), new(38.50, 43.38, Dog),   // 61-65
            new(39.82, 34.81, Ica), new(41.46, 31.79, Kar), new(38.37, 34.03, Ica), new(40.26, 40.23, Kar), new(37.18, 33.22, Ica),   // 66-70
            new(39.85, 33.51, Ica), new(37.89, 41.13, Gda), new(37.52, 42.46, Gda), new(41.64, 32.34, Kar), new(41.11, 42.70, Dog),   // 71-75
            new(39.92, 44.05, Dog), new(40.65, 29.27, Mar), new(41.20, 32.62, Kar), new(36.72, 37.12, Gda), new(37.07, 36.25, Akd),   // 76-80
            new(40.84, 31.16, Kar)                                                                                                  // 81
        };

        public static int Count => Centres.Length;

        public static Centre? ByPlate(int plate) => plate is >= 1 and <= 81 ? Centres[plate - 1] : null;

        public static Centre? ByName(string? province) => ByPlate(Provinces.PlateOf(province));

        // The inline map of Türkiye (viewBox 0 0 640 300) uses a Mercator projection fitted to it
        public static (double X, double Y) ToMap(double latitude, double longitude)
        {
            var x = -821.154 + 1855.143 * longitude * Math.PI / 180;
            var y = 1524.674 - 1855.143 * Math.Log(Math.Tan(Math.PI / 4 + latitude * Math.PI / 360));
            return (Math.Round(x, 1), Math.Round(y, 1));
        }
    }
}
