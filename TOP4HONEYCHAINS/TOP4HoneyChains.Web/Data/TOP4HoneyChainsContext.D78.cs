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

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<Consent>(entity =>
        {
            entity.Property(e => e.Method).IsUnicode(false);
            entity.HasOne<OneTimeCode>().WithMany().HasForeignKey(e => e.CodeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Notice>().WithMany().HasForeignKey(e => e.NoticeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Organization>().WithMany().HasForeignKey(e => e.RegisteringOrgId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
