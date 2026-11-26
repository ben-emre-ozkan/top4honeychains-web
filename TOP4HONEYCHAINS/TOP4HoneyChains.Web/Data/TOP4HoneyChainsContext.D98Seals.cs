using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Data;

// D-98 (migration 036): the seal checks when a container is emptied into a batch
public partial class TOP4HoneyChainsContext
{
    public virtual DbSet<ContainerEmptying> ContainerEmptyings { get; set; }

    private static void OnModelCreatingD98Seals(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContainerEmptying>(entity =>
        {
            entity.ToTable("ContainerEmptyings", tb => tb.HasTrigger("TR_ContainerEmptyings_AppendOnly"));
            entity.Property(e => e.Outcome).IsUnicode(false);
            entity.Property(e => e.ExpectedSeal).IsUnicode(false);
            entity.Property(e => e.ScannedSeal).IsUnicode(false);
            entity.HasIndex(e => new { e.ContainerId, e.EmptyingId }, "IX_ContainerEmptyings_Container");
            entity.HasIndex(e => e.FinalHoneyProductionId, "IX_ContainerEmptyings_Batch");
            entity.HasIndex(e => e.ContainerId, "UX_ContainerEmptyings_Emptied").IsUnique().HasFilter("[Outcome] = 'Emptied'");
            entity.HasOne(e => e.Container).WithMany().HasForeignKey(e => e.ContainerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Batch).WithMany().HasForeignKey(e => e.FinalHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
