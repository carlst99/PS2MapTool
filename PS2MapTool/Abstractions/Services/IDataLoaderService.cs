using PS2MapTool.Abstractions.Tiles;
using PS2MapTool.Areas;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Abstractions.Services;

/// <summary>
/// Provides functions to load mapping data.
/// </summary>
public interface IDataLoaderService
{
    /// <summary>
    /// Loads map tile info from a data source.
    /// </summary>
    /// <param name="worldName">The world to retrieve the map tiles of.</param>
    /// <param name="lod">The LOD to retrieve the map tiles of.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the tile enumeration.</param>
    /// <returns>The list of tiles.</returns>
    Task<IReadOnlyList<ITileDataSource>> GetTilesAsync(string worldName, Lod lod, CancellationToken ct = default);

    /// <summary>
    /// Loads an <see cref="AreasSourceInfo"/> object from a data source.
    /// </summary>
    /// <param name="worldName">The world to retrieve the area data of.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to stop the operation.</param>
    /// <returns>The areas info.</returns>
    Task<AreasSourceInfo> GetAreasAsync(string worldName, CancellationToken ct = default);
}
