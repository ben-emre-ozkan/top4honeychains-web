using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Data;

public partial class TOP4HoneyChainsContext : DbContext
{
    public TOP4HoneyChainsContext()
    {
    }

    public TOP4HoneyChainsContext(DbContextOptions<TOP4HoneyChainsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Apiary> Apiaries { get; set; }
    public virtual DbSet<ApiaryPhoto> ApiaryPhotos { get; set; }
    public virtual DbSet<Beekeeper> Beekeepers { get; set; }
    public virtual DbSet<BeekeeperEducationLevel> BeekeeperEducationLevels { get; set; }
    public virtual DbSet<BeekeepingPurposeType> BeekeepingPurposeTypes { get; set; }
    public virtual DbSet<BeekeepingType> BeekeepingTypes { get; set; }
    public virtual DbSet<HoneyType> HoneyTypes { get; set; }
    public virtual DbSet<RawHoneyProduction> RawHoneyProductions { get; set; }
    public virtual DbSet<RawHoneyProductionPeriod> RawHoneyProductionPeriods { get; set; }
    public virtual DbSet<FinalHoneyProduction> FinalHoneyProductions { get; set; }
    public virtual DbSet<FinalHoneyProductionDetail> FinalHoneyProductionDetails { get; set; }
    public virtual DbSet<CriticalControlPoint> CriticalControlPoints { get; set; }
    public virtual DbSet<CriticalControlPointParameter> CriticalControlPointParameters { get; set; }
    public virtual DbSet<CriticalControlPointParameterContent> CriticalControlPointParameterContents { get; set; }
    public virtual DbSet<CriticalControlPointParameterCategory> CriticalControlPointParameterCategories { get; set; }
    public virtual DbSet<CriticalControlPointMonitoringLog> CriticalControlPointMonitoringLogs { get; set; }
    public virtual DbSet<AssignmentStatus> AssignmentStatuses { get; set; }
    public virtual DbSet<RawHoneyCCPAssignment> RawHoneyCCPAssignments { get; set; }
    public virtual DbSet<FinalHoneyCCPAssignment> FinalHoneyCCPAssignments { get; set; }
    public virtual DbSet<Wholesaler> Wholesalers { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Apiary>(entity =>
        {
            entity.HasKey(e => e.ApiaryId).HasName("PK__Apiaries__7AF2440E983FF371");

            entity.HasOne(d => d.ApiaryBeekeeper).WithMany(p => p.Apiaries).HasConstraintName("FK_Apiaries_Beekeepers");
        });

        modelBuilder.Entity<ApiaryPhoto>(entity =>
        {
            entity.HasKey(e => e.ApiaryPhotoId).HasName("PK__ApiaryPh__21B7B5E24F96C3CE");

            entity.HasOne(d => d.ApiaryPhotoApiary).WithMany(p => p.ApiaryPhotos).HasConstraintName("FK_ApiaryPhotos_Apiaries");

            entity.HasOne(d => d.ApiaryPhotoProductionPeriod).WithMany(p => p.ApiaryPhotos).HasConstraintName("FK_ApiaryPhotos_ProductionPeriods");
        });

        modelBuilder.Entity<Beekeeper>(entity =>
        {
            entity.HasKey(e => e.BeekeeperId).HasName("PK__Beekeepe__BA84374CB23C3AD9");

            entity.Property(e => e.BeekeeperId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BeekeeperExperienceTime).HasDefaultValue((short)0);

            entity.HasOne(d => d.BeekeeperBeekeepingPurposeType).WithMany(p => p.Beekeepers).HasConstraintName("FK_Beekeepers_BeekeepingPurposeTypes");

            entity.HasOne(d => d.BeekeeperBeekeepingType).WithMany(p => p.Beekeepers).HasConstraintName("FK_Beekeepers_BeekeepingTypes");

            entity.HasOne(d => d.BeekeeperEducationLevel).WithMany(p => p.Beekeepers).HasConstraintName("FK_Beekeepers_BeekeeperEducationLevels");
        });

        modelBuilder.Entity<BeekeeperEducationLevel>(entity =>
        {
            entity.HasKey(e => e.BeekeeperEducationLevelId).HasName("PK__Educatio__09F03C266C53B106");
        });

        modelBuilder.Entity<BeekeepingPurposeType>(entity =>
        {
            entity.HasKey(e => e.BeekeepingPurposeTypeId).HasName("PK__Beekeepi__516F03B529E304FF");
        });

        modelBuilder.Entity<BeekeepingType>(entity =>
        {
            entity.HasKey(e => e.BeekeepingTypeId).HasName("PK__Beekeepi__516F03B50C8DE081");
        });

        modelBuilder.Entity<HoneyType>(entity =>
        {
            entity.HasKey(e => e.HoneyTypeId).HasName("PK__HoneyTyp__516F03B59C57B810");
        });

        modelBuilder.Entity<RawHoneyProduction>(entity =>
        {
            entity.HasKey(e => e.RawHoneyProductionId).HasName("PK__ApiaryHo__D5D9A2D54CD1EFEC");

            entity.Property(e => e.RawHoneyProductionDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.RawHoneyProductionApiary).WithMany(p => p.RawHoneyProductions).HasConstraintName("FK_HoneyProductions_Apiaries");

            entity.HasOne(d => d.RawHoneyProductionHoneyTypeNavigation).WithMany(p => p.RawHoneyProductions).HasConstraintName("FK_HoneyProductions_HoneyTypes");

            entity.HasOne(d => d.RawHoneyProductionPeriod).WithMany(p => p.RawHoneyProductions).HasConstraintName("FK_HoneyProductions_ProductionPeriods");
           
            entity.HasOne(d => d.RawHoneyProductionWholesaler)
            .WithMany(p => p.RawHoneyProductions)
            .HasForeignKey(d => d.RawHoneyProductionWholesalerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_RawHoneyProductions_Wholesalers");

        });

        modelBuilder.Entity<RawHoneyProductionPeriod>(entity =>
        {
            entity.HasKey(e => e.RawHoneyProductionPeriodId).HasName("PK__Producti__EB4172B14E971992");

            entity.Property(e => e.RawHoneyProductionPeriodEndDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.RawHoneyProductionPeriodStartDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<FinalHoneyProduction>(entity =>
        {
            entity.HasKey(e => e.FinalHoneyProductionId);

            entity.Property(e => e.FinalHoneyProductionTitle)
                .HasMaxLength(150);

            entity.Property(e => e.FinalHoneyProductionQR)
                .IsRequired();

            entity.Property(e => e.FinalHoneyProductionDate)
                .HasColumnType("date");

            entity.Property(e => e.FinalHoneyProductionTotalAmount)
                .HasColumnType("decimal(10,2)");

            // 🔥 En kritik yeni ilişki: Wholesaler
            entity.HasOne(d => d.FinalHoneyProductionWholesaler)
                .WithMany(p => p.FinalHoneyProductions)
                .HasForeignKey(d => d.FinalHoneyProductionWholesalerId)
                .OnDelete(DeleteBehavior.Restrict) // audit sistemi için doğru seçim
                .HasConstraintName("FK_FinalHoneyProductions_Wholesalers");
        });

        modelBuilder.Entity<FinalHoneyProductionDetail>(entity =>
        {
            entity.HasKey(e => e.FinalHoneyProductionDetailId);

            entity.Property(e => e.UsedAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            // 🔥 UNIQUE (Batch + Raw) = duplicate engelleme
            entity.HasIndex(e => new { e.FinalHoneyProductionId, e.RawHoneyProductionId })
                .IsUnique()
                .HasDatabaseName("UQ_Batch_Raw");

            // 🔗 FinalHoneyProduction ilişkisi (CASCADE SQL ile uyumlu)
            entity.HasOne(d => d.FinalHoneyProduction)
                .WithMany(p => p.FinalHoneyProductionDetails)
                .HasForeignKey(d => d.FinalHoneyProductionId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔗 RawHoneyProduction ilişkisi
            entity.HasOne(d => d.RawHoneyProduction)
                .WithMany(p => p.FinalHoneyProductionDetails)
                .HasForeignKey(d => d.RawHoneyProductionId)
                .OnDelete(DeleteBehavior.Restrict); // çok önemli (traceability)

            entity.ToTable(tb => tb.HasTrigger("TR_Check_WholesalerConsistency"));
        });

        modelBuilder.Entity<CriticalControlPointParameter>(entity =>
        {
            entity.HasOne(d => d.CCP)
                .WithMany(p => p.CriticalControlPointParameters)
                .HasForeignKey(d => d.CCPId);

            entity.HasOne(d => d.ParameterCategoryNavigation)
                .WithMany(p => p.CriticalControlPointParameters)
                .HasForeignKey(d => d.ParameterCategory);
        });

        modelBuilder.Entity<CriticalControlPointParameterContent>(entity =>
        {
            entity.HasKey(e => e.ContentId);

            entity.HasOne(e => e.Parameter)
                .WithMany(p => p.CriticalControlPointParameterContents)
                .HasForeignKey(e => e.ParameterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CriticalControlPointMonitoringLog>(entity =>
        {
            entity.HasOne(d => d.CCP)
                .WithMany(p => p.CriticalControlPointMonitoringLogs)
                .HasForeignKey(d => d.CCPId);

            entity.HasOne(d => d.Parameter)
                .WithMany(p => p.CriticalControlPointMonitoringLogs)
                .HasForeignKey(d => d.ParameterId);

            entity.HasOne(d => d.RawHoneyProduction)
                .WithMany()
                .HasForeignKey(d => d.RawHoneyProductionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.FinalHoneyProduction)
                .WithMany(p => p.CriticalControlPointMonitoringLogs)
                .HasForeignKey(d => d.FinalHoneyProductionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RawHoneyCCPAssignment>(entity =>
        {
            entity.HasKey(e => e.RawHoneyCCPAssignmentId);

            entity.HasOne(d => d.CCP)
                .WithMany()
                .HasForeignKey(d => d.RawHoneyCCPAssignmentCCP);

            entity.HasOne(d => d.RawHoneyProduction)
                .WithMany(p => p.RawHoneyCCPAssignments)
                .HasForeignKey(d => d.RawHoneyProductionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.AssignmentStatus)
                .WithMany(p => p.RawHoneyCCPAssignments)
                .HasForeignKey(d => d.RawHoneyCCPAssignmentStatus);
        });


        modelBuilder.Entity<FinalHoneyCCPAssignment>()
        .HasIndex(x => new { x.FinalHoneyProductionId, x.FinalHoneyCCPAssignmentCCP })
        .IsUnique();

        modelBuilder.Entity<FinalHoneyCCPAssignment>()
            .HasOne(x => x.FinalHoneyProduction)
            .WithMany(x => x.FinalHoneyCCPAssignments)
            .HasForeignKey(x => x.FinalHoneyProductionId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<Wholesaler>(entity =>
        {
            entity.HasKey(e => e.WholesalerId)
                .HasName("PK_Wholesalers");

            // Matches the DB column: datetime DEFAULT getdate()
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
