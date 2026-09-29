using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TOP4HoneyChains.Web.ModelBinding
{
    // Binds decimal/double/float independently of the UI culture (ENG-07).
    // "1.5" and "1,5" both mean 1.5 in every supported language; with the default
    // culture-aware binder, "1.5" under "tr" was silently read as 15.
    public class FlexibleDecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

            var raw = valueResult.FirstValue;
            var messages = bindingContext.ModelMetadata.ModelBindingMessageProvider;

            if (string.IsNullOrWhiteSpace(raw))
            {
                if (bindingContext.ModelMetadata.IsReferenceOrNullableType)
                    bindingContext.Result = ModelBindingResult.Success(null);
                else
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, messages.ValueMustNotBeNullAccessor(raw ?? string.Empty));
                return Task.CompletedTask;
            }

            var type = Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType;
            if (TryParse(raw, type, out var value))
            {
                bindingContext.Result = ModelBindingResult.Success(value);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                    messages.AttemptedValueIsInvalidAccessor(raw, bindingContext.ModelMetadata.GetDisplayName()));
            }
            return Task.CompletedTask;
        }

        internal static bool TryParse(string raw, Type type, out object? value)
        {
            value = null;
            var normalized = Normalize(raw);
            if (normalized == null)
                return false;

            const NumberStyles styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;
            var inv = CultureInfo.InvariantCulture;

            if (type == typeof(decimal) && decimal.TryParse(normalized, styles, inv, out var d)) { value = d; return true; }
            if (type == typeof(double) && double.TryParse(normalized, styles, inv, out var db)) { value = db; return true; }
            if (type == typeof(float) && float.TryParse(normalized, styles, inv, out var f)) { value = f; return true; }
            return false;
        }

        // Turns "1,5", "1.5", "1.234,5", "1,234.5", "1 234,5" into invariant "1.5" / "1234.5".
        // The right-most '.' or ',' is the decimal separator; a single kind of separator that
        // occurs more than once is a group separator ("1.234.567").
        internal static string? Normalize(string raw)
        {
            var s = new string(raw.Where(c => !char.IsWhiteSpace(c) && c != ' ' && c != ' ' && c != '\'').ToArray());
            if (s.Length == 0)
                return null;

            var lastDot = s.LastIndexOf('.');
            var lastComma = s.LastIndexOf(',');

            if (lastDot >= 0 && lastComma >= 0)
            {
                var decimalSep = lastDot > lastComma ? '.' : ',';
                var groupSep = decimalSep == '.' ? ',' : '.';
                s = s.Replace(groupSep.ToString(), string.Empty);
                if (decimalSep == ',') s = s.Replace(',', '.');
            }
            else if (lastComma >= 0)
            {
                s = s.Count(c => c == ',') > 1 ? s.Replace(",", string.Empty) : s.Replace(',', '.');
            }
            else if (lastDot >= 0 && s.Count(c => c == '.') > 1)
            {
                s = s.Replace(".", string.Empty);
            }

            return s.Count(c => c == '.') > 1 ? null : s;
        }
    }

    public class FlexibleDecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
            return type == typeof(decimal) || type == typeof(double) || type == typeof(float)
                ? new FlexibleDecimalModelBinder()
                : null;
        }
    }
}
