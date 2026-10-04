using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    // D-52: the 81 provinces (plate code) and their districts, for the province / district dropdowns
    public class Province
    {
        [StringLength(2)]
        public string Code { get; set; } = null!;

        [StringLength(50)]
        public string Name { get; set; } = null!;

        [StringLength(200)]
        public string? Source { get; set; }
    }

    public class District
    {
        public int DistrictId { get; set; }

        [StringLength(2)]
        public string ProvinceCode { get; set; } = null!;

        [StringLength(60)]
        public string Name { get; set; } = null!;
    }
}
