using SixLabors.ImageSharp;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Abstractions.Tiles.Services;

/// <summary>
/// Defines a service used to load tile data into a usable format.
/// </summary>
public interface ITileLoaderService
{
    /// <summary>
    /// Gets a value indicating whether or not this <see cref="ITileLoaderService"/> can load the given type of tile.
    /// </summary>
    /// <param name="tile">The tile.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A value indicating whether the tile can be loaded by this <see cref="ITileLoaderService"/>.</returns>
    Task<bool> CanLoadAsync(ITileDataSource tile, CancellationToken ct = default);

    /// <summary>
    /// Loads a tile into a usable in-memory representation.
    /// </summary>
    /// <param name="tile">The tile.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
    /// <returns>An <see cref="Image"/>. The type of pixel buffer is determined by the implementing class.</returns>
    Task<Image> LoadAsync(ITileDataSource tile, CancellationToken ct = default);
}
