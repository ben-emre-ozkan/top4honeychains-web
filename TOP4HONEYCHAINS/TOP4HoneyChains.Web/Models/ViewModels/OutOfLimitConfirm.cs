namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // D-64: approving a record with values outside their limits needs an explicit confirmation listing them.
    // Approval confirms that the values are recorded correctly, not that the honey complies.
    public record OutOfLimitItem(int? ParameterId, string? Name, decimal? Number, string? Text, string? Unit, string? AppliedLimit, bool LimitVerified);

    // FormId / ButtonId: the approval form and its Approve button; the form holds a hidden "confirmOutOfLimit" field
    public record OutOfLimitConfirm(string FormId, string ButtonId, List<OutOfLimitItem> Items);
}
