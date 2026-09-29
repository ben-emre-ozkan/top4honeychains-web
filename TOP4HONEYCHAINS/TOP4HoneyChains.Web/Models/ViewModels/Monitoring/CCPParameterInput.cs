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
    }

}
