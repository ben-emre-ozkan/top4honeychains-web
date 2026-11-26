using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Data;

// D-98 (migration 035): the beekeeper's honey types, honey types added from the forms and merged honey types
public partial class TOP4HoneyChainsContext
{
    public virtual DbSet<BeekeeperHoneyType> BeekeeperHoneyTypes { get; set; }
    public virtual DbSet<HoneyTypeMerge> HoneyTypeMerges { get; set; }

    private static void OnModelCreatingD98HoneyTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HoneyType>(entity =>
            entity.HasOne<Organization>().WithMany().HasForeignKey(e => e.CreatedByOrgId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_HoneyTypes_Organizations"));

        modelBuilder.Entity<BeekeeperHoneyType>(entity =>
        {
            entity.ToTable("BeekeeperHoneyTypes");
            // nonclustered: the key can exceed 900 bytes (BeekeeperId is nvarchar(450))
            entity.HasKey(e => new { e.BeekeeperId, e.HoneyTypeId }).HasName("PK_BeekeeperHoneyTypes").IsClustered(false);
            entity.HasIndex(e => e.HoneyTypeId).HasDatabaseName("IX_BeekeeperHoneyTypes_HoneyTypeId");
            entity.HasOne<Beekeeper>().WithMany().HasForeignKey(e => e.BeekeeperId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_BeekeeperHoneyTypes_Beekeepers");
            entity.HasOne(e => e.HoneyType).WithMany().HasForeignKey(e => e.HoneyTypeId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_BeekeeperHoneyTypes_HoneyTypes");
        });

        modelBuilder.Entity<HoneyTypeMerge>(entity =>
        {
            entity.ToTable("HoneyTypeMerges");
            entity.HasIndex(e => e.SourceHoneyTypeId).IsUnique().HasDatabaseName("UX_HoneyTypeMerges_Source");
            entity.HasIndex(e => e.TargetHoneyTypeId).HasDatabaseName("IX_HoneyTypeMerges_Target");
            entity.HasOne<HoneyType>().WithMany().HasForeignKey(e => e.TargetHoneyTypeId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_HoneyTypeMerges_HoneyTypes");
        });
    }
}
