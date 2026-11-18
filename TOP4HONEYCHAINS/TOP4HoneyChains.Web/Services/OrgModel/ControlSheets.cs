using System.Globalization;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-88: control sheets. All CCPs of a lot except CCP 0 (the primary audit at intake) and CCP 1 (written from the approved lab
    // report), and every CCP of a batch, are entered on one page and approved in one action; the four-eyes rule still applies to
    // each record. The pages of single CCPs stay (files, "Not applicable" removal, corrections).
    public static class ControlSheets
    {
        public static readonly string[] OwnPageLotCcps = { LabService.Ccp0Code, "CCP 1" };

        public static bool OnSheet(string kind, string? code) => kind == "batch" || !OwnPageLotCcps.Contains(code);

        public static string Url(string kind, int id) => $"/CCPMonitoring/Sheet?kind={kind}&id={id}";

        // "CCP 4–10" for a run of three or more codes, otherwise the codes listed
        public static string Codes(IEnumerable<string?> codes)
        {
            var list = codes.Where(c => !string.IsNullOrEmpty(c)).Select(c => c!).Distinct().ToList();
            var numbers = list.Select(c => c.StartsWith("CCP ", StringComparison.Ordinal) && int.TryParse(c[4..], NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : (int?)null).ToList();
            if (list.Count >= 3 && numbers.All(n => n != null))
            {
                var sorted = numbers.Select(n => n!.Value).OrderBy(n => n).ToList();
                if (sorted.Last() - sorted.First() == sorted.Count - 1)
                    return $"CCP {sorted.First()}–{sorted.Last()}";
            }
            return string.Join(", ", list.OrderBy(c => numbers[list.IndexOf(c)] ?? int.MaxValue));
        }
    }
}
