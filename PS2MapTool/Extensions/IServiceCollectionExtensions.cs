using Microsoft.Extensions.DependencyInjection.Extensions;
using PS2MapTool.Services;
using PS2MapTool.Services.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds components that enable the use of asset-based map tools.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>An <see cref="IServiceCollection"/> so that calls may be chained.</returns>
        public static IServiceCollection AddAssetMappingServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IAreasService, AreasService>();
            services.TryAddSingleton<IDataLoaderService, DirectoryDataLoaderService>();
            services.TryAddSingleton<IImageCompressionService, IImageCompressionService>();
            services.TryAddSingleton<IImageStitchService, ImageStitchService>();

            services.TryAddSingleton<DdsTileLoaderService>();
            services.TryAddSingleton<PngTileLoaderService>();
            services.TryAddSingleton(s =>
            {
                TileLoaderServiceRepository repo = new();
                repo.Add(s.GetRequiredService<DdsTileLoaderService>());
                repo.Add(s.GetRequiredService<PngTileLoaderService>());
                return repo;
            });

            return services;
        }
    }
}
