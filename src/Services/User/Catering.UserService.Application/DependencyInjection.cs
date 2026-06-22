using Microsoft.Extensions.DependencyInjection;

namespace Catering.UserService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserServiceApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
