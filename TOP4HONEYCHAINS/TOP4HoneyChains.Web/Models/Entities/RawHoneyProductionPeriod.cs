using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

public partial class RawHoneyProductionPeriod
{
    [Key]
    public int RawHoneyProductionPeriodId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? RawHoneyProductionPeriodTitle { get; set; }

    public DateOnly? RawHoneyProductionPeriodStartDate { get; set; }

    public DateOnly? RawHoneyProductionPeriodEndDate { get; set; }

    [InverseProperty("ApiaryPhotoProductionPeriod")]
    public virtual ICollection<ApiaryPhoto> ApiaryPhotos { get; set; } = new List<ApiaryPhoto>();

    [InverseProperty("RawHoneyProductionPeriod")]
    public virtual ICollection<RawHoneyProduction> RawHoneyProductions { get; set; } = new List<RawHoneyProduction>();
}
