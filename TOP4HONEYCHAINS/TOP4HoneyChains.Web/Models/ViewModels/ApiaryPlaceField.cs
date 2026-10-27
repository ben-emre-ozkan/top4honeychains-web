namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // D-78 (F5): the province / district / locality fields of an apiary and the title they produce (_ApiaryPlace partial).
    // The field names differ per form (admin apiary pages, beekeeper registration); LatId/LngId are the coordinate inputs
    // from which the province is proposed.
    public record ApiaryPlaceField(
        string ProvinceName, string DistrictName, string LocalityName,
        string? Province, int? DistrictId, string? Locality,
        string LatId, string LngId,
        string? CurrentTitle = null, bool Required = true);
}
