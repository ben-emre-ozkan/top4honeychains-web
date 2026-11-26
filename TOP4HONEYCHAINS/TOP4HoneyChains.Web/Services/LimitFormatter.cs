using System.Globalization;
using Microsoft.Extensions.Localization;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services
{
    // D-67: limit texts for display, built from the limit rows in the selected language and one format:
    //   "≤ 23 %", "≥ 3 DN when HMF ≤ 15", "Fructose + Glucose ≥ 45 %", "Fructose / Glucose 0,9–1,4", "Pine: ≤ 23 %"
    // Parameter names and honey types come from the reference translations, units are shown as symbols only.
    // The stored AppliedLimit (English, D-51) stays as it is for the audit; it is no longer what the pages show.
    public class LimitFormatter
    {
        private readonly RefText _ref;
        private readonly IStringLocalizer<SharedResource> _l;

        public LimitFormatter(RefText refText, IStringLocalizer<SharedResource> localizer)
        {
            _ref = refText;
            _l = localizer;
        }

        // The limits of a parameter (its Limits with their related parameters loaded). With a honey profile only the
        // rows for that honey are shown and without a scope label; otherwise every row, scoped by honey class or type.
        public string? Describe(CriticalControlPointParameter p, HoneyProfile? honey = null)
        {
            // D-98: a yes/no question with an expected answer
            if (CcpLimitEvaluator.ExpectedAnswer(p) is string expected)
                return $"{_l["Limit.expected"].Value} {_l[expected].Value}";
            var known = honey != null && (honey.HoneyClass != null || honey.HoneyTypeId != null);
            var rows = p.Limits
                .Where(l => !known || ((l.HoneyTypeId == null || l.HoneyTypeId == honey!.HoneyTypeId) && (l.HoneyClass == null || l.HoneyClass == honey!.HoneyClass)))
                .OrderBy(l => l.HoneyTypeId != null).ThenBy(l => l.HoneyClass).ThenBy(l => l.LimitId)
                .ToList();

            // D-94: for a known honey only the main limit that applies is shown. As in CcpLimitEvaluator, a row for the
            // honey type beats one for the honey class, which beats one for every honey; so a less specific main row is
            // dropped once a more specific one without a condition exists (chestnut: "≥ 0.8 mS/cm", not "≤ 0.8; ≥ 0.8").
            if (known)
            {
                static bool IsMain(CriticalControlPointParameterLimit l) => l.RatioWithParameterId == null && l.SumWithParameterId == null;
                static int Level(CriticalControlPointParameterLimit l) => l.HoneyTypeId != null ? 2 : l.HoneyClass != null ? 1 : 0;
                var unconditional = rows.Where(l => IsMain(l) && l.ConditionParameterId == null).ToList();
                if (unconditional.Count > 0)
                {
                    var level = unconditional.Max(Level);
                    rows = rows.Where(l => !IsMain(l) || Level(l) >= level).ToList();
                }
            }
            var unit = UnitSymbol(p);
            var parts = new List<string>();

            // the parameter's own default range applies unless a main row for this honey replaces it
            var mainRowForHoney = known && rows.Any(l => l.RatioWithParameterId == null && l.SumWithParameterId == null && l.ConditionParameterId == null);
            if ((p.MinValue != null || p.MaxValue != null) && !mainRowForHoney)
                parts.Add(Range(p.MinValue, p.MaxValue, unit));

            foreach (var l in rows)
            {
                string text;
                if (l.RatioWithParameterId != null)
                    text = $"{Name(p.ParameterId, p.ParameterName)} / {Name(l.RatioWithParameterId, l.RatioWithParameter?.ParameterName)} {Range(l.MinValue, l.MaxValue, "")}";
                else if (l.SumWithParameterId != null)
                    text = $"{Name(p.ParameterId, p.ParameterName)} + {Name(l.SumWithParameterId, l.SumWithParameter?.ParameterName)} {Range(l.MinValue, l.MaxValue, unit)}";
                else
                    text = Range(l.MinValue, l.MaxValue, unit);
                if (l.ConditionParameterId != null && l.ConditionMaxValue != null)
                {
                    var condition = $"{Name(l.ConditionParameterId, l.ConditionParameter?.ParameterName)} ≤ {Number(l.ConditionMaxValue.Value)}{Spaced(l.ConditionParameter == null ? "" : UnitSymbol(l.ConditionParameter))}";
                    text += " " + string.Format(_l["Limit.When"].Value, condition);
                }
                if (!known)
                {
                    var scope = l.HoneyTypeId != null ? _ref.HoneyType(l.HoneyTypeId, l.HoneyType?.HoneyTypeTitle) : l.HoneyClass != null ? _l[l.HoneyClass].Value : null;
                    if (scope != null)
                        text = $"{scope}: {text}";
                }
                parts.Add(text.Trim());
            }
            return parts.Count == 0 ? null : string.Join("; ", parts.Distinct());
        }

        // "Moisture" and "%": the name and the unit symbol of a parameter in the selected language
        public string Name(int? id, string? name) => _ref.Parameter(id, name);

        public string UnitSymbol(CriticalControlPointParameter p) => Units.Symbol(_ref.Unit(p.ParameterId, p.ParameterUnit));

        // "Moisture (%)" for labels
        public string Label(CriticalControlPointParameter p)
        {
            var unit = UnitSymbol(p);
            return unit.Length == 0 ? Name(p.ParameterId, p.ParameterName) : $"{Name(p.ParameterId, p.ParameterName)} ({unit})";
        }

        public string Range(decimal? min, decimal? max, string unit)
        {
            var u = Spaced(unit);
            if (min != null && max != null) return $"{Number(min.Value)}–{Number(max.Value)}{u}";
            if (min != null) return $"≥ {Number(min.Value)}{u}";
            if (max != null) return $"≤ {Number(max.Value)}{u}";
            return "";
        }

        private static string Spaced(string unit) => string.IsNullOrEmpty(unit) ? "" : " " + unit;

        private static string Number(decimal d) => d.ToString("0.####", CultureInfo.CurrentCulture);
    }
}
