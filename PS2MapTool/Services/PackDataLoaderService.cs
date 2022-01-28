using PS2MapTool.Abstractions.Services;
using PS2MapTool.Areas;
using PS2MapTool.Tiles;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services;

/// <summary>
/// Provides functions to load mapping data from pack2 files.
/// </summary>
public class PackDataLoaderService : IDataLoaderService
{
    protected readonly string _packsLocation;

    /// <summary>
    /// Initialises a new instance of the <see cref="PackDataLoaderService"/> object.
    /// </summary>
    /// <param name="packsPath">The path to the pack2 files.</param>
    public PackDataLoaderService(string packsPath)
    {
        _packsLocation = packsPath;
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public virtual IEnumerable<TileDataSource> GetTiles(string worldName, Lod lod, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public virtual Task<AreasSourceInfo> GetAreasAsync(string worldName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
