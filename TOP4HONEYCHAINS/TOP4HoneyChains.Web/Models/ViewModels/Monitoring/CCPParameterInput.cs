namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    public class CCPParameterInput
    {
        public int ParameterId { get; set; }

        public string ParameterName { get; set; } = "";

        public string? Unit { get; set; }

        public string? CriticalLimit { get; set; }

        public string? MeasuredTextValue { get; set; }

        public decimal? MeasuredNumericValue { get; set; }

        public string? Notes { get; set; }

        // Metadata (D-18): Number / YesNo / Text / File, or null for parameters without a type
        public string? DataType { get; set; }
        public bool IsRequired { get; set; }

        // D-62: required only when another parameter of the record has this answer
        public int? RequiredIfParameterId { get; set; }
        public string? RequiredIfValue { get; set; }

        // D-67: the parameter with its limit rows, for the localised limit text (display only, never bound)
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public TOP4HoneyChains.Web.Models.Entities.CriticalControlPointParameter? Definition { get; set; }

        // All limits of the parameter as text, and whether they are verified
        public string? LimitText { get; set; }
        public bool LimitVerified { get; set; }

        // Result stored with the value
        public bool? IsInRange { get; set; }
        public string? AppliedLimit { get; set; }

        // File parameters (D-19)
        public int? LogId { get; set; }
        public bool HasStoredFile { get; set; }
        public IFormFile? File { get; set; }
        public bool RemoveFile { get; set; }

        // P4: the value comes from an approved lab report and cannot be changed here (D-42)
        public int? LabReportId { get; set; }
    }

}
