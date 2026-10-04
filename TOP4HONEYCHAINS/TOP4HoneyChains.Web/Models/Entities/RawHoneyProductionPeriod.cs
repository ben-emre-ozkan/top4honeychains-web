using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

public partial class RawHoneyProductionPeriod : IValidatableObject
{
    [Key]
    public int RawHoneyProductionPeriodId { get; set; }

    [Required(ErrorMessage = "The title is required.")]
    [StringLength(50)]
    // D-55: nvarchar since 012 (Turkish letters)
    public string? RawHoneyProductionPeriodTitle { get; set; }

    [Required(ErrorMessage = "The start date is required.")]
    public DateOnly? RawHoneyProductionPeriodStartDate { get; set; }

    [Required(ErrorMessage = "The end date is required.")]
    public DateOnly? RawHoneyProductionPeriodEndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RawHoneyProductionPeriodStartDate.HasValue && RawHoneyProductionPeriodEndDate.HasValue
            && RawHoneyProductionPeriodEndDate <= RawHoneyProductionPeriodStartDate)
        {
            yield return new ValidationResult("The end date must be after the start date.",
                new[] { nameof(RawHoneyProductionPeriodEndDate) });
        }
    }

    [InverseProperty("ApiaryPhotoProductionPeriod")]
    public virtual ICollection<ApiaryPhoto> ApiaryPhotos { get; set; } = new List<ApiaryPhoto>();

    [InverseProperty("RawHoneyProductionPeriod")]
    public virtual ICollection<RawHoneyProduction> RawHoneyProductions { get; set; } = new List<RawHoneyProduction>();
}
