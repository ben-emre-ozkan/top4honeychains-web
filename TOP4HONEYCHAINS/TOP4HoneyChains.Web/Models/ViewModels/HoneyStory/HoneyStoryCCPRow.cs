namespace TOP4HoneyChains.Web.Models.ViewModels.HoneyStory
{
    public class HoneyStoryCCPRow
    {
        public int? CCPId { get; set; }
        public string? CCPCode { get; set; }
        public string? CCPTitle { get; set; }

        public int? ParameterId { get; set; }
        public string? ParameterName { get; set; }
        public string? Unit { get; set; }

        public decimal? NumericValue { get; set; }
        public string? TextValue { get; set; }

        public DateTime? MeasurementDate { get; set; }
        public string? Notes { get; set; }
    }
}
