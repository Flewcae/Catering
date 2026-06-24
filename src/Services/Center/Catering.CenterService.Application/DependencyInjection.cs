using Microsoft.Extensions.DependencyInjection;

namespace Catering.CenterService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCenterServiceApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
