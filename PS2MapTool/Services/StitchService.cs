using PS2MapTool.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
{
    public class StitchService
    {
        private const string PREFIX = "[purple]STITCH [/]";

        /// <summary>
        /// The pixel size of each tile.
        /// </summary>
        public const int TILE_SIZE = 256;

        private readonly IAnsiConsole _console;
        private readonly ParallelTaskRunner _taskRunner;

        public StitchService(IAnsiConsole console, ParallelTaskRunner taskRunner)
        {
            _console = console;
            _taskRunner = taskRunner;
        }

        /// <summary>
        /// Filters through all files in the given directory and sorts valid tiles into a <see cref="WorldLodBucket"/>
        /// </summary>
        /// <param name="tilesSource">The directory in which the tiles are stored.</param>
        /// <param name="worlds">Setting this parameter will only include tiles of the given worlds.</param>
        /// <param name="lods">Setting this parameter will only include tiles of the given LODs.</param>
        public static WorldLodBucket GenerateTileBuckets(string tilesSource, IEnumerable<string>? worlds, IEnumerable<int>? lods = null)
        {
            List<string>? normalisedLods = lods?.Select(l => "LOD" + l.ToString()).ToList();
            WorldLodBucket bucket = new(worlds, normalisedLods);

            foreach (string filePath in Directory.EnumerateFiles(tilesSource))
            {
                if (!Tile.TryParse(filePath, out Tile tile))
                    continue;

                bucket.AddLodTile(tile);
            }

            return bucket;
        }

        /// <summary>
        /// Stitches tiles into a map.
        /// </summary>
        /// <param name="tileBucket">The tiles to stitch.</param>
        /// <param name="outputPath">The path to output the map to.</param>
        /// <param name="completionCallback">Will be called on a successful stitch, with the path to the saved map as the parameter.</param>
        public void StitchTiles(WorldLodBucket tileBucket, string outputPath, CancellationToken ct, Action<string>? completionCallback = null)
        {
            foreach (string world in tileBucket.GetWorlds())
            {
                foreach (string lod in tileBucket.GetLods(world))
                {
                    StitchTiles(tileBucket.GetTiles(world, lod).ToList(), outputPath, ct, completionCallback);
                }
            }
        }

        /// <summary>
        /// Stitches tiles into a map.
        /// </summary>
        /// <param name="tiles">The tiles to stitch.</param>
        /// <param name="outputPath">The path to output the map to.</param>
        /// <param name="completionCallback">Will be called on a successful stitch, with the path to the saved map as the parameter.</param>
        public void StitchTiles(IList<Tile> tiles, string outputPath, CancellationToken ct, Action<string>? completionCallback = null)
        {
            Task<string> stitchTask = new(() =>
            {
                Tile referenceTile = tiles[0];
                _console.MarkupLine($"{ PREFIX }Stitching tiles for { Formatter.World(referenceTile.World) } at { Formatter.Lod(referenceTile.Lod) }...");

                IEnumerable<Tile> orderedBucket = tiles.OrderByDescending((b) => b.X).ThenBy((b) => b.Y);

                // Allocate for the stitched image
                int tilesPerSide = (int)Math.Sqrt(tiles.Count); // Square image, this will always be an integer
                int pixelsPerSide = tilesPerSide * TILE_SIZE; // Each tile is 256x256 pixels
                using Image<Rgba32> stitchedImage = new(pixelsPerSide, pixelsPerSide);

                // Draw each tile onto the stitched image.
                int x = 0, y = 0;
                foreach (Tile tile in orderedBucket)
                {
                    Image tileImage = LoadDDSImage(tile.Path);
                    tileImage.Mutate(o => o.Rotate(RotateMode.Rotate270));

                    stitchedImage.Mutate(o => o.DrawImage(tileImage, new Point(x, y), 1f));
                    tileImage.Dispose();

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
                _console.MarkupLine($"{ PREFIX }{ Formatter.Success("Completed") } stitching tiles for { Formatter.World(referenceTile.World) } at { Formatter.Lod(referenceTile.Lod) }...");

                string outputFilePath = Path.Combine(outputPath, $"{referenceTile.World}_{referenceTile.Lod}.png");

                // Save the stitched image.
                _console.MarkupLine($"{ PREFIX }Saving { Formatter.World(referenceTile.World) } at { Formatter.Lod(referenceTile.Lod) }...");
                stitchedImage.SaveAsPng(outputFilePath);
                stitchedImage.Dispose();
                _console.MarkupLine($"{ PREFIX }{ Formatter.Success("Completed") } saving { Formatter.World(referenceTile.World) } at { Formatter.Lod(referenceTile.Lod) }...");

                return outputFilePath;
            }, ct, TaskCreationOptions.LongRunning);

            stitchTask.ContinueWith((t) =>
            {
                if (t.IsCompletedSuccessfully && completionCallback is not null)
                    completionCallback(t.Result);
            }, ct);

            _taskRunner.EnqueueTask(stitchTask);
        }

        /// <summary>
        /// Loads a DDS image in an ImageSharp-compatible fashion.
        /// </summary>
        /// <param name="filePath">The path to the image to load.</param>
        /// <returns></returns>
        public static Image<Bgra32> LoadDDSImage(string filePath)
        {
            using Pfim.IImage image = Pfim.Pfim.FromFile(filePath);
            byte[] newData;

            int tightStride = image.Width * image.BitsPerPixel / 8;
            if (image.Stride != tightStride)
            {
                newData = new byte[image.Height * tightStride];
                for (int i = 0; i < image.Height; i++)
                {
                    Buffer.BlockCopy(image.Data, i * image.Stride, newData, i * tightStride, tightStride);
                }
            }
            else
            {
                newData = image.Data;
            }

            return Image.LoadPixelData<Bgra32>(newData, image.Width, image.Height);
        }
    }
}
