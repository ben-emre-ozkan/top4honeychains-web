using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace TOP4HoneyChains.Web.Services
{
    // D-51: field names and validation messages follow the selected language. A property without [Display]
    // is shown with its humanized name ("HoneyTypeTitle" -> "Honey Type Title"), translated through the
    // shared resources; validation attributes without their own message get the framework's English
    // template as message, which the data-annotations localizer then translates.
    public class LocalizedMetadataProvider : IDisplayMetadataProvider, IValidationMetadataProvider
    {
        private readonly IStringLocalizer _localizer;

        public LocalizedMetadataProvider(IStringLocalizer localizer) => _localizer = localizer;

        public static string Humanize(string name)
        {
            var spaced = Regex.Replace(name, @"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
            return spaced.Length > 0 ? char.ToUpperInvariant(spaced[0]) + spaced[1..] : spaced;
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.MetadataKind != ModelMetadataKind.Property || context.Key.Name == null)
                return;
            if (context.DisplayMetadata.DisplayName != null)
                return;
            var key = Humanize(context.Key.Name);
            context.DisplayMetadata.DisplayName = () => _localizer[key];
        }

        public static string? DefaultMessage(ValidationAttribute a) => a switch
        {
            RequiredAttribute => "The {0} field is required.",
            StringLengthAttribute s when s.MinimumLength > 0 => "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.",
            StringLengthAttribute => "The field {0} must be a string with a maximum length of {1}.",
            MaxLengthAttribute => "The field {0} must be a string or array type with a maximum length of '{1}'.",
            MinLengthAttribute => "The field {0} must be a string or array type with a minimum length of '{1}'.",
            RangeAttribute => "The field {0} must be between {1} and {2}.",
            EmailAddressAttribute => "The {0} field is not a valid e-mail address.",
            PhoneAttribute => "The {0} field is not a valid phone number.",
            UrlAttribute => "The {0} field is not a valid fully-qualified http, https, or ftp URL.",
            CompareAttribute => "'{0}' and '{1}' do not match.",
            RegularExpressionAttribute => "The field {0} must match the regular expression '{1}'.",
            _ => null
        };

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            foreach (var attribute in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
            {
                if (!string.IsNullOrEmpty(attribute.ErrorMessage) || !string.IsNullOrEmpty(attribute.ErrorMessageResourceName))
                    continue;
                var message = DefaultMessage(attribute);
                if (message != null)
                    attribute.ErrorMessage = message;
            }
        }
    }

    public class ConfigureLocalizedMvc : IConfigureOptions<MvcOptions>
    {
        private readonly IStringLocalizerFactory _factory;

        public ConfigureLocalizedMvc(IStringLocalizerFactory factory) => _factory = factory;

        public void Configure(MvcOptions options)
        {
            var l = _factory.Create(typeof(SharedResource));
            options.ModelMetadataDetailsProviders.Add(new LocalizedMetadataProvider(l));

            // model-binding messages (wrong number format, missing value…)
            var m = options.ModelBindingMessageProvider;
            m.SetAttemptedValueIsInvalidAccessor((value, field) => l["The value '{0}' is not valid for {1}.", value, field]);
            m.SetMissingBindRequiredValueAccessor(field => l["A value for the '{0}' parameter or property was not provided.", field]);
            m.SetMissingKeyOrValueAccessor(() => l["A value is required."]);
            m.SetMissingRequestBodyRequiredValueAccessor(() => l["A non-empty request body is required."]);
            m.SetNonPropertyAttemptedValueIsInvalidAccessor(value => l["The value '{0}' is not valid.", value]);
            m.SetNonPropertyUnknownValueIsInvalidAccessor(() => l["The supplied value is invalid."]);
            m.SetNonPropertyValueMustBeANumberAccessor(() => l["The field must be a number."]);
            m.SetUnknownValueIsInvalidAccessor(field => l["The supplied value is invalid for {0}.", field]);
            m.SetValueIsInvalidAccessor(value => l["The value '{0}' is invalid.", value]);
            m.SetValueMustBeANumberAccessor(field => l["The field {0} must be a number.", field]);
            m.SetValueMustNotBeNullAccessor(value => l["The value '{0}' is invalid.", value]);
        }
    }
}
