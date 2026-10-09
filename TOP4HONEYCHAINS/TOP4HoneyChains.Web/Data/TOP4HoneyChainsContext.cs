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
    public virtual DbSet<CriticalControlPointParameterLimit> CriticalControlPointParameterLimits { get; set; }

    // Organization model (P1, docs/ROLE-MODEL.md)
    public virtual DbSet<Organization> Organizations { get; set; }
    public virtual DbSet<Capability> Capabilities { get; set; }
    public virtual DbSet<OrgCapability> OrgCapabilities { get; set; }
    public virtual DbSet<OrgDocument> OrgDocuments { get; set; }
    public virtual DbSet<OrgVerificationEvent> OrgVerificationEvents { get; set; }
    public virtual DbSet<OrgRoleDefinition> OrgRoleDefinitions { get; set; }
    public virtual DbSet<Membership> Memberships { get; set; }
    public virtual DbSet<BeekeeperAssociation> BeekeeperAssociations { get; set; }
    public virtual DbSet<BeekeeperRegistry> BeekeeperRegistry { get; set; }
    public virtual DbSet<RegistryVerification> RegistryVerifications { get; set; }
    public virtual DbSet<AuditEntry> AuditEntries { get; set; }
    public virtual DbSet<CcpResponsibility> CcpResponsibilities { get; set; }
    public virtual DbSet<CcpResponsibilityCapability> CcpResponsibilityCapabilities { get; set; }
    public virtual DbSet<CcpResponsibilityRole> CcpResponsibilityRoles { get; set; }
    public virtual DbSet<CcpCorrection> CcpCorrections { get; set; }
    public virtual DbSet<LotIntake> LotIntakes { get; set; }
    public virtual DbSet<Shipment> Shipments { get; set; }
    public virtual DbSet<HolderStorageRecord> HolderStorageRecords { get; set; }
    public virtual DbSet<HolderStorageValue> HolderStorageValues { get; set; }
    public virtual DbSet<Sample> Samples { get; set; }
    public virtual DbSet<SampleCustodyEvent> SampleCustodyEvents { get; set; }
    public virtual DbSet<LabReport> LabReports { get; set; }
    public virtual DbSet<LabResult> LabResults { get; set; }
    public virtual DbSet<Consent> Consents { get; set; }
    public virtual DbSet<BeekeeperStorefront> BeekeeperStorefronts { get; set; }
    public virtual DbSet<BatchOriginShare> BatchOriginShares { get; set; }
    public virtual DbSet<ReferenceTranslation> ReferenceTranslations { get; set; }
    public virtual DbSet<Province> Provinces { get; set; }
    public virtual DbSet<RolePermission> RolePermissions { get; set; }
    public virtual DbSet<District> Districts { get; set; }
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
            entity.Property(e => e.OriginCountry).IsFixedLength().IsUnicode(false).HasDefaultValue("TR");

            entity.HasOne(d => d.RawHoneyProductionApiary).WithMany(p => p.RawHoneyProductions).HasConstraintName("FK_HoneyProductions_Apiaries");

            entity.HasOne(d => d.RawHoneyProductionHoneyTypeNavigation).WithMany(p => p.RawHoneyProductions).HasConstraintName("FK_HoneyProductions_HoneyTypes");

            entity.HasOne(d => d.RawHoneyProductionPeriod).WithMany(p => p.RawHoneyProductions).HasConstraintName("FK_HoneyProductions_ProductionPeriods");
           
            // D-75: the column allows NULL in the database (a delivery to a plant not linked to a wholesaler record); [Required] on the
            // property is for the admin form only, so EF must not treat the column as NOT NULL (it would drop "IS NULL" filters)
            entity.Property(e => e.RawHoneyProductionWholesalerId).IsRequired(false);
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

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organizations");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.LegacyWholesaler).WithMany().HasForeignKey(e => e.LegacyWholesalerId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Capability>().ToTable("Capabilities");
        modelBuilder.Entity<OrgRoleDefinition>().ToTable("OrgRoles");
        modelBuilder.Entity<OrgCapability>(entity =>
        {
            entity.ToTable("OrgCapabilities");
            entity.HasKey(e => new { e.OrgId, e.Capability });
            entity.Property(e => e.AddedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Org).WithMany(o => o.Capabilities).HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<OrgDocument>(entity =>
        {
            entity.ToTable("OrgDocuments");
            entity.Property(e => e.Sha256).IsFixedLength();
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Org).WithMany(o => o.Documents).HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<OrgVerificationEvent>(entity =>
        {
            entity.ToTable("OrgVerificationEvents");
            entity.Property(e => e.At).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Org).WithMany(o => o.VerificationEvents).HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.ToTable("Memberships");
            entity.Property(e => e.GrantedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Org).WithMany(o => o.Memberships).HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<BeekeeperAssociation>(entity =>
        {
            entity.ToTable("BeekeeperAssociations");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Beekeeper).WithMany().HasForeignKey(e => e.BeekeeperId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssociationOrg).WithMany().HasForeignKey(e => e.AssociationOrgId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<BeekeeperRegistry>(entity =>
        {
            entity.ToTable("BeekeeperRegistry");
            entity.HasOne(e => e.Beekeeper).WithOne().HasForeignKey<BeekeeperRegistry>(e => e.BeekeeperId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<RegistryVerification>(entity =>
        {
            entity.ToTable("RegistryVerifications");
            entity.Property(e => e.Sha256).IsFixedLength();
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Beekeeper).WithMany().HasForeignKey(e => e.BeekeeperId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CcpResponsibility>(entity =>
        {
            entity.ToTable("CcpResponsibilities");
            entity.Property(e => e.CcpId).ValueGeneratedNever();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Ccp).WithMany().HasForeignKey(e => e.CcpId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Capabilities).WithOne().HasForeignKey(c => c.CcpId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Roles).WithOne().HasForeignKey(c => c.CcpId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CcpResponsibilityCapability>(entity =>
        {
            entity.ToTable("CcpResponsibilityCapabilities");
            entity.HasKey(e => new { e.CcpId, e.Capability });
        });
        modelBuilder.Entity<CcpResponsibilityRole>(entity =>
        {
            entity.ToTable("CcpResponsibilityRoles");
            entity.HasKey(e => new { e.CcpId, e.Action, e.OrgRole });
        });
        modelBuilder.Entity<CcpCorrection>(entity =>
        {
            entity.ToTable("CcpCorrections");
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Ccp).WithMany().HasForeignKey(e => e.CcpId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Parameter).WithMany().HasForeignKey(e => e.ParameterId).OnDelete(DeleteBehavior.Restrict);
        });

        // D-46: column types of the real database that the conventions would map differently
        modelBuilder.Entity<OrgDocument>().Property(e => e.Sha256).IsFixedLength().IsUnicode(false);
        modelBuilder.Entity<RegistryVerification>().Property(e => e.Sha256).IsFixedLength().IsUnicode(false);
        modelBuilder.Entity<CriticalControlPointParameterContent>().Property(e => e.ContentType).HasMaxLength(50);
        modelBuilder.Entity<CriticalControlPointParameterContent>().Property(e => e.VideoPath).HasMaxLength(500);
        modelBuilder.Entity<Beekeeper>().Property(e => e.DeactivatedAt).HasColumnType("datetime");
        modelBuilder.Entity<CriticalControlPointMonitoringLog>().Property(e => e.MeasurementDate).HasColumnType("datetime");
        modelBuilder.Entity<FinalHoneyProduction>(entity =>
        {
            entity.Property(e => e.FinalHoneyProductionFinalizedDate).HasColumnType("datetime");
            entity.Property(e => e.FinalizeRequestedAt).HasColumnType("datetime");
        });
        modelBuilder.Entity<RawHoneyCCPAssignment>(entity =>
        {
            entity.Property(e => e.RawHoneyCCPAssignmentStartDate).HasColumnType("datetime");
            entity.Property(e => e.RawHoneyCCPAssignmentEndDate).HasColumnType("datetime");
            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.NotApplicableAt).HasColumnType("datetime");
        });
        modelBuilder.Entity<FinalHoneyCCPAssignment>(entity =>
        {
            entity.Property(e => e.FinalHoneyCCPAssignmentStartDate).HasColumnType("datetime");
            entity.Property(e => e.FinalHoneyCCPAssignmentEndDate).HasColumnType("datetime");
            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.NotApplicableAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<LotIntake>(entity =>
        {
            entity.ToTable("LotIntakes");
            entity.Property(e => e.ReceivedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.FormSha256).IsFixedLength().IsUnicode(false);
            entity.HasIndex(e => e.RawHoneyProductionId).IsUnique();
            entity.HasOne(e => e.RawHoneyProduction).WithMany().HasForeignKey(e => e.RawHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ReceivingOrg).WithMany().HasForeignKey(e => e.ReceivingOrgId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("Shipments");
            entity.Property(e => e.Status).HasDefaultValue(ShipmentStatus.Pending);
            entity.Property(e => e.ShippedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Batch).WithMany().HasForeignKey(e => e.FinalHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.FromOrg).WithMany().HasForeignKey(e => e.FromOrgId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ToOrg).WithMany().HasForeignKey(e => e.ToOrgId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HolderStorageRecord>(entity =>
        {
            entity.ToTable("HolderStorageRecords");
            entity.HasIndex(e => new { e.FinalHoneyProductionId, e.HolderOrgId, e.CcpId }).IsUnique();
            entity.HasOne(e => e.Batch).WithMany().HasForeignKey(e => e.FinalHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.HolderOrg).WithMany().HasForeignKey(e => e.HolderOrgId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Ccp).WithMany().HasForeignKey(e => e.CcpId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HolderStorageValue>(entity =>
        {
            entity.ToTable("HolderStorageValues");
            entity.Property(e => e.MeasuredAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.RecordId, e.ParameterId }).IsUnique();
            entity.HasOne(e => e.Record).WithMany(r => r.Values).HasForeignKey(e => e.RecordId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Parameter).WithMany().HasForeignKey(e => e.ParameterId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sample>(entity =>
        {
            entity.ToTable("Samples");
            entity.Property(e => e.Status).HasDefaultValue(SampleStatus.Sent);
            entity.Property(e => e.TakenAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => e.SampleCode).IsUnique();
            entity.HasOne(e => e.RawHoneyProduction).WithMany().HasForeignKey(e => e.RawHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.TakingOrg).WithMany().HasForeignKey(e => e.TakingOrgId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.LabOrg).WithMany().HasForeignKey(e => e.LabOrgId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Report).WithOne(r => r.Sample).HasForeignKey<LabReport>(r => r.SampleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SampleCustodyEvent>(entity =>
        {
            entity.ToTable("SampleCustodyEvents");
            entity.Property(e => e.At).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Sample).WithMany(s => s.Events).HasForeignKey(e => e.SampleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LabReport>(entity =>
        {
            entity.ToTable("LabReports");
            entity.Property(e => e.Status).HasDefaultValue(LabReportStatus.Draft);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.Sha256).IsFixedLength().IsUnicode(false);
            entity.HasIndex(e => e.ReportCode).IsUnique();
            entity.HasOne(e => e.LabOrg).WithMany().HasForeignKey(e => e.LabOrgId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LabResult>(entity =>
        {
            entity.ToTable("LabResults");
            entity.HasIndex(e => new { e.ReportId, e.ParameterId }).IsUnique();
            entity.HasOne(e => e.Report).WithMany(r => r.Results).HasForeignKey(e => e.ReportId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Parameter).WithMany().HasForeignKey(e => e.ParameterId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Consent>(entity =>
        {
            entity.ToTable("Consents");
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.TextSha256).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.DocumentSha256).IsFixedLength().IsUnicode(false);
            entity.HasOne(e => e.Beekeeper).WithMany().HasForeignKey(e => e.BeekeeperId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssociationOrg).WithMany().HasForeignKey(e => e.AssociationOrgId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BeekeeperStorefront>(entity =>
        {
            entity.ToTable("BeekeeperStorefronts");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => new { e.Area, e.AreaKey, e.Action, e.OrgRole });
            entity.Property(e => e.Area).IsUnicode(false);
            entity.Property(e => e.AreaKey).IsUnicode(false).HasDefaultValue("");
            entity.Property(e => e.Action).IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.ToTable("Provinces");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).IsFixedLength().IsUnicode(false);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("Districts");
            entity.Property(e => e.ProvinceCode).IsFixedLength().IsUnicode(false);
            entity.HasIndex(e => new { e.ProvinceCode, e.Name }).IsUnique();
            entity.HasOne<Province>().WithMany().HasForeignKey(e => e.ProvinceCode).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ReferenceTranslation>(entity =>
        {
            entity.ToTable("ReferenceTranslations");
            entity.HasKey(e => new { e.Entity, e.EntityKey, e.Field, e.Culture });
            entity.Property(e => e.Entity).IsUnicode(false);
            entity.Property(e => e.Field).IsUnicode(false);
            entity.Property(e => e.Culture).IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<BatchOriginShare>(entity =>
        {
            entity.ToTable("BatchOriginShares");
            entity.HasKey(e => new { e.FinalHoneyProductionId, e.CountryCode });
            entity.Property(e => e.CountryCode).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.ComputedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.ToTable("AuditEntries", tb => tb.HasTrigger("TR_AuditEntries_AppendOnly"));
            // column types as in 004_org_model.sql (D-46: the EF model describes the real schema)
            entity.Property(e => e.PrevHash).HasMaxLength(64).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.Hash).HasMaxLength(64).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.Action).HasMaxLength(30);
            entity.Property(e => e.Entity).HasMaxLength(100);
            entity.Property(e => e.EntityKey).HasMaxLength(200);
            entity.Property(e => e.Flags).HasMaxLength(200);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.ClientIp).HasMaxLength(64);
        });

        modelBuilder.Entity<CriticalControlPointParameterLimit>(entity =>
        {
            entity.HasOne(e => e.Parameter)
                .WithMany(p => p.Limits)
                .HasForeignKey(e => e.ParameterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ConditionParameter)
                .WithMany()
                .HasForeignKey(e => e.ConditionParameterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SumWithParameter)
                .WithMany()
                .HasForeignKey(e => e.SumWithParameterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RatioWithParameter)
                .WithMany()
                .HasForeignKey(e => e.RatioWithParameterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.HoneyType)
                .WithMany()
                .HasForeignKey(e => e.HoneyTypeId)
                .OnDelete(DeleteBehavior.Restrict);
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
