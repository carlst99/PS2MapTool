using PS2MapTool.Services;
using PS2MapTool.Services.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds components required to use the map tool services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>An <see cref="IServiceCollection"/> so that calls may be chained.</returns>
        public static IServiceCollection AddPS2MapToolServices(this IServiceCollection services)
        {
            services.AddSingleton<IAreasService, AreasService>()
                    .AddSingleton<IDataLoaderService, DirectoryDataLoaderService>()
                    .AddSingleton<IImageCompressionService, IImageCompressionService>()
                    .AddSingleton<IImageStitchService, ImageStitchService>();

            services.AddSingleton<DdsTileLoaderService>();
            services.AddSingleton<PngTileLoaderService>()
                    .AddSingleton(s =>
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
