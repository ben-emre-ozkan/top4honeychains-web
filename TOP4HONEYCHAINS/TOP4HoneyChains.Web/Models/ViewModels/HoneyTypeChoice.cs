using Microsoft.AspNetCore.Mvc.Rendering;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // D-98: a honey type list of a data-entry form with "Other" (a new name and its honey class) for the roles that may add one.
    // The new name and class are posted as NewHoneyTypeName and NewHoneyTypeClass.
    public record HoneyTypeChoice(
        string Name,
        string Id,
        IEnumerable<SelectListItem> Items,
        int? Selected,
        bool MayAdd,
        string? NewName = null,
        string? NewClass = null,
        bool Placeholder = false);

    // D-98: the beekeeper's honey types on the registration form (several), with "Other" for one new type
    public record HoneyTypeChecks(
        IEnumerable<SelectListItem> Items,
        IReadOnlyCollection<int> Selected,
        bool MayAdd,
        bool Other,
        string? NewName = null,
        string? NewClass = null);
}
