using System.Globalization;
using System.Text;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Tests.Demo
{
    // D-86: plausible values for the demo honey, all within the platform's limits (moisture, HMF, diastase, sugars, proline, …). The
    // same values (docs/partner-test/values.json) are suggested to the testers in the partner test guide.
    public static class DemoValues
    {
        private sealed record Values(Dictionary<string, string> Numbers, Dictionary<string, string> Texts, Dictionary<string, string> Ccp0, string OtherYesNo);

        private static readonly Lazy<Values> Data = new(() =>
        {
            using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(DemoReference.RepoFile("docs", "partner-test", "values.json")));
            Dictionary<string, string> Map(string name) => json.RootElement.GetProperty(name).EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.GetString()!, StringComparer.OrdinalIgnoreCase);
            return new Values(Map("numbers"), Map("texts"), Map("ccp0"), json.RootElement.GetProperty("otherYesNo").GetString()!);
        });

        public static string For(string ccp, CriticalControlPointParameter p, DemoCountry c)
        {
            var v = Data.Value;
            var key = $"{ccp}|{p.ParameterName.Trim()}";
            if (p.DataType == CcpDataType.Number)
                return v.Numbers.TryGetValue(key, out var n) ? n : throw new InvalidOperationException("no demo value for " + key);
            if (v.Texts.TryGetValue(key, out var t))
                return t.Replace("{country}", c.CountryName).Replace("{botanical}", c.Botanical);
            if (ccp == "CCP 0")
                return p.DataType == CcpDataType.YesNo ? v.Ccp0["yesNo"]
                    : v.Ccp0.FirstOrDefault(x => x.Key != "yesNo" && x.Key != "text" && p.ParameterName.StartsWith(x.Key, StringComparison.Ordinal)).Value ?? v.Ccp0["text"];
            if (p.DataType == CcpDataType.YesNo)
                return v.OtherYesNo;
            throw new InvalidOperationException("no demo value for " + key);
        }
    }

    // D-86: a one-page PDF with a few lines of text (lab report, signed forms of the demo). Latin letters only: the text is
    // written with the standard Helvetica font, so diacritics are dropped.
    public static class DemoFiles
    {
        public static byte[] Pdf(string title, params string[] lines)
        {
            static string Plain(string s)
            {
                var decomposed = s.Replace('ł', 'l').Replace('Ł', 'L').Replace('ı', 'i').Replace('İ', 'I').Replace('–', '-').Normalize(NormalizationForm.FormD);
                var b = new StringBuilder();
                foreach (var ch in decomposed)
                    if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark && ch < 128)
                        b.Append(ch is '(' or ')' or '\\' ? "\\" + ch : ch.ToString());
                return b.ToString();
            }
            var content = new StringBuilder();
            content.Append("BT /F1 16 Tf 56 780 Td (").Append(Plain(title)).Append(") Tj ET\n");
            var y = 750;
            foreach (var line in lines.Append("TOP4HoneyChains demo environment - " + DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
            {
                content.Append("BT /F1 11 Tf 56 ").Append(y.ToString(CultureInfo.InvariantCulture)).Append(" Td (").Append(Plain(line)).Append(") Tj ET\n");
                y -= 18;
            }
            var stream = content.ToString();
            var objects = new[]
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}endstream"
            };
            var pdf = new StringBuilder("%PDF-1.4\n");
            var offsets = new List<int>();
            for (var i = 0; i < objects.Length; i++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }
            var xref = Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
            foreach (var o in offsets)
                pdf.Append(o.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
            pdf.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            return Encoding.ASCII.GetBytes(pdf.ToString());
        }
    }

    // D-86: what the partner test guide needs to know about the demo data (no passwords)
    public sealed class DemoManifest
    {
        public SortedDictionary<string, CountryData> Countries { get; set; } = new();

        // the CCP parameters with their limits (reference data), for the suggested values of the guide and the walkthrough
        public List<Parameter> Parameters { get; set; } = new();

        public sealed record Parameter(int Id, string Ccp, string Name, string Type, string? Unit, decimal? Min, decimal? Max, int? RequiredIf);

        public CountryData Country(string code)
        {
            if (!Countries.TryGetValue(code, out var c))
                Countries[code] = c = new CountryData();
            return c;
        }

        public sealed class CountryData
        {
            public string Language { get; set; } = "";
            public string Currency { get; set; } = "";
            public string Province { get; set; } = "";
            public List<string> Regions { get; set; } = new();
            public SortedDictionary<string, Org> Orgs { get; set; } = new();
            public Dictionary<string, Account> Accounts { get; set; } = new();
            public SortedDictionary<string, Beekeeper> Beekeepers { get; set; } = new();
            public Example? Example { get; set; }
        }

        public sealed class Org
        {
            public string Name { get; set; } = "";
            public int Id { get; set; }
        }

        public sealed class Account
        {
            public string UserName { get; set; } = "";
            public string Role { get; set; } = "";
            public string Org { get; set; } = "";
            public string Phone { get; set; } = "";
        }

        public sealed class Beekeeper
        {
            public string Name { get; set; } = "";
            public string Id { get; set; } = "";
            public string BusinessNumber { get; set; } = "";
            public string RegistryNumber { get; set; } = "";
            public string Phone { get; set; } = "";
            public string District { get; set; } = "";
            public string Locality { get; set; } = "";
            public int RegistrationId { get; set; }
            public int ApiaryId { get; set; }
            public bool Photo { get; set; }
            public string? PortalUserName { get; set; }
        }

        public sealed class Example
        {
            public int IntakeId { get; set; }
            public int LotId { get; set; }
            public int BatchId { get; set; }
            public string BatchTitle { get; set; } = "";
            public string Qr { get; set; } = "";
            public string ReportNumber { get; set; } = "";
            public string StorefrontCode { get; set; } = "";
        }
    }
}
