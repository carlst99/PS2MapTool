using CommunityToolkit.HighPerformance.Buffers;
using PS2MapTool.Abstractions.Tiles;
using PS2MapTool.Abstractions.Tiles.Services;
using PS2MapTool.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Tiles.Services;

/// <inheritdoc cref="ITileStitchService"/>
public class TileStitchService : ITileStitchService
{
    /// <summary>
    /// The pixel size of each tile.
    /// </summary>
    public const int TILE_SIZE = 256;

    private readonly TileLoaderServiceRepository _tileProcessorRepository;

    /// <summary>
    /// Initialises a new instance of the <see cref="TileStitchService"/> class.
    /// </summary>
    /// <param name="tileProcessorRepository">The tile processor to use.</param>
    public TileStitchService(TileLoaderServiceRepository tileProcessorRepository)
    {
        _tileProcessorRepository = tileProcessorRepository;
    }

    /// <inheritdoc />
    public virtual async Task<Image<Rgba32>> StitchAsync(IList<ITileDataSource> tiles, CancellationToken ct = default)
    {
        IEnumerable<ITileDataSource> orderedBucket = tiles.OrderByDescending(t => t.Y).ThenBy(t => t.X);

        double root = Math.Sqrt(tiles.Count);
        if (root != (int)root)
            throw new InvalidOperationException("There must be a square amount of tiles to stitch them together.");

        // Allocate for the stitched image
        int tilesPerSide = (int)root; // Square image, this will always be an integer
        int pixelsPerSide = tilesPerSide * TILE_SIZE;
        Image<Rgba32> stitchedImage = new(pixelsPerSide, pixelsPerSide);

        // Draw each tile onto the stitched image.
        int x = 0, y = 0;
        foreach (ITileDataSource tile in orderedBucket)
        {
            using MemoryOwner<byte> buffer = await tile.GetTileDataAsync(ct);

            if (!_tileProcessorRepository.TryGet(buffer.Span, out ITileLoaderService? loader))
                throw new Exception($"The tile {tile.WorldName}Tile__{tile.X}_{tile.Y}_{tile.Lod} is an unknown image format.");

            using Image tileImage = loader.Load(buffer.Span);
            tileImage.Mutate(o => o.Flip(FlipMode.Vertical));

            stitchedImage.Mutate(o => o.DrawImage(tileImage, new Point(x, y), 1f));

            x += TILE_SIZE;
            if (x == pixelsPerSide)
            {
                x = 0;
                y += TILE_SIZE;
            }

            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();
        }

        return stitchedImage;
    }
}
