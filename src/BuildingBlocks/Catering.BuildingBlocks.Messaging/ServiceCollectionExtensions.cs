using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catering.BuildingBlocks.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<IEventBus, KafkaEventBus>();
        return services;
    }
}
