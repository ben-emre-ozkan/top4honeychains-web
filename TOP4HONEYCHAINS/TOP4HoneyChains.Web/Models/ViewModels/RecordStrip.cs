namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // D-67: "Entered by … · Approved by …" on a CCP record, lab report or storage record, from data the record already has.
    // Names are given, or user ids that the partial resolves to user names.
    public record RecordStrip(string? EnteredBy, DateTime? EnteredAt, string? ApprovedBy, DateTime? ApprovedAt,
        bool SelfApproved = false, bool IdsNotNames = false, bool TimesAreUtc = false);
}
