using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PS2MapTool.Abstractions.Services;
using PS2MapTool.Abstractions.Tiles.Services;
using PS2MapTool.Services;
using PS2MapTool.Tiles.Services;

namespace PS2MapTool.Extensions;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds components that enable the use of asset-based map tools.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>An <see cref="IServiceCollection"/> so that calls may be chained.</returns>
    public static IServiceCollection AddPS2MapToolServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IAreasService, AreasService>();
        services.TryAddSingleton<IDataLoaderService, DirectoryDataLoaderService>();
        services.TryAddSingleton<ITileStitchService, TileStitchService>();

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
