using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

[Index("HoneyTypeTitle", Name = "UQ__HoneyTyp__E8252B9E79E7B6B2", IsUnique = true)]
public partial class HoneyType
{
    [Key]
    public int HoneyTypeId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string HoneyTypeTitle { get; set; } = null!;

    [StringLength(250)]
    public string? HoneyTypeDescription { get; set; }

    [InverseProperty("RawHoneyProductionHoneyTypeNavigation")]
    public virtual ICollection<RawHoneyProduction> RawHoneyProductions { get; set; } = new List<RawHoneyProduction>();
}
