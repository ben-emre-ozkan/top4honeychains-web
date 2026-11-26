using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOP4HoneyChains.Web.Models.Entities;

public partial class RawHoneyProduction
{
    [Key]
    public int RawHoneyProductionId { get; set; }

    [Required(ErrorMessage = "The apiary is required.")]
    public int? RawHoneyProductionApiaryId { get; set; }

    [Required(ErrorMessage = "The production period is required.")]
    public int? RawHoneyProductionPeriodId { get; set; }

    [Required(ErrorMessage = "The honey type is required.")]
    public int? RawHoneyProductionHoneyType { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    [Required(ErrorMessage = "The amount is required.")]
    // Limits are parsed invariantly; under the default "tr" culture "0.01" is not a valid decimal (HTTP 500)
    [Range(typeof(decimal), "0.01", "99999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "The amount must be greater than 0.")]
    public decimal? RawHoneyProductionAmount { get; set; }

    // D-98 (migration 034): association path, the Quality Assurance Officer's "ready for packing" after the approved lab report
    public DateTime? PackingReadyAt { get; set; }

    [StringLength(450)]
    public string? PackingReadyByUserId { get; set; }

    public DateOnly? RawHoneyProductionDate { get; set; }
    
    [Column("RawHoneyProductionWholesalerId")]
    [Required(ErrorMessage = "The wholesaler is required.")]
    public int? RawHoneyProductionWholesalerId { get; set; }

    // Owning organization (D-36); null = the organization created from the wholesaler
    public int? OwnerOrgId { get; set; }

    // P5 (D-31, EU 2024/1438): ISO 3166-1 alpha-2; null = TR (lots recorded before the column existed)
    [StringLength(2)]
    public string? OriginCountry { get; set; }

    [ForeignKey("RawHoneyProductionApiaryId")]
    [InverseProperty("RawHoneyProductions")]
    public virtual Apiary? RawHoneyProductionApiary { get; set; }

    [ForeignKey("RawHoneyProductionHoneyType")]
    [InverseProperty("RawHoneyProductions")]
    public virtual HoneyType? RawHoneyProductionHoneyTypeNavigation { get; set; }

    [ForeignKey("RawHoneyProductionPeriodId")]
    [InverseProperty("RawHoneyProductions")]
    public virtual RawHoneyProductionPeriod? RawHoneyProductionPeriod { get; set; }

    [ForeignKey("RawHoneyProductionWholesalerId")]
    [InverseProperty("RawHoneyProductions")]
    public virtual Wholesaler? RawHoneyProductionWholesaler { get; set; }
    public virtual ICollection<RawHoneyCCPAssignment> RawHoneyCCPAssignments { get; set; }
    = new List<RawHoneyCCPAssignment>();

    public virtual ICollection<FinalHoneyProductionDetail> FinalHoneyProductionDetails { get; set; }
    = new List<FinalHoneyProductionDetail>();
}
