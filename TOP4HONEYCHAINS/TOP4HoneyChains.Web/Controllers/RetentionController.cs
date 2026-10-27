using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P5 (D78-SPEC 6.3, D-84): the retention report (dry run, always) and the run (only with Retention:ApplyEnabled, and
    // only by a platform administrator who confirms the owner's approval). The page never changes data by itself.
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class RetentionController : Controller
    {
        private readonly RetentionService _retention;
        private readonly IStringLocalizer<SharedResource> _l;

        public RetentionController(RetentionService retention, IStringLocalizer<SharedResource> l)
        {
            _retention = retention;
            _l = l;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Options = _retention.Options;
            ViewBag.LastRun = await _retention.LastRunAsync();
            var rules = await _retention.DryRunAsync();
            ViewBag.Kept = rules.ToDictionary(r => r.Key, r => Kept(r.Key, _retention.Options));
            return View(rules);
        }

        // the dry-run report as a file (counts only, no personal data)
        public async Task<IActionResult> Report()
        {
            var o = _retention.Options;
            var sb = new StringBuilder("Rule;Data;Kept;Then;Cutoff (UTC);Rows;Changed by the job\n");
            foreach (var r in await _retention.DryRunAsync())
                sb.Append(r.Key).Append(';').Append(_l[$"Ret.{r.Key}.Data"].Value).Append(';').Append(Kept(r.Key, o)).Append(';')
                  .Append(_l[$"Ret.{r.Key}.Then"].Value).Append(';').Append(r.Cutoff.ToString("yyyy-MM-dd")).Append(';').Append(r.Count).Append(';')
                  .Append(r.Applies ? "yes" : "no").Append('\n');
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"retention-dry-run-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(bool approved)
        {
            if (!_retention.Options.ApplyEnabled)
                TempData["Error"] = "Retention is off (Retention:ApplyEnabled); its first run is the owner's decision.";
            else if (!approved)
                TempData["Error"] = "Confirm that the owner approved this run.";
            else
            {
                var r = await _retention.ApplyAsync("manual");
                TempData["Success"] = $"Retention run: {r.Codes} codes deleted, {r.Sms} SMS messages cleared, {r.Prices} prices deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        public string Kept(string key, RetentionOptions o) => key switch
        {
            "codes" => string.Format(_l["Ret.codes.Kept"].Value, o.OneTimeCodeDays),
            "sms" => string.Format(_l["Ret.Years"].Value, o.SmsYears),
            "prices" => string.Format(_l["Ret.Years"].Value, o.PriceYears),
            "lots" => string.Format(_l["Ret.lots.Kept"].Value, o.ProductYears, o.BestBeforeYears),
            "consents" => string.Format(_l["Ret.consents.Kept"].Value, o.ProofYears),
            "accounts" => string.Format(_l["Ret.accounts.Kept"].Value, o.FormerMemberYears),
            "audit" => string.Format(_l["Ret.Years"].Value, o.AuditYears),
            _ => _l[$"Ret.{key}.Kept"].Value
        };
    }
}
