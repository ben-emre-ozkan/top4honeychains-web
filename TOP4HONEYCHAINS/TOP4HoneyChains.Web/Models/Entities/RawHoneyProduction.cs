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

    public int? RawHoneyProductionApiaryId { get; set; }

    public int? RawHoneyProductionPeriodId { get; set; }

    public int? RawHoneyProductionHoneyType { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? RawHoneyProductionAmount { get; set; }

    public DateOnly? RawHoneyProductionDate { get; set; }
    
    [Column("RawHoneyProductionWholesalerId")]
    public int? RawHoneyProductionWholesalerId { get; set; }

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
