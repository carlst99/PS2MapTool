using PS2MapTool.Services.Abstractions;
using PS2MapTool.Tiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services;

/// <inheritdoc cref="IImageStitchService"/>
public class ImageStitchService : IImageStitchService
{
    /// <summary>
    /// The pixel size of each tile.
    /// </summary>
    public const int TILE_SIZE = 256;

    private readonly TileLoaderServiceRepository _tileProcessorRepository;

    /// <summary>
    /// Initialises a new instance of the <see cref="ImageStitchService"/> class.
    /// </summary>
    /// <param name="tileProcessorRepository">The tile processor to use.</param>
    public ImageStitchService(TileLoaderServiceRepository tileProcessorRepository)
    {
        _tileProcessorRepository = tileProcessorRepository;
    }

    /// <inheritdoc />
    public virtual async Task<Image<Rgba32>> StitchTilesAsync(IList<TileDataSource> tiles, CancellationToken ct = default)
    {
        IEnumerable<TileDataSource> orderedBucket = tiles.OrderByDescending(t => t.Y).ThenBy(t => t.X);

        double root = Math.Sqrt(tiles.Count);
        if (root != (int)root)
            throw new InvalidOperationException("There must be a square amount of tiles to stitch them together.");

        // Allocate for the stitched image
        int tilesPerSide = (int)root; // Square image, this will always be an integer
        int pixelsPerSide = tilesPerSide * TILE_SIZE;
        Image<Rgba32> stitchedImage = new(pixelsPerSide, pixelsPerSide);

        // Draw each tile onto the stitched image.
        int x = 0, y = 0;
        foreach (TileDataSource tile in orderedBucket)
        {
            if (!_tileProcessorRepository.TryGet(tile, out ITileLoaderService? loader))
                throw new Exception($"The tile {tile.World}Tile__{tile.X}_{tile.Y}_{tile.Lod} is an unknown image format.");

            Image tileImage = await loader.LoadAsync(tile, ct).ConfigureAwait(false);
            tileImage.Mutate(o => o.Flip(FlipMode.Vertical));

            stitchedImage.Mutate(o => o.DrawImage(tileImage, new Point(x, y), 1f));

            x += TILE_SIZE;
            if (x == pixelsPerSide)
            {
                x = 0;
                y += TILE_SIZE;
            }

            tileImage.Dispose();

            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();
        }

        return stitchedImage;
    }
}
