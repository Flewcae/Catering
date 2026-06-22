using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.Abstractions;
using Catering.NotificationService.Application.IntegrationEvents;
using Catering.NotificationService.Infrastructure.Channels;
using Catering.NotificationService.Infrastructure.Messaging;
using Catering.NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catering.NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServiceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("NotificationDb")));

        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IEmailSender, ConsoleEmailSender>();
        services.AddScoped<ISmsSender, ConsoleSmsSender>();
        services.AddScoped<IPushNotificationSender, ConsolePushNotificationSender>();

        services.AddScoped<IIntegrationEventHandler<UserCreatedIntegrationEvent>, Application.IntegrationEventHandlers.UserCreatedIntegrationEventHandler>();
        services.AddHostedService<UserCreatedConsumer>();

        return services;
    }
}
