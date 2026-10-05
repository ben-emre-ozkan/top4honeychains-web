namespace TOP4HoneyChains.Web.Services
{
    // Units are stored with a long name, e.g. "% (Percentage)" or "DN (Schade)". The public pages show only the
    // symbol ("%", "DN") and keep the long form as a tooltip.
    public static class Units
    {
        public static bool IsNone(string? unit) => string.IsNullOrWhiteSpace(unit) || unit.Trim() == "None";

        public static string Symbol(string? unit)
        {
            if (IsNone(unit))
                return "";
            var u = unit!.Trim();
            var open = u.IndexOf(" (", StringComparison.Ordinal);
            return open > 0 && u.EndsWith(')') ? u[..open].Trim() : u;
        }
    }
}
