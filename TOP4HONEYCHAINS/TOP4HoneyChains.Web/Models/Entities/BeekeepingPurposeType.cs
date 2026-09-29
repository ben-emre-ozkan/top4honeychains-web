using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

[Index("BeekeepingPurposeTypeTitle", Name = "UQ__Beekeepi__E8252B9E466CAEF7", IsUnique = true)]
public partial class BeekeepingPurposeType
{
    [Key]
    public int BeekeepingPurposeTypeId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string BeekeepingPurposeTypeTitle { get; set; } = null!;

    [InverseProperty("BeekeeperBeekeepingPurposeType")]
    public virtual ICollection<Beekeeper> Beekeepers { get; set; } = new List<Beekeeper>();
}
