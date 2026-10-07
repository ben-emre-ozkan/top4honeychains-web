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
        public DateOnly? HarvestedOn { get; set; }

        // D-72: plate code of the province (0 when unknown); the only place information on the public page
        public int ProvincePlate { get; set; }

        // Photos for that apiary
        public List<HoneyStoryPhoto> Photos { get; set; } = new();

        // Every CCP that applies to raw lots, for THIS raw honey production
        public List<HoneyStoryCcpBlock> CcpBlocks { get; set; } = new();

        // P5 (D-43): code of the beekeeper's published storefront, if any
        public string? StorefrontCode { get; set; }

        // The approved report of a verified laboratory on this lot (organization model), for the verification page
        public string? LabReportCode { get; set; }
        public string? LabName { get; set; }
        public bool LabAccredited { get; set; }
        public DateTime? LabApprovedAt { get; set; }
    }
}
