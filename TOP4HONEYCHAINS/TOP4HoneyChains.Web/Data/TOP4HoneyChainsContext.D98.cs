using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Data;

// D-98 (migration 034): the path after the laboratory, "ready for packing", returned CCP records and the final honey result
public partial class TOP4HoneyChainsContext
{
    private static void OnModelCreatingD98(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity => entity.Property(e => e.CcpPath).IsUnicode(false));
        modelBuilder.Entity<Sample>(entity =>
        {
            entity.HasIndex(e => e.FinalHoneyProductionId).HasFilter("[FinalHoneyProductionId] IS NOT NULL");
            entity.HasOne(e => e.FinalHoneyProduction).WithMany().HasForeignKey(e => e.FinalHoneyProductionId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
