using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using Pfim;
using PS2MapTools.Validators;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTools.Commands
{
    [Command("stitch", Description = "Stitches LOD tiles together to form a complete map. Maps will be created for all World/LOD combinations found in the source directory, unless otherwise specified.")]
    public class StitchCommand : ICommand
    {
        private const string OPTIPNG_FILE_NAME = "optipng.exe";
        private const int TILE_SIZE = 256;

        private readonly Dictionary<string, Dictionary<string, List<Tile>>> _worldLodBuckets = new();
        private readonly ParallelTaskRunner _taskRunner;

        private IAnsiConsole _console;

        #region Command Parameters/Options

        [CommandParameter(0, Description = "The path to the directory containing the LOD tiles. Each tile should be named in the format <World>_Tile_<Y>_<X>_LOD*.")]
        public string TilesSource { get; init; }

        [CommandOption("output", 'o', Description = "The path to output the stitched map/s to.", IsRequired = false)]
        public string? OutputPath { get; init; }

        [CommandOption("disable-compression", 'd', Description = "Prevents the stitched map/s from being compressed using OptiPNG. Saves a considerable amount of time at the expense of producing maps 40-50% larger in size.", IsRequired = false)]
        public bool DisableCompression { get; init; }

        [CommandOption("max-parallelism", 'p', Description = "The maximum amount of maps that may be stitched AND compressed in parallel. Lower values use less memory and CPU resouces.", Validators = new Type[] { typeof(MaxParallelValidator) })]
        public int MaxParallelism { get; init; }

        [CommandOption("worlds", 'w', Description = "Limits map generation to the given worlds.")]
        public IReadOnlyList<string>? Worlds { get; init; }

        [CommandOption("lods", 'l', Description = "Limits map generation to the given LODs", Validators = new Type[] { typeof(LODNumberValidator) })]
        public IReadOnlyList<int>? LODs { get; init; }

        #endregion

        public StitchCommand()
        {
            TilesSource = string.Empty;
            MaxParallelism = 4;
            _taskRunner = new ParallelTaskRunner();
            _console = AnsiConsole.Create(new AnsiConsoleSettings());
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (!Directory.Exists(TilesSource))
                throw new CommandException("The provided tiles source directory does not exist.");

            if (OutputPath is not null && !Directory.Exists(OutputPath))
            {
                try
                {
                    Directory.CreateDirectory(OutputPath);
                }
                catch (Exception ex)
                {
                    throw new CommandException("The specified output directory does not exist and could not be created.", innerException: ex);
                }
            }

            _console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(console.Output)
            });

            CancellationToken ct = console.RegisterCancellationHandler();
            _taskRunner.Start(ct, MaxParallelism);

            GenerateWorldLodBuckets();
            EnqueueStitchTasks(ct);

            // Job done, wait for all tasks to complete.
            await _taskRunner.WaitForAll().ConfigureAwait(false);
            _taskRunner.Stop();
        }

        /// <summary>
        /// Filters through all files in the source directory and sorts tiles into applicable <see cref="_worldLodBuckets"/>.
        /// </summary>
        private void GenerateWorldLodBuckets()
        {
            _console.Write("Generating map buckets...");
            IEnumerable<string>? normalisedLods = LODs?.Select(l => "LOD" + l.ToString());

            foreach (string filePath in Directory.EnumerateFiles(TilesSource))
            {
                if (!Tile.TryParse(filePath, out Tile tile))
                    continue;

                // Discard the tile if we shouldn't be generating maps for its world
                if (Worlds is not null && !Worlds.Contains(tile.World))
                    continue;

                // Discard the tile if we shouldn't be generating maps for its LOD
                if (normalisedLods is not null && !normalisedLods.Contains(tile.LOD))
                    continue;

                // Ensure that the world bucket exists
                if (!_worldLodBuckets.ContainsKey(tile.World))
                    _worldLodBuckets[tile.World] = new Dictionary<string, List<Tile>>();

                // Ensure that the LOD bucket exists
                if (!_worldLodBuckets[tile.World].ContainsKey(tile.LOD))
                    _worldLodBuckets[tile.World][tile.LOD] = new List<Tile>();

                _worldLodBuckets[tile.World][tile.LOD].Add(tile);
            }

            _console.MarkupLine("\t [lightgreen]Done[/]");

            _console.WriteLine("Maps will be generated for:");
            foreach (string world in _worldLodBuckets.Keys)
                _console.MarkupLine($"\t[yellow]{world}[/]: [fuchsia]{ string.Join(',', _worldLodBuckets[world].Keys) }[/]");

#if RELEASE
            _console.Confirm("Continue?");
#endif

            _console.WriteLine();
        }

        private void EnqueueStitchTasks(CancellationToken ct)
        {
            foreach (Dictionary<string, List<Tile>> worldBucket in _worldLodBuckets.Values)
            {
                foreach (List<Tile> lodBucket in worldBucket.Values)
                {
                    Task stitchTask = new(() =>
                    {
                        Tile referenceTile = lodBucket[0];
                        _console.MarkupLine($"Stitching tiles for [yellow]{referenceTile.World}[/] at [fuchsia]{referenceTile.LOD}[/]...");

                        IEnumerable<Tile> orderedBucket = lodBucket.OrderByDescending((b) => b.X).ThenBy((b) => b.Y);

                        // Allocate for the complete stitched image
                        int tilesPerSide = (int)Math.Sqrt(lodBucket.Count); // Square image, this will always be an integer
                        int pixelsPerSide = tilesPerSide * TILE_SIZE; // Each tile is 256x256 pixels
                        using Image<Rgba32> stitchedImage = new(pixelsPerSide, pixelsPerSide);

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
                        }

                        stitchedImage.Mutate(o => o.RotateFlip(RotateMode.Rotate90, FlipMode.Vertical));

#pragma warning disable CS8604 // Possible null reference argument.
                        string? outputDirectory = OutputPath ?? Path.GetDirectoryName(referenceTile.Path);
                        string outputFilePath = Path.Combine(outputDirectory, $"{referenceTile.World}_{referenceTile.LOD}.png");
#pragma warning restore CS8604 // Possible null reference argument.

                        stitchedImage.SaveAsPng(outputFilePath);
                        stitchedImage.Dispose();
                        _console.MarkupLine($"[lightgreen]Completed[/] stitching tiles for [yellow]{referenceTile.World}[/] at [fuchsia]{referenceTile.LOD}[/]...");

                        if (!DisableCompression)
                            EnqueueCompression(outputFilePath, ct);
                    }, ct, TaskCreationOptions.LongRunning);

                    _taskRunner.EnqueueTask(stitchTask);
                }
            }
        }

        private void EnqueueCompression(string filePath, CancellationToken ct)
        {
            if (!File.Exists(OPTIPNG_FILE_NAME))
            {
                _console.MarkupLine("[red]Compression failed:[/] OptiPNG cannot be found.");
                return;
            }

            Task compressionTask = new(() =>
            {
                _console.MarkupLine("Beginning compression on [aqua]" + Path.GetFileName(filePath) + "[/]");
                try
                {
                    Process? process = Process.Start(new ProcessStartInfo(OPTIPNG_FILE_NAME, filePath)
                    {
                        UseShellExecute = true
                    });

                    if (process is null)
                        return;

                    Task.Delay(500).Wait(); // Fixes issues with OptiPNG not starting fast enough on high LODs
                    process.WaitForExit();
                    _console.MarkupLine("[lightgreen]Completed[/] compressing [aqua]" + Path.GetFileName(filePath) + "[/]");
                }
                catch (Exception ex)
                {
                    _console.MarkupLine("[red]Compression failed:[/] " + ex.ToString());
                }
            }, ct, TaskCreationOptions.LongRunning);

            _taskRunner.EnqueueTask(compressionTask);
        }

        private static Image<Bgra32> LoadDDSImage(string filePath)
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
