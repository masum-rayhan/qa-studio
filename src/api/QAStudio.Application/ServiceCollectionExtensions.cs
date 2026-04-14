using Microsoft.Extensions.DependencyInjection;

namespace QAStudio.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application layer interfaces are registered in Infrastructure layer
        // where the implementations live
        return services;
    }
}
