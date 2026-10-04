using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TOP4HoneyChains.Web.Services
{
    // D-53: numbers typed with a comma or a dot as decimal separator, in every language. "1,5" and "1.5" are 1.5,
    // never 15. When both appear, the last one is the decimal separator and the other groups thousands
    // ("1.234,5" and "1,234.5" are 1234.5). A single separator is always the decimal separator ("1.234" is 1.234);
    // the same separator repeated groups thousands ("1.234.567").
    public static class DecimalInput
    {
        public static bool TryParse(string? text, out decimal value)
        {
            value = 0;
            if (text == null)
                return false;
            var s = new string(text.Where(c => !char.IsWhiteSpace(c) && c != ' ' && c != ' ' && c != '\'').ToArray());
            if (s.Length == 0)
                return false;
            var lastDot = s.LastIndexOf('.');
            var lastComma = s.LastIndexOf(',');
            string normalized;
            if (lastDot >= 0 && lastComma >= 0)
            {
                var decimalSep = lastDot > lastComma ? '.' : ',';
                var groupSep = decimalSep == '.' ? ',' : '.';
                var at = s.LastIndexOf(decimalSep);
                var whole = s[..at];
                if (s.IndexOf(decimalSep) != at || !Grouped(whole, groupSep))
                    return false;
                normalized = whole.Replace(groupSep.ToString(), "") + "." + s[(at + 1)..];
            }
            else if (lastDot >= 0 || lastComma >= 0)
            {
                var sep = lastDot >= 0 ? '.' : ',';
                var count = s.Count(c => c == sep);
                if (count == 1)
                    normalized = s.Replace(sep, '.');
                else if (Grouped(s, sep))
                    normalized = s.Replace(sep.ToString(), "");
                else
                    return false;
            }
            else
                normalized = s;
            return decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
        }

        public static decimal? Parse(string? text) => TryParse(text, out var v) ? v : null;

        // "1.234.567" / "-12,345": the first group has 1-3 digits, every following group exactly 3
        private static bool Grouped(string s, char sep)
        {
            var body = s.StartsWith('-') || s.StartsWith('+') ? s[1..] : s;
            var parts = body.Split(sep);
            if (parts.Length == 1)
                return parts[0].Length > 0 && parts[0].All(char.IsDigit);
            return parts[0].Length is >= 1 and <= 3 && parts.All(p => p.All(char.IsDigit)) && parts.Skip(1).All(p => p.Length == 3);
        }
    }

    // Binds decimal, double and float (and their nullable forms) through DecimalInput, so every numeric field accepts
    // both separators whatever the selected language
    public class DecimalInputModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext context)
        {
            var result = context.ValueProvider.GetValue(context.ModelName);
            if (result == ValueProviderResult.None)
                return Task.CompletedTask;
            context.ModelState.SetModelValue(context.ModelName, result);
            var text = result.FirstValue;
            var type = Nullable.GetUnderlyingType(context.ModelType) ?? context.ModelType;
            if (string.IsNullOrWhiteSpace(text))
            {
                if (Nullable.GetUnderlyingType(context.ModelType) != null)
                    context.Result = ModelBindingResult.Success(null);
                else
                    context.ModelState.TryAddModelError(context.ModelName, context.ModelMetadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(text ?? ""));
                return Task.CompletedTask;
            }
            if (!DecimalInput.TryParse(text, out var d))
            {
                context.ModelState.TryAddModelError(context.ModelName,
                    context.ModelMetadata.ModelBindingMessageProvider.AttemptedValueIsInvalidAccessor(text, context.ModelMetadata.GetDisplayName()));
                return Task.CompletedTask;
            }
            object value = type == typeof(decimal) ? d : type == typeof(double) ? (double)d : (float)d;
            context.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }
    }

    public class DecimalInputModelBinderProvider : IModelBinderProvider
    {
        private static readonly Type[] Types = { typeof(decimal), typeof(double), typeof(float) };

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
            return Types.Contains(type) ? new DecimalInputModelBinder() : null;
        }
    }
}
