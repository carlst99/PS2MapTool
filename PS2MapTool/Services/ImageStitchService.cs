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

namespace PS2MapTool.Services
{
    /// <inheritdoc cref="IImageStitchService"/>
    public class ImageStitchService : IImageStitchService
    {
        /// <summary>
        /// The pixel size of each tile.
        /// </summary>
        public const int TILE_SIZE = 256;

        private readonly TileProcessorServiceRepository _tileProcessorRepository;

        /// <summary>
        /// Initialises a new instance of the <see cref="ImageStitchService"/> class.
        /// </summary>
        /// <param name="tileProcessorRepository">The tile processor to use.</param>
        public ImageStitchService(TileProcessorServiceRepository tileProcessorRepository)
        {
            _tileProcessorRepository = tileProcessorRepository;
        }

        /// <inheritdoc />
        public virtual async Task<Image<Rgba32>> StitchTilesAsync(IList<TileInfo> tiles, CancellationToken ct = default)
        {
            IEnumerable<TileInfo> orderedBucket = tiles.OrderByDescending((b) => b.X).ThenBy((b) => b.Y); // TODO: Might need to change this for other swap?

            // Allocate for the stitched image
            int tilesPerSide = (int)Math.Sqrt(tiles.Count); // Square image, this will always be an integer
            int pixelsPerSide = tilesPerSide * TILE_SIZE; // Each tile is 256x256 pixels
            Image<Rgba32> stitchedImage = new(pixelsPerSide, pixelsPerSide);

            // Draw each tile onto the stitched image.
            int x = 0, y = 0;
            foreach (TileInfo tile in orderedBucket)
            {
                using Image tileImage = await _tileProcessorRepository.Get(tile.ImageFormatType).LoadAsync(tile.DataSource, ct).ConfigureAwait(false);
                tileImage.Mutate(o => o.Rotate(RotateMode.Rotate270));

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

            // Rotate and flip the stitched image as required.
            stitchedImage.Mutate(o => o.RotateFlip(RotateMode.Rotate90, FlipMode.Vertical));
            return stitchedImage;
        }
    }
}
