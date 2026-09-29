using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.HoneyStory
{
    public class HoneyStoryRawSource
    {
        public int RawHoneyProductionId { get; set; }
        public int? ApiaryId { get; set; }

        public string? ApiaryName { get; set; }
        public string? BeekeeperName { get; set; }
        public string? HoneyType { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? BeekeeperPhotoBase64 { get; set; }

        // Photos for that apiary
        public List<HoneyStoryPhoto> Photos { get; set; } = new();

        // CCP0-CCP3 rows for THIS raw honey production
        public List<HoneyStoryCCPRow> RawCCPs { get; set; } = new();
    }
}
