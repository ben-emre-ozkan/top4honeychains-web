using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

public partial class Beekeeper
{
    [Key]
    public string? BeekeeperId { get; set; } = null!;

    public byte[]? BeekeeperProfilePhoto { get; set; }

    [NotMapped]
    public IFormFile? ProfilePhotoFile { get; set; }

    [StringLength(50)]
    // D-55: nvarchar since 012 (Turkish letters)
    public string BeekeeperFirstName { get; set; } = null!;

    [StringLength(50)]
    // D-55: nvarchar since 012 (Turkish letters)
    public string BeekeeperLastName { get; set; } = null!;

    public DateOnly BeekeeperBirthDate { get; set; }

    public short? BeekeeperExperienceTime { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BeekeeperPhoneNumber { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? BeekeeperBusinessNumber { get; set; }

    public int? BeekeeperEducationLevelId { get; set; }

    public int? BeekeeperBeekeepingPurposeTypeId { get; set; }

    public int? BeekeeperBeekeepingTypeId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? DeactivatedAt { get; set; }

    // D-78 (021): language of the SMS texts sent to the beekeeper; null = Turkish
    [StringLength(5)]
    [Unicode(false)]
    public string? PreferredLanguage { get; set; }


    [InverseProperty("ApiaryBeekeeper")]
    public virtual ICollection<Apiary> Apiaries { get; set; } = new List<Apiary>();

    [ForeignKey("BeekeeperBeekeepingPurposeTypeId")]
    [InverseProperty("Beekeepers")]
    public virtual BeekeepingPurposeType? BeekeeperBeekeepingPurposeType { get; set; }

    [ForeignKey("BeekeeperBeekeepingTypeId")]
    [InverseProperty("Beekeepers")]
    public virtual BeekeepingType? BeekeeperBeekeepingType { get; set; }

    [ForeignKey("BeekeeperEducationLevelId")]
    [InverseProperty("Beekeepers")]
    public virtual BeekeeperEducationLevel? BeekeeperEducationLevel { get; set; }
}
