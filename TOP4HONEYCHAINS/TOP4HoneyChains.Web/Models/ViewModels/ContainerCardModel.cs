using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // D-97: one container card of the delivery form (Views/Intake/_ContainerCard.cshtml). Index is the form index, or "__i__" in
    // the template that js/intake-containers.js copies for a new container.
    public record ContainerCardModel(string Index, string Number, ContainerRow Row, string? CapType);
}
