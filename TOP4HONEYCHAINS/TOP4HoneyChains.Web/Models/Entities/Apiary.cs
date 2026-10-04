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

    [Required(ErrorMessage = "The title is required.")]
    [StringLength(50)]
    // D-55: nvarchar since 012 (Turkish letters)
    public string ApiaryTitle { get; set; } = null!;

    [Range(1, 10000, ErrorMessage = "The beehive count must be between 1 and 10000.")]
    public short ApiaryBeehivesCount { get; set; }

    [Column(TypeName = "decimal(12, 9)")]
    [Range(-90.0, 90.0, ErrorMessage = "The latitude must be between -90 and 90.")]
    public decimal ApiaryLocationLatitude { get; set; }

    [Column(TypeName = "decimal(12, 9)")]
    [Range(-180.0, 180.0, ErrorMessage = "The longitude must be between -180 and 180.")]
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
