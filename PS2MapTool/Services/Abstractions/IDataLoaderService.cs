using PS2MapTool.Areas;
using PS2MapTool.Tiles;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services.Abstractions
{
    /// <summary>
    /// Provides functions to load mapping data.
    /// </summary>
    public interface IDataLoaderService
    {
        /// <summary>
        /// Loads map tile info from a data source.
        /// </summary>
        /// <param name="world">The world to retrieve the map tiles of.</param>
        /// <param name="lod">The LOD to retrieve the map tiles of.</param>
        /// <returns>The tiles.</returns>
        IAsyncEnumerable<TileInfo> GetTilesAsync(World world, Lod lod, CancellationToken ct = default);

        /// <summary>
        /// Loads an <see cref="AreasInfo"/> object from a data source.
        /// </summary>
        /// <param name="world">The world to retrieve the area data of.</param>
        /// <returns>The areas info.</returns>
        Task<AreasInfo> GetAreasAsync(World world, CancellationToken ct = default);
    }
}
