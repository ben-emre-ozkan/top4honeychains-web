using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

[Index("HoneyTypeTitle", Name = "UQ_HoneyTypes_HoneyTypeTitle", IsUnique = true)]
public partial class HoneyType
{
    [Key]
    public int HoneyTypeId { get; set; }

    // D-98 (migration 035): nvarchar(100), so a name typed by an officer keeps its Turkish letters
    [StringLength(100)]
    public string HoneyTypeTitle { get; set; } = null!;

    [StringLength(250)]
    public string? HoneyTypeDescription { get; set; }

    // Selects honey-class specific CCP limits: Floral, Honeydew or Pine (D-18)
    [StringLength(20)]
    [RegularExpression("^(Floral|Honeydew|Pine)$", ErrorMessage = "Choose a honey class.")]
    public string? HoneyClass { get; set; }

    // D-98: who added the type from a data-entry form ("Other"); NULL for the types loaded before
    public DateTime? CreatedAt { get; set; }

    [StringLength(450)]
    public string? CreatedByUserId { get; set; }

    public int? CreatedByOrgId { get; set; }

    [InverseProperty("RawHoneyProductionHoneyTypeNavigation")]
    public virtual ICollection<RawHoneyProduction> RawHoneyProductions { get; set; } = new List<RawHoneyProduction>();
}
