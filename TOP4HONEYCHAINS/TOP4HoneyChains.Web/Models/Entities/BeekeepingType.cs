using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

[Index("BeekeepingTypeTitle", Name = "UQ__Beekeepi__E8252B9E1104749B", IsUnique = true)]
public partial class BeekeepingType
{
    [Key]
    public int BeekeepingTypeId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string BeekeepingTypeTitle { get; set; } = null!;

    [InverseProperty("BeekeeperBeekeepingType")]
    public virtual ICollection<Beekeeper> Beekeepers { get; set; } = new List<Beekeeper>();
}
