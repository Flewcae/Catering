using Catering.CenterService.Application.Abstractions;
using Catering.CenterService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catering.CenterService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCenterServiceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CenterDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CenterDb")));

        services.AddScoped<ICenterRepository, CenterRepository>();

        return services;
    }
}
