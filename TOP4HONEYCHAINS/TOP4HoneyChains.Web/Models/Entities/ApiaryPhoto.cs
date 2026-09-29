using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

public partial class ApiaryPhoto
{
    [Key]
    public int ApiaryPhotoId { get; set; }

    public int ApiaryPhotoApiaryId { get; set; }

    [Unicode(false)]
    public string? ApiaryPhotoDesc { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ApiaryPhotoCreatedDate { get; set; }

    public int? ApiaryPhotoProductionPeriodId { get; set; }

    public bool? ApiaryPhotoApproved { get; set; }

    public byte[]? ApiaryPhotoData { get; set; }

    [StringLength(255)]
    public string? ApiaryPhotoFileName { get; set; }

    [StringLength(50)]
    public string? ApiaryPhotoContentType { get; set; }

    [ForeignKey("ApiaryPhotoApiaryId")]
    [InverseProperty("ApiaryPhotos")]
    public virtual Apiary? ApiaryPhotoApiary { get; set; }

    [ForeignKey("ApiaryPhotoProductionPeriodId")]
    [InverseProperty("ApiaryPhotos")]
    public virtual RawHoneyProductionPeriod? ApiaryPhotoProductionPeriod { get; set; }
}
