using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using PS2MapTool.Cli.Validators;
using PS2MapTool.Services;
using PS2MapTool.Services.Abstractions;
using PS2MapTool.Tiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Cli.Commands
{
    [Command("stitch", Description = "Stitch LOD tiles together to form a complete map. Maps will be created for all World/LOD combinations found in the source directory, unless otherwise specified.")]
    public class StitchCommand : ICommand
    {
        private readonly Stopwatch _stopwatch;
        private readonly ParallelTaskRunner _taskRunner;
        private readonly IImageStitchService _imageStitchService;
        private readonly IImageCompressionService _compressionService;

        private IDataLoaderService _dataLoader;
        private IAnsiConsole _console;
        private CancellationToken _ct;

        #region Command Parameters/Options

        [CommandParameter(0, Description = "The path to the directory containing the LOD tiles. Each tile should be named in the format <World>_Tile_<Y>_<X>_LOD*.")]
        public string TilesSource { get; init; }

        [CommandOption("output", 'o', Description = "The path to output the stitched map/s to.")]
        public string OutputPath { get; private set; }

        [CommandOption("disable-compression", 'd', Description = "Prevents the stitched map/s from being compressed using OptiPNG. Saves a considerable amount of time at the expense of producing maps 40-50% larger in size.", IsRequired = false)]
        public bool DisableCompression { get; init; }

        [CommandOption("max-parallelism", 'p', Description = "The maximum amount of maps that may be stitched AND compressed in parallel. Lower values use less memory and CPU resources.", Validators = new Type[] { typeof(MaxParallelValidator) })]
        public int MaxParallelism { get; init; }

        [CommandOption("worlds", 'w', Description = "Limits map generation to the given worlds.")]
        public IReadOnlyList<World>? Worlds { get; set; }

        [CommandOption("lods", 'l', Description = "Limits map generation to the given LODs")]
        public IReadOnlyList<Lod>? Lods { get; set; }

        #endregion

        public StitchCommand()
        {
            _stopwatch = new Stopwatch();
            _taskRunner = new ParallelTaskRunner();
            _compressionService = new OptiPngCompressionService();

            TileProcessorServiceRepository repo = new();
            repo.Add(TileImageFormatType.DDS, new DdsTileProcessorService());
            repo.Add(TileImageFormatType.PNG, new PngTileProcessorService());
            _imageStitchService = new ImageStitchService(repo);

            TilesSource = string.Empty;
            OutputPath = string.Empty;
            MaxParallelism = 4;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            Setup(console);

            _stopwatch.Start();

            TileBucket? tileBucket = GetTileBucket();
            if (tileBucket is null)
                return;

            foreach (World world in tileBucket.GetWorlds())
            {
                foreach (Lod lod in tileBucket.GetLods(world))
                {
                    Task t = new(async () =>
                    {
                        _console.MarkupLine($"Stitching tiles for { Formatter.World(world) } at { Formatter.Lod(lod) }...");

                        IList<TileInfo> tiles = tileBucket.GetTiles(world, lod);
                        using Image<Rgba32> map = await _imageStitchService.StitchTilesAsync(tiles, _ct).ConfigureAwait(false);

                        _console.MarkupLine($"Saving { Formatter.World(world) } at { Formatter.Lod(lod) }...");

                        // Save the stitched image.
                        string outputFilePath = Path.Combine(OutputPath, $"{world}_{lod}.png");
                        map.SaveAsPng(outputFilePath);
                        _console.MarkupLine($"{ Formatter.Success("Completed") } saving { Formatter.World(world) } at { Formatter.Lod(lod) } to { Formatter.Path(outputFilePath) }");

                        if (!DisableCompression)
                        {
                            _console.MarkupLine($"Compressing { Formatter.World(world) } at { Formatter.Lod(lod) }...");
                            await _compressionService.CompressAsync(outputFilePath, _ct).ConfigureAwait(false);
                            _console.MarkupLine($"{ Formatter.Success("Completed") } compressing { Formatter.World(world) } at { Formatter.Lod(lod) }");
                        }
                    }, _ct, TaskCreationOptions.LongRunning);
                    _taskRunner.EnqueueTask(t);
                }
            }

            // Job done, wait for all tasks to complete.
            await _taskRunner.WaitForAll().ConfigureAwait(false);
            _taskRunner.Stop();

            _console.WriteLine();
            _console.MarkupLine(Formatter.Success("Completed in " + _stopwatch.Elapsed.ToString(@"hh\h\ mm\m\ ss\s")));
            _stopwatch.Reset();
        }

        private void Setup(IConsole console)
        {
            if (!Directory.Exists(TilesSource))
                throw new CommandException("The provided tiles source directory does not exist.");

            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                OutputPath = TilesSource;
            }
            else if (!Directory.Exists(OutputPath))
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

            _ct = console.RegisterCancellationHandler();
            _dataLoader = new DirectoryDataLoaderService(TilesSource, SearchOption.AllDirectories);

            Worlds ??= Enum.GetValues<World>();
            Lods ??= Enum.GetValues<Lod>();

            _console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(console.Output)
            });

            _taskRunner.Start(_ct, MaxParallelism, (e) => console.Error.WriteLine(e));
        }

        /// <summary>
        /// Filters through all files in the source directory and sorts tiles into applicable <see cref="_worldLodBuckets"/>.
        /// </summary>
        private TileBucket? GetTileBucket()
        {
            _console.Write("Generating tile buckets...");
            TileBucket bucket = new();

            foreach (World w in Worlds!)
            {
                foreach (Lod l in Lods!)
                {
                    IEnumerable<TileInfo> tiles = _dataLoader.GetTiles(w, l, _ct);
                    bucket.AddTiles(tiles);
                }
            }
            _console.MarkupLine("\t " + Formatter.Success("Done"));

            _console.WriteLine("Maps will be generated for:");
            foreach (World world in bucket.GetWorlds())
                _console.MarkupLine($"\t{ Formatter.World(world) }: { Formatter.Lod(string.Join(',', bucket.GetLods(world))) }");

            _console.WriteLine();

            if (_console.Confirm("Continue?"))
                return bucket;
            else
                return null;
        }
    }
}
