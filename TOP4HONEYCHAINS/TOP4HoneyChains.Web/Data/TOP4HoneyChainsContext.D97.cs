using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Data;

// D-97 (migration 033): the seal pool and the organizations' permanent containers
public partial class TOP4HoneyChainsContext
{
    public virtual DbSet<SealBatch> SealBatches { get; set; }
    public virtual DbSet<SealCode> SealCodes { get; set; }
    public virtual DbSet<OrgContainer> OrgContainers { get; set; }

    private static void OnModelCreatingD97(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SealBatch>(entity =>
        {
            entity.ToTable("SealBatches");
            entity.Property(e => e.Kind).IsUnicode(false);
            entity.Property(e => e.FirstCode).IsUnicode(false);
            entity.Property(e => e.LastCode).IsUnicode(false);
            entity.HasOne<Organization>().WithMany().HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SealCode>(entity =>
        {
            entity.ToTable("SealCodes", tb => tb.HasTrigger("TR_SealCodes_OneWay"));
            entity.Property(e => e.Code).IsUnicode(false);
            entity.Property(e => e.Kind).IsUnicode(false);
            entity.Property(e => e.Status).IsUnicode(false).HasDefaultValue(SealStatus.Issued);
            entity.Property(e => e.UsedAs).IsUnicode(false);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.OrgId, e.Status });
            entity.HasIndex(e => e.BatchId);
            entity.HasOne(e => e.Batch).WithMany(b => b.Codes).HasForeignKey(e => e.BatchId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Organization>().WithMany().HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Container).WithMany().HasForeignKey(e => e.ContainerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Sample).WithMany().HasForeignKey(e => e.SampleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrgContainer>(entity =>
        {
            entity.ToTable("OrgContainers");
            entity.Property(e => e.Code).IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.OrgId);
            entity.HasOne(e => e.Org).WithMany().HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Container>(entity =>
        {
            entity.Property(e => e.Closure).IsUnicode(false);
            entity.HasIndex(e => e.OrgContainerId).HasFilter("[OrgContainerId] IS NOT NULL");
            entity.HasOne(e => e.OrgContainer).WithMany().HasForeignKey(e => e.OrgContainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LotIntake>(entity => entity.Property(e => e.JarCapType).IsUnicode(false));
        modelBuilder.Entity<Sample>(entity => entity.Property(e => e.CapType).IsUnicode(false));
    }
}
