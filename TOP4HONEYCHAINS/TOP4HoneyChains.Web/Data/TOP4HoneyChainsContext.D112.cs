using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Data;

// D-112 (migration 037): the flag of the live site's test set
public partial class TOP4HoneyChainsContext
{
    public virtual DbSet<TestSetAccount> TestSetAccounts { get; set; }

    private static void OnModelCreatingD112(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestSetAccount>(entity =>
        {
            entity.ToTable("TestSetAccounts");
            entity.Property(e => e.Kind).IsUnicode(false);
        });
        modelBuilder.Entity<Organization>(entity => entity.HasIndex(e => e.IsTestData).HasFilter("[IsTestData] = 1"));
    }
}
