using System.Globalization;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services
{
    // The honey a value belongs to: its honey type and that type's class (Floral / Honeydew / Pine).
    // Unknown or mixed (batch of several types) -> nulls.
    public record HoneyProfile(int? HoneyTypeId, string? HoneyClass)
    {
        public static readonly HoneyProfile Unknown = new(null, null);
    }

    // Checks a numeric CCP value against the parameter's limits (D-18, D-21).
    // Main limit: a limit row for the honey type beats one for the honey class, which beats one for
    // every honey; a row with a satisfied condition beats one without; rows whose condition is not met
    // or cannot be checked are skipped. Without a matching row, the parameter's default min/max apply.
    // Ratio rows (value / other value) are extra checks evaluated in addition; all checks must pass.
    public static class CcpLimitEvaluator
    {
        public record Result(bool? IsInRange, string? AppliedLimit);

        // siblingValues: numeric values of the other parameters of the same lot/batch (ParameterId -> value)
        public static Result Evaluate(CriticalControlPointParameter parameter, decimal? value,
            IReadOnlyDictionary<int, decimal> siblingValues, HoneyProfile honey)
        {
            if (value == null || (parameter.DataType != null && parameter.DataType != CcpDataType.Number))
                return new Result(null, null);

            var checks = new List<(bool? ok, string label)>();

            var main = MainCheck(parameter, value.Value, siblingValues, honey);
            if (main != null)
                checks.Add(main.Value);

            foreach (var row in Matching(parameter, siblingValues, honey).Where(l => l.RatioWithParameterId != null))
            {
                var label = $"{parameter.ParameterName} / {row.RatioWithParameter?.ParameterName}: {Describe(row.MinValue, row.MaxValue, null)}{Scope(row)}";
                if (!siblingValues.TryGetValue(row.RatioWithParameterId!.Value, out var other) || other == 0)
                    checks.Add((null, label + " (not checkable)"));
                else
                    checks.Add((InRange(value.Value / other, row.MinValue, row.MaxValue), label));
            }

            if (checks.Count == 0)
                return new Result(null, null);

            // Out of range if any check fails; in range only if every check could be made and passed
            bool? inRange = checks.Any(c => c.ok == false) ? false
                : checks.All(c => c.ok == true) ? true
                : null;
            return new Result(inRange, Truncate(string.Join("; ", checks.Select(c => c.label)), 200));
        }

        private static CriticalControlPointParameterLimit? MainRow(CriticalControlPointParameter parameter,
            IReadOnlyDictionary<int, decimal> siblingValues, HoneyProfile honey) =>
            Matching(parameter, siblingValues, honey)
                .Where(l => l.RatioWithParameterId == null)
                .OrderByDescending(l => l.HoneyTypeId != null)
                .ThenByDescending(l => l.HoneyClass != null)
                .ThenByDescending(l => l.ConditionParameterId != null)
                .FirstOrDefault();

        // The min/max of the main limit that applies to the value itself (for a bar chart); null when there is
        // none or the limit applies to a sum with another parameter
        public static (decimal? Min, decimal? Max)? MainBounds(CriticalControlPointParameter parameter,
            IReadOnlyDictionary<int, decimal> siblingValues, HoneyProfile honey)
        {
            var row = MainRow(parameter, siblingValues, honey);
            if (row == null)
                return parameter.MinValue == null && parameter.MaxValue == null ? null : (parameter.MinValue, parameter.MaxValue);
            if (row.SumWithParameterId != null || (row.MinValue == null && row.MaxValue == null))
                return null;
            return (row.MinValue, row.MaxValue);
        }

        private static (bool? ok, string label)? MainCheck(CriticalControlPointParameter parameter, decimal value,
            IReadOnlyDictionary<int, decimal> siblingValues, HoneyProfile honey)
        {
            var row = MainRow(parameter, siblingValues, honey);

            if (row == null)
            {
                if (parameter.MinValue == null && parameter.MaxValue == null)
                    return null;
                return (InRange(value, parameter.MinValue, parameter.MaxValue), Describe(parameter.MinValue, parameter.MaxValue, parameter.ParameterUnit));
            }

            var label = Describe(row.MinValue, row.MaxValue, parameter.ParameterUnit);
            var checkedValue = value;
            if (row.SumWithParameterId != null)
            {
                label = $"{parameter.ParameterName} + {row.SumWithParameter?.ParameterName}: {label}";
                if (!siblingValues.TryGetValue(row.SumWithParameterId.Value, out var other))
                    return (null, label + Scope(row) + " (not checkable)");
                checkedValue += other;
            }
            label += Scope(row);
            if (row.ConditionParameterId != null)
                label += $" ({row.ConditionParameter?.ParameterName} <= {Format(row.ConditionMaxValue!.Value)})";
            return (InRange(checkedValue, row.MinValue, row.MaxValue), label);
        }

        private static IEnumerable<CriticalControlPointParameterLimit> Matching(CriticalControlPointParameter parameter,
            IReadOnlyDictionary<int, decimal> siblingValues, HoneyProfile honey) =>
            parameter.Limits
                .Where(l => l.HoneyTypeId == null || l.HoneyTypeId == honey.HoneyTypeId)
                .Where(l => l.HoneyClass == null || l.HoneyClass == honey.HoneyClass)
                .Where(l => l.ConditionParameterId == null
                            || (siblingValues.TryGetValue(l.ConditionParameterId.Value, out var c) && c <= l.ConditionMaxValue));

        private static bool InRange(decimal v, decimal? min, decimal? max) =>
            (min == null || v >= min) && (max == null || v <= max);

        private static string Scope(CriticalControlPointParameterLimit row) =>
            row.HoneyTypeId != null ? $" ({row.HoneyType?.HoneyTypeTitle ?? "honey type " + row.HoneyTypeId})"
            : row.HoneyClass != null ? $" ({row.HoneyClass})"
            : "";

        // All limits of a parameter as one line, e.g. "max 20; Pine: max 23"
        public static string? DescribeAll(CriticalControlPointParameter parameter)
        {
            var parts = new List<string>();
            if (parameter.MinValue != null || parameter.MaxValue != null)
                parts.Add(Describe(parameter.MinValue, parameter.MaxValue, parameter.ParameterUnit));
            foreach (var l in parameter.Limits.OrderBy(l => l.HoneyTypeId != null).ThenBy(l => l.HoneyClass).ThenBy(l => l.LimitId))
            {
                var text = l.RatioWithParameterId != null
                    ? $"/ {l.RatioWithParameter?.ParameterName}: {Describe(l.MinValue, l.MaxValue, null)}"
                    : Describe(l.MinValue, l.MaxValue, parameter.ParameterUnit);
                if (l.SumWithParameterId != null)
                    text = $"+ {l.SumWithParameter?.ParameterName}: {text}";
                if (l.ConditionParameterId != null)
                    text += $" if {l.ConditionParameter?.ParameterName} <= {Format(l.ConditionMaxValue!.Value)}";
                var scope = l.HoneyTypeId != null ? l.HoneyType?.HoneyTypeTitle ?? $"honey type {l.HoneyTypeId}" : l.HoneyClass;
                parts.Add(scope != null ? $"{scope}: {text}" : text);
            }
            return parts.Count == 0 ? null : string.Join("; ", parts);
        }

        public static string Describe(decimal? min, decimal? max, string? unit)
        {
            var u = string.IsNullOrWhiteSpace(unit) || unit == "None" ? "" : " " + unit;
            if (min != null && max != null) return $"{Format(min.Value)}–{Format(max.Value)}{u}";
            if (min != null) return $"min {Format(min.Value)}{u}";
            if (max != null) return $"max {Format(max.Value)}{u}";
            return "";
        }

        private static string Format(decimal d) => d.ToString("0.####", CultureInfo.InvariantCulture);

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
    }
}
