using Catering.NotificationService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.NotificationService.Infrastructure.Persistence;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(builder =>
        {
            builder.ToTable("notifications");
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Recipient).HasMaxLength(256).IsRequired();
            builder.Property(n => n.Subject).HasMaxLength(256);
            builder.Property(n => n.Body).HasMaxLength(4000).IsRequired();
            builder.Property(n => n.Channel).HasConversion<string>().HasMaxLength(16);
            builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(16);
            builder.Property(n => n.ErrorMessage).HasMaxLength(2000);
            builder.HasIndex(n => n.UserId);
            builder.Ignore(n => n.DomainEvents);
        });
    }
}
