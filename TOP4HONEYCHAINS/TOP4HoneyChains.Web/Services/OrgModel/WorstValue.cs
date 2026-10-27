using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P3 (D-82, P2 follow-up 2): when the container jars of a lot are analysed one by one (Lab:AnalysisMode = PerContainer),
    // the lot's CCP 1 value of a parameter is the WORST of the containers' values, the conservative one for the limits:
    //   * a value outside its limits beats every value inside them; among values outside, the one furthest outside wins;
    //   * otherwise the value closest to the limit in the direction that matters (highest under a maximum, lowest under a minimum,
    //     the smallest margin between a minimum and a maximum); without a limit the highest value (contaminants grow upwards);
    //   * a text value: every container's text, prefixed by its container code (all are shown, none is dropped).
    public static class WorstValue
    {
        public record Candidate(string Tag, decimal? Number, string? Text, IReadOnlyDictionary<int, decimal> Siblings);

        public static int? Pick(CriticalControlPointParameter parameter, IReadOnlyList<Candidate> candidates, HoneyProfile honey)
        {
            var numeric = candidates.Select((c, i) => (c, i)).Where(x => x.c.Number != null).ToList();
            if (numeric.Count == 0)
                return null;
            var scored = numeric.Select(x =>
            {
                var v = x.c.Number!.Value;
                var result = CcpLimitEvaluator.Evaluate(parameter, v, x.c.Siblings, honey);
                var bounds = CcpLimitEvaluator.MainBounds(parameter, x.c.Siblings, honey);
                var direction = CcpLimitEvaluator.WorseDirection(parameter, x.c.Siblings, honey);
                return (x.i, outside: result.IsInRange == false, score: Score(v, bounds, direction));
            }).ToList();
            // outside the limits first, then the highest badness score; the first container wins a tie
            return scored.OrderByDescending(s => s.outside).ThenByDescending(s => s.score).ThenBy(s => s.i).First().i;
        }

        // the higher, the worse
        private static decimal Score(decimal v, (decimal? Min, decimal? Max)? bounds, int? direction)
        {
            var (min, max) = bounds ?? (null, null);
            if (min != null && max != null && max > min)
            {
                var span = max.Value - min.Value;
                // distance to the nearer limit, negative inside: the smallest margin is the worst
                return -Math.Min((max.Value - v) / span, (v - min.Value) / span);
            }
            return direction == -1 ? -v : v;
        }

        public static string? CombineTexts(IEnumerable<Candidate> candidates)
        {
            var texts = candidates.Where(c => !string.IsNullOrWhiteSpace(c.Text)).ToList();
            if (texts.Count == 0)
                return null;
            if (texts.Select(c => c.Text!.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1)
                return texts[0].Text!.Trim();
            var joined = string.Join("; ", texts.Select(c => $"{c.Tag}: {c.Text!.Trim()}"));
            return joined.Length > 100 ? joined[..99] + "…" : joined;
        }
    }
}
