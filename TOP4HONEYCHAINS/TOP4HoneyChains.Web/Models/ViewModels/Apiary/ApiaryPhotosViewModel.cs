using Microsoft.AspNetCore.Mvc.Rendering;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Models.ViewModels.Apiary
{
    public class ApiaryPhotosViewModel
    {
        public int ApiaryId { get; set; }
        public int? ProductionPeriodId { get; set; }

        public string? Description { get; set; }
        public bool? Approved { get; set; }

        public List<ApiaryPhoto> ExistingPhotos { get; set; } = new();

        public SelectList? Apiaries { get; set; }
        public SelectList? ProductionPeriods { get; set; }
    }
}
