namespace TOP4HoneyChains.Web.Models.ViewModels.Storefront
{
    // The public beekeeper page (design: docs/design/beekeeper-page). Every value comes from verified, approved
    // data; a section whose data is missing stays empty and is hidden. Never a phone number, personal photo,
    // birth date or exact location.
    public class StorefrontPageViewModel
    {
        public string Code { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Initials { get; set; } = "";

        // The demo storefront of db/demo/create_demo.sql: sample texts and photos are shown only there
        public bool IsDemo { get; set; }

        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Region { get; set; }
        public string? Association { get; set; }
        public int? MemberSinceYear { get; set; }
        public List<string> HoneyTypes { get; set; } = new();

        public bool RegistryVerified { get; set; }
        public bool AccreditedLab { get; set; }
        public bool AllCcpsApproved { get; set; }

        public ActivityBatch? LatestBatch { get; set; }
        public ActivityReport? LatestReport { get; set; }
        public int? Ccp0PassedSeason { get; set; }
        public bool Ccp0PassedForAllLots { get; set; }

        public int VerifiedLots { get; set; }
        public int BatchCount { get; set; }
        public int LabReportCount { get; set; }
        public decimal KgTraced { get; set; }

        public int? HarvestFrom { get; set; }
        public int? HarvestTo { get; set; }

        public MapMarker? Marker { get; set; }
        public List<CalendarRow> Calendar { get; set; } = new();
        public LabSection? Lab { get; set; }
        public List<BatchRow> Batches { get; set; } = new();
        public List<Reference> References { get; set; } = new();
        public List<Photo> Photos { get; set; } = new();

        public bool HasStats => VerifiedLots > 0 || BatchCount > 0 || LabReportCount > 0 || KgTraced > 0;
        public bool HasActivity => LatestBatch != null || LatestReport != null || Ccp0PassedSeason != null;
        public bool HasAbout => IsDemo || HoneyTypes.Any() || HarvestFrom != null || Ccp0PassedForAllLots;
        public string Place => string.Join(", ", new[] { District, Province }.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    public record ActivityBatch(string Title, DateTime? Date);

    public record ActivityReport(string Code, int WithinLimit, int Checked);

    // x/y in the 640x300 map; Label is the place shown next to the marker
    public record MapMarker(double X, double Y, string Label);

    // One row of the calendar; months are 1..12. Label is a honey type title, or a resource key when LabelIsKey
    public record CalendarRow(string Label, bool LabelIsKey, IReadOnlySet<int> Flowering, IReadOnlySet<int> Harvest);

    public record LabSection(string ReportCode, string Laboratory, bool Accredited, List<LabValue> Values);

    // ValuePercent / LimitPercent / MinPercent position the bar and the limit markers (0..100)
    public record LabValue(int ParameterId, string Name, string? Unit, decimal Value, bool? InRange, decimal? Min, decimal? Max,
        double ValuePercent, double? LimitPercent, double? MinPercent);

    public record OriginShare(string Country, decimal Percentage);

    public record BatchRow(Guid Qr, string Title, DateTime? Date, string? PackedBy, List<OriginShare> Origins,
        int SharePercent, bool AllCcpsApproved, bool AccreditedLab);

    public static class ReferenceKinds
    {
        public const string Association = "Association";
        public const string Processor = "Processor";
        public const string Packer = "Packer";
        public const string ProcessorAndPacker = "ProcessorAndPacker";
        public const string Laboratory = "Laboratory";
        public const string AccreditedLaboratory = "AccreditedLaboratory";
        public const string Wholesaler = "Wholesaler";
        public const string Retailer = "Retailer";
    }

    // A recorded relationship: the association (Year = member since), a processor (Lots, Batches), a laboratory
    // (Lots = reports), a holder that received batches (Batches)
    public record Reference(string Kind, string Name, int Lots, int Batches, int? Year);

    // Photo with a caption and an alternative text (resource keys); PositionClass sets the crop (storefront.css)
    public record Photo(string Url, string CaptionKey, string AltKey, string? PositionClass, bool Wide);
}
