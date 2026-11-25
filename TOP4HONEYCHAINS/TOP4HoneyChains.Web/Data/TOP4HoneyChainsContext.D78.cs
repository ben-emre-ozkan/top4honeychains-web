using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Data;

// D-78 P1 (docs/design/D78-SPEC.md 3, migrations 018-022): the tables added for registration, SMS, notices and accounts.
// Column types as in the scripts (D-46: the EF model describes the real schema; checked by SchemaDriftTests).
public partial class TOP4HoneyChainsContext
{
    public virtual DbSet<SmsMessage> SmsMessages { get; set; }
    public virtual DbSet<OneTimeCode> OneTimeCodes { get; set; }
    public virtual DbSet<Notice> Notices { get; set; }
    public virtual DbSet<NoticeAcceptance> NoticeAcceptances { get; set; }
    public virtual DbSet<BeekeeperAccount> BeekeeperAccounts { get; set; }
    public virtual DbSet<BeekeeperRegistration> BeekeeperRegistrations { get; set; }
    public virtual DbSet<UserCredentialIssue> UserCredentialIssues { get; set; }
    public virtual DbSet<UserSetting> UserSettings { get; set; }
    public virtual DbSet<Container> Containers { get; set; }
    public virtual DbSet<ContainerSealEvent> ContainerSealEvents { get; set; }
    public virtual DbSet<SampleSource> SampleSources { get; set; }
    public virtual DbSet<LotPrice> LotPrices { get; set; }
    public virtual DbSet<DeliveryCorrection> DeliveryCorrections { get; set; }
    public virtual DbSet<DataSubjectRequest> DataSubjectRequests { get; set; }
    public virtual DbSet<ComplianceReview> ComplianceReviews { get; set; }
    // D-87 (migration 029)
    public virtual DbSet<ConsumerAccount> ConsumerAccounts { get; set; }
    public virtual DbSet<ConsumerSavedBatch> ConsumerSavedBatches { get; set; }
    public virtual DbSet<ConsumerFeedback> ConsumerFeedback { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        OnModelCreatingD97(modelBuilder);
        modelBuilder.Entity<SmsMessage>(entity =>
        {
            entity.ToTable("SmsMessages", tb => tb.HasTrigger("TR_SmsMessages_AppendOnly"));
            entity.Property(e => e.Purpose).IsUnicode(false);
            entity.Property(e => e.SubjectType).IsUnicode(false);
            entity.Property(e => e.ToPhoneMasked).IsUnicode(false);
            entity.Property(e => e.ToPhoneHash).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.Language).IsUnicode(false);
            entity.Property(e => e.Provider).IsUnicode(false);
            entity.Property(e => e.ProviderMessageId).IsUnicode(false);
            entity.Property(e => e.Status).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.ToPhoneHash, e.CreatedAt });
            entity.HasIndex(e => new { e.SentByUserId, e.CreatedAt });
        });

        modelBuilder.Entity<OneTimeCode>(entity =>
        {
            entity.ToTable("OneTimeCodes", tb => tb.HasTrigger("TR_OneTimeCodes_AppendOnly"));
            entity.Property(e => e.Purpose).IsUnicode(false);
            entity.Property(e => e.SubjectType).IsUnicode(false);
            entity.Property(e => e.ContentSha256).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.CodeHash).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.Salt).IsFixedLength();
            entity.Property(e => e.VoidReason).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.Purpose, e.SubjectType, e.SubjectKey, e.CreatedAt });
            entity.HasOne(e => e.Sms).WithMany().HasForeignKey(e => e.SmsId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notice>(entity =>
        {
            entity.ToTable("Notices", tb => tb.HasTrigger("TR_Notices_Immutable"));
            entity.Property(e => e.Kind).IsUnicode(false);
            entity.Property(e => e.Audience).IsUnicode(false);
            entity.Property(e => e.Purpose).IsUnicode(false);
            entity.Property(e => e.Version).IsUnicode(false);
            entity.Property(e => e.Language).IsUnicode(false);
            entity.Property(e => e.BodySha256).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.PublishedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.Kind, e.Audience, e.OrgId, e.Purpose, e.Version, e.Language }).IsUnique();
            entity.HasOne(e => e.Org).WithMany().HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NoticeAcceptance>(entity =>
        {
            entity.ToTable("NoticeAcceptances", tb => tb.HasTrigger("TR_NoticeAcceptances_AppendOnly"));
            entity.Property(e => e.Method).IsUnicode(false);
            entity.Property(e => e.EvidenceSha256).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.ClientIp).IsUnicode(false);
            entity.Property(e => e.At).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.UserId, e.NoticeId });
            entity.HasIndex(e => new { e.BeekeeperId, e.NoticeId });
            entity.HasOne(e => e.Notice).WithMany().HasForeignKey(e => e.NoticeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Beekeeper>().WithMany().HasForeignKey(e => e.BeekeeperId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OneTimeCode>().WithMany().HasForeignKey(e => e.CodeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Apiary>(entity =>
        {
            entity.Property(e => e.Province).IsFixedLength();
            entity.HasOne<Province>().WithMany().HasForeignKey(e => e.Province).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<District>().WithMany().HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BeekeeperAccount>(entity =>
        {
            entity.ToTable("BeekeeperAccounts");
            entity.Property(e => e.Status).IsUnicode(false).HasDefaultValue(BeekeeperAccountStatus.Active);
            entity.Property(e => e.LinkedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.Beekeeper).WithOne().HasForeignKey<BeekeeperAccount>(e => e.BeekeeperId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BeekeeperRegistration>(entity =>
        {
            entity.ToTable("BeekeeperRegistrations", tb => tb.HasTrigger("TR_BeekeeperRegistrations_AppendOnly"));
            entity.Property(e => e.Kind).IsUnicode(false);
            entity.Property(e => e.Method).IsUnicode(false);
            entity.Property(e => e.EvidenceStatus).IsUnicode(false);
            entity.Property(e => e.PhoneE164Hash).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.EvidenceSha256).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.At).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.BeekeeperId, e.At });
            entity.HasIndex(e => new { e.RegisteringOrgId, e.EvidenceStatus });
            entity.HasOne(e => e.Beekeeper).WithMany().HasForeignKey(e => e.BeekeeperId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.RegisteringOrg).WithMany().HasForeignKey(e => e.RegisteringOrgId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OneTimeCode>().WithMany().HasForeignKey(e => e.CodeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Notice>().WithMany().HasForeignKey(e => e.AttestationNoticeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserCredentialIssue>(entity =>
        {
            entity.ToTable("UserCredentialIssues", tb => tb.HasTrigger("TR_UserCredentialIssues_AppendOnly"));
            entity.Property(e => e.Channel).IsUnicode(false);
            entity.Property(e => e.IssuedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.UserId, e.IssueId });
            entity.HasOne<SmsMessage>().WithMany().HasForeignKey(e => e.SmsId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.ToTable("UserSettings");
            entity.Property(e => e.Language).IsUnicode(false);

            entity.HasOne<OneTimeCode>().WithMany().HasForeignKey(e => e.PhoneCodeId).OnDelete(DeleteBehavior.Restrict);
        });

        // D-78 P2 (migration 023)
        modelBuilder.Entity<Container>(entity =>
        {
            entity.ToTable("Containers", tb => tb.HasTrigger("TR_Containers_Immutable"));
            entity.Property(e => e.Kind).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => e.ContainerCode).IsUnique();
            entity.HasIndex(e => new { e.RawHoneyProductionId, e.Seq }).IsUnique();
            entity.HasOne(e => e.Intake).WithMany(i => i.Containers).HasForeignKey(e => e.IntakeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<RawHoneyProduction>().WithMany().HasForeignKey(e => e.RawHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContainerSealEvent>(entity =>
        {
            entity.ToTable("ContainerSealEvents", tb => tb.HasTrigger("TR_ContainerSealEvents_AppendOnly"));
            entity.Property(e => e.Event).IsUnicode(false);
            entity.Property(e => e.PhotoSha256).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.At).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.ContainerId, e.EventId });
            entity.HasOne(e => e.Container).WithMany(c => c.SealEvents).HasForeignKey(e => e.ContainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SampleSource>(entity =>
        {
            entity.ToTable("SampleSources", tb => tb.HasTrigger("TR_SampleSources_InsertOnly"));
            entity.HasKey(e => new { e.CompositeSampleId, e.SourceSampleId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne<Sample>().WithMany().HasForeignKey(e => e.CompositeSampleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Sample>().WithMany().HasForeignKey(e => e.SourceSampleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sample>(entity =>
        {
            entity.Property(e => e.Kind).IsUnicode(false);
            entity.HasIndex(e => e.ContainerId);
            entity.HasOne(e => e.Container).WithMany(c => c.Samples).HasForeignKey(e => e.ContainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LotIntake>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("TR_LotIntakes_DeliveryLocked"));
            entity.Property(e => e.ConfirmationMethod).IsUnicode(false);
            entity.Property(e => e.EvidenceStatus).IsUnicode(false);
            entity.Property(e => e.ContentSha256).IsFixedLength().IsUnicode(false);
            entity.HasOne<OneTimeCode>().WithMany().HasForeignKey(e => e.CodeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RawHoneyProduction>(entity => entity.ToTable(tb => tb.HasTrigger("TR_RawHoneyProductions_DeliveryLocked")));

        // D-78 P3 (migrations 024, 025)
        modelBuilder.Entity<LotPrice>(entity =>
        {
            entity.ToTable("LotPrices", tb => tb.HasTrigger("TR_LotPrices_AppendOnly"));
            entity.Property(e => e.Currency).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.Basis).IsUnicode(false).HasDefaultValue(PriceBasis.Asking);
            entity.Property(e => e.ClientIp).IsUnicode(false);
            entity.HasIndex(e => new { e.RawHoneyProductionId, e.PriceId });
            entity.HasOne<RawHoneyProduction>().WithMany().HasForeignKey(e => e.RawHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeliveryCorrection>(entity =>
        {
            entity.ToTable("DeliveryCorrections", tb => tb.HasTrigger("TR_DeliveryCorrections_Guard"));
            entity.Property(e => e.OldContentSha256).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.NewContentSha256).IsFixedLength().IsUnicode(false);
            entity.Property(e => e.Status).IsUnicode(false).HasDefaultValue(CorrectionStatus.Pending);
            entity.HasIndex(e => new { e.IntakeId, e.CorrectionId });
            entity.HasOne(e => e.Intake).WithMany().HasForeignKey(e => e.IntakeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<RawHoneyProduction>().WithMany().HasForeignKey(e => e.RawHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<SmsMessage>().WithMany().HasForeignKey(e => e.InfoSmsId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DataSubjectRequest>(entity =>
        {
            entity.Property(e => e.Kind).IsUnicode(false);
            entity.Property(e => e.Status).IsUnicode(false).HasDefaultValue(DataSubjectRequestStatus.Open);
            entity.HasIndex(e => new { e.OrgId, e.Status, e.DueOn });
            entity.HasOne<Organization>().WithMany().HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Beekeeper>().WithMany().HasForeignKey(e => e.BeekeeperId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComplianceReview>(entity =>
        {
            entity.ToTable("ComplianceReviews", tb => tb.HasTrigger("TR_ComplianceReviews_AppendOnly"));
            entity.Property(e => e.SubjectType).IsUnicode(false);
            entity.Property(e => e.Outcome).IsUnicode(false);
            entity.HasIndex(e => new { e.OrgId, e.SubjectType, e.SubjectId });
            entity.HasOne<Organization>().WithMany().HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrgDocument>(entity => entity.Property(e => e.AgreementKind).IsUnicode(false));

        // D-87 (migration 029)
        modelBuilder.Entity<ConsumerAccount>(entity =>
        {
            entity.ToTable("ConsumerAccounts");
            entity.Property(e => e.Pseudonym).IsUnicode(false);
            entity.Property(e => e.Language).IsUnicode(false);
            entity.Property(e => e.NoticeVersion).IsUnicode(false);
            entity.Property(e => e.Status).IsUnicode(false).HasDefaultValue(ConsumerAccountStatus.Active);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => e.Pseudonym).IsUnique();
            entity.HasOne<Notice>().WithMany().HasForeignKey(e => e.NoticeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ConsumerSavedBatch>(entity =>
        {
            entity.ToTable("ConsumerSavedBatches");
            entity.HasKey(e => new { e.ConsumerUserId, e.FinalHoneyProductionId });
            entity.Property(e => e.SavedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne<ConsumerAccount>().WithMany().HasForeignKey(e => e.ConsumerUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Batch).WithMany().HasForeignKey(e => e.FinalHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ConsumerFeedback>(entity =>
        {
            entity.ToTable("ConsumerFeedback");
            entity.HasKey(e => e.FeedbackId);
            entity.Property(e => e.Pseudonym).IsUnicode(false);
            entity.Property(e => e.Category).IsUnicode(false);
            entity.Property(e => e.Language).IsUnicode(false);
            entity.Property(e => e.Status).IsUnicode(false).HasDefaultValue(ConsumerFeedbackStatus.New);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.OrgId, e.Status });
            entity.HasIndex(e => new { e.ConsumerUserId, e.CreatedAt });
            entity.HasOne<ConsumerAccount>().WithMany().HasForeignKey(e => e.ConsumerUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Batch).WithMany().HasForeignKey(e => e.FinalHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Org).WithMany().HasForeignKey(e => e.OrgId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Consent>(entity =>
        {
            entity.Property(e => e.Method).IsUnicode(false);
            entity.HasOne<OneTimeCode>().WithMany().HasForeignKey(e => e.CodeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Notice>().WithMany().HasForeignKey(e => e.NoticeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Organization>().WithMany().HasForeignKey(e => e.RegisteringOrgId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
