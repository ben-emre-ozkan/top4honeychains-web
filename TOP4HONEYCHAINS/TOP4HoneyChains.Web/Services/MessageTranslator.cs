using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;

namespace TOP4HoneyChains.Web.Services
{
    // Translates server messages (flash messages, denial reasons, lock reasons, finalize blockers, validation
    // errors) at display time. The English message is the resource key; templates use {0}, {1}… for the
    // variable parts, which are translated in turn. A message without a translation stays English.
    public class MessageTranslator
    {
        private record Template(Regex Pattern, string Translation, int FixedLength);

        private static readonly ConcurrentDictionary<string, (Dictionary<string, string> Exact, List<Template> Templates)> Cache = new();
        private static readonly Regex Placeholder = new(@"\{(\d+)\}");
        private static readonly Regex SentenceBreak = new(@"(?<=\.) (?=[A-Z])");
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly RefText? _ref;

        public MessageTranslator(IStringLocalizer<SharedResource> localizer, RefText? refText = null)
        {
            _localizer = localizer;
            _ref = refText;
        }

        private (Dictionary<string, string> Exact, List<Template> Templates) Table() =>
            Cache.GetOrAdd(CultureInfo.CurrentUICulture.Name, _ =>
            {
                var exact = new Dictionary<string, string>(StringComparer.Ordinal);
                var templates = new List<Template>();
                IEnumerable<LocalizedString> all;
                try { all = _localizer.GetAllStrings(includeParentCultures: false).ToList(); }
                catch (MissingManifestResourceException) { all = Array.Empty<LocalizedString>(); }
                foreach (var s in all)
                {
                    // UI keys (Org.Title, Corr.Help…) are not messages
                    if (s.Name == s.Value || Regex.IsMatch(s.Name, @"^[A-Za-z]+\.[A-Za-z]+$"))
                        continue;
                    if (Placeholder.IsMatch(s.Name))
                    {
                        var pattern = "^" + Placeholder.Replace(Regex.Escape(s.Name).Replace(@"\{", "{"), m => $@"(?<p{m.Groups[1].Value}>(?:(?!\. [A-Z]).)+?)") + "$";
                        templates.Add(new Template(new Regex(pattern, RegexOptions.Singleline), s.Value, Placeholder.Replace(s.Name, "").Length));
                    }
                    else
                        exact[s.Name] = s.Value;
                }
                // most specific (longest fixed text) first
                templates.Sort((a, b) => b.FixedLength.CompareTo(a.FixedLength));
                return (exact, templates);
            });

        public string T(string? message) => message == null ? "" : Translate(message, 0);

        private string Translate(string text, int depth)
        {
            if (string.IsNullOrWhiteSpace(text) || depth > 3)
                return text;
            var (exact, templates) = Table();
            if (exact.Count == 0 && templates.Count == 0)
                return text;
            if (exact.TryGetValue(text, out var hit))
                return hit;
            foreach (var t in templates)
            {
                var m = t.Pattern.Match(text);
                if (!m.Success)
                    continue;
                return Placeholder.Replace(t.Translation, p =>
                {
                    var g = m.Groups["p" + p.Groups[1].Value];
                    return g.Success ? Translate(g.Value, depth + 1) : p.Value;
                });
            }
            // several sentences joined together (e.g. a list of finalize blockers); a placeholder never spans sentences
            var parts = SentenceBreak.Split(text);
            if (parts.Length > 1)
                return string.Join(" ", parts.Select(p => Translate(p, depth + 1)));
            // D-51: a variable part that is reference data (CCP title, status, parameter, honey type)
            return depth > 0 ? _ref?.AnyText(text) ?? text : text;
        }
    }

    // Translates the model-state errors of a view result (AddModelError messages) before the view renders
    public class TranslateModelStateFilter : IAsyncResultFilter
    {
        private readonly MessageTranslator _translator;

        public TranslateModelStateFilter(MessageTranslator translator) => _translator = translator;

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ViewResult or PartialViewResult && context.ModelState.ErrorCount > 0)
            {
                foreach (var entry in context.ModelState.ToList())
                {
                    var errors = entry.Value.Errors.ToList();
                    if (errors.Count == 0 || errors.All(e => string.IsNullOrEmpty(e.ErrorMessage)))
                        continue;
                    entry.Value.Errors.Clear();
                    foreach (var e in errors)
                    {
                        if (string.IsNullOrEmpty(e.ErrorMessage))
                            entry.Value.Errors.Add(e.Exception!);
                        else
                            entry.Value.Errors.Add(_translator.T(e.ErrorMessage));
                    }
                }
            }
            await next();
        }
    }
}
