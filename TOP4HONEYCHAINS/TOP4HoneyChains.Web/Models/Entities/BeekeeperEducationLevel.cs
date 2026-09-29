using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

[Index("BeekeeperEducationLevelTitle", Name = "UQ__Educatio__84B18355CC86A2D3", IsUnique = true)]
public partial class BeekeeperEducationLevel
{
    [Key]
    public int BeekeeperEducationLevelId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string BeekeeperEducationLevelTitle { get; set; } = null!;

    [InverseProperty("BeekeeperEducationLevel")]
    public virtual ICollection<Beekeeper> Beekeepers { get; set; } = new List<Beekeeper>();
}
