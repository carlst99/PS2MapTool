using DbgCensus.Rest.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PS2MapTool.Census.Services;
using PS2MapTool.Census.Services.Abstractions;

namespace PS2MapTool.Census.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddCensusMappingServices(this IServiceCollection services)
        {
            services.AddCensusRestServices();
            services.TryAddSingleton<ICensusService, CensusService>();

            return services;
        }
    }
}
