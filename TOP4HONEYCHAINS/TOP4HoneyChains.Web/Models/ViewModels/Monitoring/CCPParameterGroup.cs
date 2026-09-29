namespace TOP4HoneyChains.Web.Models.ViewModels.Monitoring
{
    public class CCPParameterGroup
    {
        public string CategoryName { get; set; } = "";
        public string? CategoryDescription { get; set; }
        public List<CCPParameterInput> Parameters { get; set; } = new();
    }

}
