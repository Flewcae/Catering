using Microsoft.Extensions.DependencyInjection;

namespace Catering.NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServiceApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
