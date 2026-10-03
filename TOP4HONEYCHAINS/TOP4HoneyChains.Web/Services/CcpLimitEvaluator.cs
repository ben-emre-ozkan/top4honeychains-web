using System.Globalization;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services
{
    // Checks a numeric CCP value against the parameter's limits (D-18).
    // Order: a limit row for the honey class (exact class beats "every class"), where a row with a
    // satisfied condition beats one without; rows whose condition is not met or cannot be checked are
    // skipped. Without a matching row, the parameter's default min/max apply.
    public static class CcpLimitEvaluator
    {
        public record Result(bool? IsInRange, string? AppliedLimit);

        // siblingValues: numeric values of the other parameters of the same lot/batch (ParameterId -> value)
        public static Result Evaluate(CriticalControlPointParameter parameter, decimal? value,
            IReadOnlyDictionary<int, decimal> siblingValues, string? honeyClass)
        {
            if (value == null || (parameter.DataType != null && parameter.DataType != CcpDataType.Number))
                return new Result(null, null);

            var row = SelectLimit(parameter, siblingValues, honeyClass);
            decimal? min, max;
            decimal checkedValue = value.Value;
            string label;

            if (row != null)
            {
                min = row.MinValue;
                max = row.MaxValue;
                label = Describe(min, max, parameter.ParameterUnit);
                if (row.SumWithParameterId != null)
                {
                    if (!siblingValues.TryGetValue(row.SumWithParameterId.Value, out var other))
                        return new Result(null, $"{label} (sum with {row.SumWithParameter?.ParameterName ?? "another value"}; not recorded)");
                    checkedValue += other;
                    label = $"{parameter.ParameterName} + {row.SumWithParameter?.ParameterName}: {label}";
                }
                if (row.HoneyClass != null)
                    label += $" ({row.HoneyClass})";
                if (row.ConditionParameterId != null)
                    label += $" ({row.ConditionParameter?.ParameterName} <= {Format(row.ConditionMaxValue!.Value)})";
            }
            else
            {
                min = parameter.MinValue;
                max = parameter.MaxValue;
                if (min == null && max == null)
                    return new Result(null, null);
                label = Describe(min, max, parameter.ParameterUnit);
            }

            var inRange = (min == null || checkedValue >= min) && (max == null || checkedValue <= max);
            return new Result(inRange, Truncate(label, 200));
        }

        private static CriticalControlPointParameterLimit? SelectLimit(CriticalControlPointParameter parameter,
            IReadOnlyDictionary<int, decimal> siblingValues, string? honeyClass)
        {
            return parameter.Limits
                .Where(l => l.HoneyClass == null || l.HoneyClass == honeyClass)
                .Where(l => l.ConditionParameterId == null
                            || (siblingValues.TryGetValue(l.ConditionParameterId.Value, out var c) && c <= l.ConditionMaxValue))
                .OrderByDescending(l => l.HoneyClass != null)
                .ThenByDescending(l => l.ConditionParameterId != null)
                .FirstOrDefault();
        }

        // All limits of a parameter as one line, e.g. "max 20; Pine: max 23"
        public static string? DescribeAll(CriticalControlPointParameter parameter)
        {
            var parts = new List<string>();
            if (parameter.MinValue != null || parameter.MaxValue != null)
                parts.Add(Describe(parameter.MinValue, parameter.MaxValue, parameter.ParameterUnit));
            foreach (var l in parameter.Limits.OrderBy(l => l.HoneyClass).ThenBy(l => l.LimitId))
            {
                var text = Describe(l.MinValue, l.MaxValue, parameter.ParameterUnit);
                if (l.SumWithParameterId != null)
                    text = $"+ {l.SumWithParameter?.ParameterName}: {text}";
                if (l.ConditionParameterId != null)
                    text += $" if {l.ConditionParameter?.ParameterName} <= {Format(l.ConditionMaxValue!.Value)}";
                parts.Add(l.HoneyClass != null ? $"{l.HoneyClass}: {text}" : text);
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
