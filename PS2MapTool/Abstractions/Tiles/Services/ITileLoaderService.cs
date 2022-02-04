using SixLabors.ImageSharp;
using System;

namespace PS2MapTool.Abstractions.Tiles.Services;

/// <summary>
/// Defines a service used to load tile data into a usable format.
/// </summary>
public interface ITileLoaderService
{
    /// <summary>
    /// Gets a value indicating whether or not this <see cref="ITileLoaderService"/> can load the given type of tile.
    /// </summary>
    /// <param name="buffer">The tile data.</param>
    /// <returns>A value indicating whether the tile can be loaded by this <see cref="ITileLoaderService"/>.</returns>
    bool CanLoad(ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Loads a tile into a usable in-memory representation.
    /// </summary>
    /// <param name="buffer">The tile data.</param>
    /// <returns>An <see cref="Image"/>. The type of pixel buffer is determined by the implementing class.</returns>
    Image Load(ReadOnlySpan<byte> buffer);
}
