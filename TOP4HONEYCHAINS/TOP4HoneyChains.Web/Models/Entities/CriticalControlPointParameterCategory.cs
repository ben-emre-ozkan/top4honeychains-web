using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public partial class CriticalControlPointParameterCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [StringLength(100)]
        public string? CategoryName { get; set; }

        [StringLength(150)]
        public string? CategoryDesc { get; set; }

        public virtual ICollection<CriticalControlPointParameter> CriticalControlPointParameters { get; set; }
            = new List<CriticalControlPointParameter>();
    }

}
