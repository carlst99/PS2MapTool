using PS2MapTool.Core.Areas;
using PS2MapTool.Core.Tiles;
using PS2MapTool.Core.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Core.Services
{
    /// <summary>
    /// Provides functions to load mapping data from pack2 files.
    /// </summary>
    public sealed class PackDataLoaderService : IDataLoaderService
    {
        private readonly string _packsLocation;

        /// <summary>
        /// Initialises a new instance of the <see cref="PackDataLoaderService"/> object.
        /// </summary>
        /// <param name="packsPath">The path to the pack2 files.</param>
        public PackDataLoaderService(string packsPath)
        {
            _packsLocation = packsPath;
        }

        /// <inheritdoc />
        public IAsyncEnumerable<TileInfo> GetTilesAsync(World world, Lod lod, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<AreasInfo> GetAreasInfoAsync(World world, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
