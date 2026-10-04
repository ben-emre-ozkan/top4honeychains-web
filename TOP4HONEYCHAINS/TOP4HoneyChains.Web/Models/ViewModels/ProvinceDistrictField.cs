namespace TOP4HoneyChains.Web.Models.ViewModels
{
    // D-52: model of the shared province / district dropdown partial
    public record ProvinceDistrictField(string ProvinceName, string DistrictName, string? Province, string? District,
        bool Required, string ProvinceLabel = "Province", string DistrictLabel = "District");
}
