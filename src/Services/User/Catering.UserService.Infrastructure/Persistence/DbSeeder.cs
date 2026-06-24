using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;
using Catering.UserService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Catering.UserService.Infrastructure.Persistence;

public static class DbSeeder
{
    private const string SeedAdminEmail = "admin@catering.local";
    private const string SeedAdminPassword = "Admin123!";
    private const string SeedAdminTcIdentityNumber = "10000000146";

    public static async Task SeedAsync(UserDbContext dbContext, IPasswordHasher passwordHasher)
    {
        var department = await dbContext.Departments.FirstOrDefaultAsync();
        if (department is null)
        {
            department = Department.Create("Genel Müdürlük", "Sistem yönetimi ve genel operasyonlar");
            await dbContext.Departments.AddAsync(department);
            await dbContext.SaveChangesAsync();
        }

        var position = await dbContext.Positions.FirstOrDefaultAsync();
        if (position is null)
        {
            position = Position.Create("Sistem Yöneticisi", "Sistem yönetimi pozisyonu");
            await dbContext.Positions.AddAsync(position);
            await dbContext.SaveChangesAsync();
        }

        var adminExists = await dbContext.Users.AnyAsync(u => u.Email == SeedAdminEmail);
        if (!adminExists)
        {
            var admin = User.Register(
                SeedAdminEmail,
                passwordHasher.Hash(SeedAdminPassword),
                "Sistem",
                "Yöneticisi",
                SeedAdminTcIdentityNumber,
                "+905000000000",
                birthDate: null,
                address: null,
                department.Id,
                position.Id,
                DateOnly.FromDateTime(DateTime.UtcNow),
                hasDisability: false,
                disabilityDescription: null,
                salaryCeiling: null,
                notes: "Seed ile oluşturulan varsayılan yönetici hesabı.",
                role: SystemRole.SuperAdmin,
                passwordRegistered: true);

            await dbContext.Users.AddAsync(admin);
            await dbContext.SaveChangesAsync();
        }
    }
}
