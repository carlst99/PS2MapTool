﻿using PS2MapTool.Abstractions.Tiles;
using System;
using System.Collections.Generic;

namespace PS2MapTool.Tiles;

/// <summary>
/// A dictionary wrapper for storing map tiles.
/// </summary>
public class TileBucket : IDisposable
{
    private readonly Dictionary<string, Dictionary<Lod, List<ITileDataSource>>> _lodBucket;

    /// <summary>
    /// Gets or sets a value indicating whether or not this <see cref="TileBucket"/> instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="TileBucket"/> object.
    /// </summary>
    public TileBucket()
    {
        _lodBucket = new Dictionary<string, Dictionary<Lod, List<ITileDataSource>>>();
    }

    /// <summary>
    /// Adds a tile to the bucket.
    /// </summary>
    /// <param name="tile">The tile to add.</param>
    public void AddTile(ITileDataSource tile)
    {
        // Ensure that the LOD bucket exists for this world
        if (!_lodBucket.ContainsKey(tile.WorldName))
            _lodBucket[tile.WorldName] = new Dictionary<Lod, List<ITileDataSource>>();

        // Ensure that the tile bucket exists for this LOD
        if (!_lodBucket[tile.WorldName].ContainsKey(tile.Lod))
            _lodBucket[tile.WorldName][tile.Lod] = new List<ITileDataSource>();

        _lodBucket[tile.WorldName][tile.Lod].Add(tile);
    }

    /// <summary>
    /// Adds multiple tiles to the bucket.
    /// </summary>
    /// <param name="tiles">The tiles to add.</param>
    public void AddTiles(IEnumerable<ITileDataSource> tiles)
    {
        foreach (ITileDataSource tile in tiles)
            AddTile(tile);
    }

    /// <summary>
    /// Gets all the worlds that this bucket is storing tiles of.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetWorlds() => _lodBucket.Keys;

    /// <summary>
    /// Gets all the LODs for a world that this bucket is storing tiles of.
    /// </summary>
    /// <param name="world">The world to get the LODs of.</param>
    /// <returns></returns>
    public IEnumerable<Lod> GetLods(string world) => _lodBucket[world].Keys;

    /// <summary>
    /// Gets the stored tiles for a particular world/LOD.
    /// </summary>
    /// <param name="world">The world to get tiles for.</param>
    /// <param name="lod">The LOD of the world to get tiles for."/></param>
    /// <returns></returns>
    public List<ITileDataSource> GetTiles(string world, Lod lod)
    {
        if (!_lodBucket.ContainsKey(world))
            throw new ArgumentException("No tiles for that world have been stored in this bucket.", nameof(world));

        if (!_lodBucket[world].ContainsKey(lod))
            throw new ArgumentException("No tiles for the LOD of that world have been stored in this bucket.", nameof(lod));

        return _lodBucket[world][lod];
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposedManaged">A value indicating whether or not to dispose of managed resources.</param>
    protected virtual void Dispose(bool disposedManaged)
    {
        if (IsDisposed)
            return;

        if (disposedManaged)
        {
            foreach (Dictionary<Lod, List<ITileDataSource>> dics in _lodBucket.Values)
            {
                foreach (List<ITileDataSource> tiles in dics.Values)
                {
                    foreach (ITileDataSource tile in tiles)
                    {
                        if (tile is IDisposable disposable)
                            disposable.Dispose();
                    }
                }
            }
        }

        IsDisposed = true;
    }
}
