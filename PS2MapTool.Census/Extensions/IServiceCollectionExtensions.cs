using DbgCensus.Rest.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PS2MapTool.Census.Services;
using PS2MapTool.Census.Services.Abstractions;

namespace PS2MapTool.Census.Extensions;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services that enable the use of census-based map tools.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>An <see cref="IServiceCollection"/> so that calls may be chained.</returns>
    public static IServiceCollection AddCensusMappingServices(this IServiceCollection services)
    {
        services.AddCensusRestServices();
        services.TryAddSingleton<ICensusService, CensusService>();

        return services;
    }
}
