using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOP4HoneyChains.Web.Models.Entities
{
    public class CriticalControlPointParameterContent
    {
        [Key]
        public int ContentId { get; set; }

        [Required]
        public int ParameterId { get; set; }

        [Required]
        public string ContentType { get; set; } = null!;

        public string? WhatIsIt { get; set; }

        public string? WhyImportant { get; set; }

        public string? ExtraInfo { get; set; }

        public string? VideoPath { get; set; }

        public int SortOrder { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        // 🔗 Navigation
        [ForeignKey("ParameterId")]
        public virtual CriticalControlPointParameter? Parameter { get; set; }
    }
}
