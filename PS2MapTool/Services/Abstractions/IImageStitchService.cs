using PS2MapTool.Tiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services.Abstractions;

/// <summary>
/// Defines a service used to stitch map tiles together.
/// </summary>
public interface IImageStitchService
{
    /// <summary>
    /// Stitches the given tiles into a complete map image.
    /// </summary>
    /// <param name="tiles">The tile to stitch.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
    /// <returns>An <see cref="Image{Rgba32}"/>.</returns>
    Task<Image<Rgba32>> StitchTilesAsync(IList<TileInfo> tiles, CancellationToken ct = default);
}
