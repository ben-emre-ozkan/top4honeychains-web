using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Data;

// D-78 P1 (docs/design/D78-SPEC.md 3, migrations 018-022): the tables added for registration, SMS, notices and accounts.
// Column types as in the scripts (D-46: the EF model describes the real schema; checked by SchemaDriftTests).
public partial class TOP4HoneyChainsContext
{
    public virtual DbSet<SmsMessage> SmsMessages { get; set; }
    public virtual DbSet<OneTimeCode> OneTimeCodes { get; set; }

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
    }
}
