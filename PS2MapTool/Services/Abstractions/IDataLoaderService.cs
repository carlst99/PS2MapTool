﻿using PS2MapTool.Areas;
using PS2MapTool.Tiles;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services.Abstractions;

/// <summary>
/// Provides functions to load map data.
/// </summary>
public interface IDataLoaderService
{
    /// <summary>
    /// Loads map tile info from a data source.
    /// </summary>
    /// <param name="world">The world to retrieve the map tiles of.</param>
    /// <param name="lod">The LOD to retrieve the map tiles of.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the tile enumeration.</param>
    /// <returns>The tiles.</returns>
    IEnumerable<TileInfo> GetTiles(AssetZone world, Lod lod, CancellationToken ct = default);

    /// <summary>
    /// Loads an <see cref="AreasSourceInfo"/> object from a data source.
    /// </summary>
    /// <param name="world">The world to retrieve the area data of.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to stop the operation.</param>
    /// <returns>The areas info.</returns>
    Task<AreasSourceInfo> GetAreasAsync(AssetZone world, CancellationToken ct = default);
}
