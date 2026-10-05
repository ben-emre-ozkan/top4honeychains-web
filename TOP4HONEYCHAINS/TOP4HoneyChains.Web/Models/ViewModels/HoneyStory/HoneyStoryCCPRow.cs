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

        public bool Recorded { get; set; } = true;
        public bool IsFile { get; set; }

        // Compliance with verified limits only (D-18); null = no verified limit or not checkable
        public bool? Compliant { get; set; }
        public string? LimitText { get; set; }

        // Bounds of the main verified limit for a bar (null when not verified, no limit or a sum limit)
        public decimal? LimitMin { get; set; }
        public decimal? LimitMax { get; set; }
    }
}
