﻿using PS2MapTool.Areas;
using PS2MapTool.Tiles;
using PS2MapTool.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<TileInfo> GetTiles(World world, Lod lod, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<AreasSourceInfo> GetAreasAsync(World world, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
