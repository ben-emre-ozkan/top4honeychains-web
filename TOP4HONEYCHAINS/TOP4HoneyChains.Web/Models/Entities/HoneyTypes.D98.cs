using System.ComponentModel.DataAnnotations;

namespace TOP4HoneyChains.Web.Models.Entities;

// D-98 (migration 035): the honey types a beekeeper produces, recorded at registration
public class BeekeeperHoneyType
{
    [StringLength(450)]
    public string BeekeeperId { get; set; } = null!;

    public int HoneyTypeId { get; set; }

    public virtual HoneyType? HoneyType { get; set; }
}

// D-98: a honey type merged into another by the platform administrator. The source row is removed; its id and title stay here,
// so a delivery confirmed with the source's id still verifies and the old name still points to the target.
public class HoneyTypeMerge
{
    [Key]
    public int MergeId { get; set; }

    public int SourceHoneyTypeId { get; set; }

    [StringLength(100)]
    public string SourceTitle { get; set; } = null!;

    [StringLength(20)]
    public string? SourceClass { get; set; }

    public int TargetHoneyTypeId { get; set; }

    public DateTime MergedAt { get; set; }

    [StringLength(450)]
    public string MergedByUserId { get; set; } = null!;
}
