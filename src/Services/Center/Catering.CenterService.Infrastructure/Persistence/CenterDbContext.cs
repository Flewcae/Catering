using Catering.CenterService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.CenterService.Infrastructure.Persistence;

public sealed class CenterDbContext(DbContextOptions<CenterDbContext> options) : DbContext(options)
{
    public DbSet<Center> Centers => Set<Center>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Center>(builder =>
        {
            builder.ToTable("centers");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
            builder.Property(c => c.Address).HasMaxLength(500).IsRequired();
        });
    }
}
