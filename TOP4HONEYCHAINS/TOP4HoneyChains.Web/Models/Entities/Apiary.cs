using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

public partial class Apiary
{
    [Key]
    public int ApiaryId { get; set; }

    [StringLength(450)]
    public string? ApiaryBeekeeperId { get; set; }

    public Guid ApiaryQRCode { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string ApiaryTitle { get; set; } = null!;

    public short ApiaryBeehivesCount { get; set; }

    [Column(TypeName = "decimal(12, 9)")]
    public decimal ApiaryLocationLatitude { get; set; }

    [Column(TypeName = "decimal(12, 9)")]
    public decimal ApiaryLocationLongitude { get; set; }

    public bool ApiaryIsActive { get; set; } = true;

    [ForeignKey("ApiaryBeekeeperId")]
    [InverseProperty("Apiaries")]
    public virtual Beekeeper? ApiaryBeekeeper { get; set; }

    [InverseProperty("ApiaryPhotoApiary")]
    public virtual ICollection<ApiaryPhoto> ApiaryPhotos { get; set; } = new List<ApiaryPhoto>();

    [InverseProperty("RawHoneyProductionApiary")]
    public virtual ICollection<RawHoneyProduction> RawHoneyProductions { get; set; } = new List<RawHoneyProduction>();
}
