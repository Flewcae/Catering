using Catering.UserService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.UserService.Infrastructure.Persistence;

public sealed class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetRequest> PasswordResetRequests => Set<PasswordResetRequest>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<Center> Centers => Set<Center>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.TcIdentityNumber).HasMaxLength(11).IsRequired();
            builder.Property(u => u.PhoneNumber).HasMaxLength(32).IsRequired();
            builder.Property(u => u.Address).HasMaxLength(500);
            builder.Property(u => u.ProfilePictureUrl).HasMaxLength(1000);
            builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(32);
            builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(32);
            builder.Property(u => u.DisabilityDescription).HasMaxLength(1000);
            builder.Property(u => u.SalaryCeiling).HasPrecision(18, 2);
            builder.Property(u => u.Notes).HasMaxLength(2000);

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.TcIdentityNumber).IsUnique();

            builder.Property(u => u.CenterId);

            builder.HasOne(u => u.Department).WithMany().HasForeignKey(u => u.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(u => u.Position).WithMany().HasForeignKey(u => u.PositionId).OnDelete(DeleteBehavior.Restrict);

            builder.Ignore(u => u.DomainEvents);
        });

        modelBuilder.Entity<Department>(builder =>
        {
            builder.ToTable("departments");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
            builder.Property(d => d.Description).HasMaxLength(1000);
            builder.HasIndex(d => d.Name).IsUnique();
        });

        modelBuilder.Entity<Position>(builder =>
        {
            builder.ToTable("positions");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(1000);
            builder.Property(p => p.Permissions).HasColumnType("text[]");
            builder.HasIndex(p => p.Name).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("refresh_tokens");
            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.TokenHash).HasMaxLength(128).IsRequired();
            builder.HasIndex(rt => rt.TokenHash).IsUnique();
            builder.HasOne(rt => rt.User).WithMany().HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetRequest>(builder =>
        {
            builder.ToTable("password_reset_requests");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.CodeHash).HasMaxLength(128).IsRequired();
            builder.Property(p => p.Channel).HasConversion<string>().HasMaxLength(16);
            builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceToken>(builder =>
        {
            builder.ToTable("device_tokens");
            builder.HasKey(dt => dt.Id);
            builder.Property(dt => dt.Token).HasMaxLength(512).IsRequired();
            builder.Property(dt => dt.Platform).HasMaxLength(32).IsRequired();
            builder.HasIndex(dt => dt.Token).IsUnique();
            builder.HasIndex(dt => dt.UserId);
            builder.HasOne(dt => dt.User).WithMany().HasForeignKey(dt => dt.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Center>(builder =>
        {
            builder.ToTable("centers_cache");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
            builder.Property(c => c.Address).HasMaxLength(500).IsRequired();
            builder.HasIndex(c => c.CenterId).IsUnique();
        });
    }
}
