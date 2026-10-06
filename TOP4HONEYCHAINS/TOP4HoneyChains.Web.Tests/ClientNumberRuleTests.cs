using System.Diagnostics;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-66: the client-side number rule (wwwroot/js/validation.js) accepts exactly what DecimalInput accepts (D-53).
    // Runs the script with Node.js when it is installed; without Node the comparison cannot run and the test returns.
    public class ClientNumberRuleTests
    {
        private static readonly string[] Inputs =
        {
            "1,5", "1.5", "15", "-1,5", " 12,75 ", "0,0001", ",5", "1.234,5", "1,234.5", "1.234", "1,234", "1.234.567", "1 234,5",
            "17,2", "17.2", "", "   ", "abc", "1,2,3", "1.2.3", "1,23.45,6", "12.34,5.6", "1e3", "5.", "+3", "1.234.56", "12,3456"
        };

        private static string? FindScript()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var path = Path.Combine(dir.FullName, "TOP4HoneyChains.Web", "wwwroot", "js", "validation.js");
                if (File.Exists(path))
                    return path;
            }
            return null;
        }

        [Fact]
        public void The_client_rule_matches_the_server_parser()
        {
            var script = FindScript();
            Assert.NotNull(script);
            var probe = "global.window={};window.jQuery=Object.assign(function(){},{validator:{methods:{}}});require(process.argv[1]);" +
                        "const lines=require('fs').readFileSync(0,'utf8').split('\\n');lines.pop();" +
                        "console.log(lines.map(t=>{const v=window.T4hDecimal.parse(t);return isNaN(v)?'x':String(v);}).join('|'));";
            Process? node;
            try
            {
                node = Process.Start(new ProcessStartInfo("node", new[] { "-e", probe, script! })
                {
                    RedirectStandardInput = true, RedirectStandardOutput = true, UseShellExecute = false
                });
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return;   // Node.js is not installed here
            }
            node!.StandardInput.Write(string.Join("\n", Inputs) + "\n");
            node.StandardInput.Close();
            var output = node.StandardOutput.ReadToEnd().Trim().Split('|');
            node.WaitForExit();

            Assert.Equal(Inputs.Length, output.Length);
            for (var i = 0; i < Inputs.Length; i++)
            {
                var server = DecimalInput.TryParse(Inputs[i], out var d) ? d.ToString(System.Globalization.CultureInfo.InvariantCulture) : "x";
                var client = output[i] == "x" ? "x" : decimal.Parse(output[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture)
                    .ToString(System.Globalization.CultureInfo.InvariantCulture);
                Assert.True(server == client, $"\"{Inputs[i]}\": server {server}, client {client}");
            }
        }
    }
}
