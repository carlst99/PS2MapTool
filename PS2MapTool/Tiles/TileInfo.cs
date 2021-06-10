﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace PS2MapTool.Tiles
{
    /// <summary>
    /// Contains information about a tile data source.
    /// </summary>
    public record TileInfo : IAsyncDisposable
    {
        /// <summary>
        /// The file extension for raw tiles.
        /// </summary>
        public const string TILE_EXTENSION = ".dds";

        /// <summary>
        /// The data source.
        /// </summary>
        public Stream DataSource { get; init; }

        /// <summary>
        /// The world that this tile belongs to.
        /// </summary>
        public World World { get; init; }

        /// <summary>
        /// The X coordinate of the tile.
        /// </summary>
        public int X { get; init; }

        /// <summary>
        /// The Y coordinate of the tile.
        /// </summary>
        public int Y { get; init; }

        /// <summary>
        /// The level of detail that this tile is for.
        /// </summary>
        public Lod Lod { get; init; }

        /// <summary>
        /// Gets a value indicating if this <see cref="TileInfo"/> object has been disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="TileInfo"/> object.
        /// </summary>
        /// <param name="world">The world that this tile belongs to.</param>
        /// <param name="x">The X coordinate of the tile.</param>
        /// <param name="y">The Y coordinate of the tile.</param>
        /// <param name="lod">The level of detail that this tile is for.</param>
        /// <param name="dataSource">The data source.</param>
        public TileInfo(World world, int x, int y, Lod lod, Stream dataSource)
        {
            World = world;
            X = x;
            Y = y;
            Lod = lod;
            DataSource = dataSource;
        }

        public static bool TryParse(string tileName, Stream dataSource, out TileInfo? tile)
        {
            tile = null;
            string[] nameComponents = tileName.Split('_');

            if (nameComponents.Length != 5)
                return false;

            if (!Enum.TryParse(nameComponents[0], out World world))
                return false;

            if (nameComponents[1] != "Tile")
                return false;

            if (!int.TryParse(nameComponents[2], out int y))
                return false;

            if (!int.TryParse(nameComponents[3], out int x)) // TODO: Testing a swap of the X and Y positions
                return false;

            if (!Enum.TryParse(nameComponents[4], out Lod lod))
                return false;

            tile = new TileInfo(world, x, y, lod, dataSource);

            return true;
        }

        public override string ToString()
        {
            return $"{World}_Tile_{Y}_{X}_{Lod}";
        }

        /// <summary>
        /// Disposes of the <see cref="DataSource"/>.
        /// </summary>
        /// <returns>A value representing the task.</returns>
        public async ValueTask DisposeAsync()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            await DisposeAsync(disposing: true).ConfigureAwait(false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (DataSource is not null)
                        await DataSource.DisposeAsync().ConfigureAwait(false);
                }

                IsDisposed = true;
            }
        }
    }
}