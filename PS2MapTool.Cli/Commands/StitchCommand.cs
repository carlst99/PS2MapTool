using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using PS2MapTool.Abstractions.Services;
using PS2MapTool.Abstractions.Tiles;
using PS2MapTool.Abstractions.Tiles.Services;
using PS2MapTool.Cli.Validators;
using PS2MapTool.Services;
using PS2MapTool.Tiles;
using PS2MapTool.Tiles.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Cli.Commands;

[Command("stitch", Description = "Stitch LOD tiles together to form a complete map. Maps will be created for all World/LOD combinations found in the source directory, unless otherwise specified.")]
public class StitchCommand : ICommand
{
    private readonly Stopwatch _stopwatch;
    private readonly ITileStitchService _imageStitchService;
    private readonly IImageCompressionService _compressionService;

    private IDataLoaderService _dataLoader;
    private IAnsiConsole _console;
    private CancellationToken _ct;

    #region Command Parameters/Options

    [CommandParameter(0, Description = "The path to the directory containing either pack2, OR pre-extracted LOD tiles.")]
    public string TilesSource { get; init; }

    [CommandOption("output", 'o', Description = "The path to output the stitched map/s to.")]
    public string OutputPath { get; private set; }

    [CommandOption("disable-compression", 'd', Description = "Prevents the stitched map/s from being compressed using OptiPNG. Saves a considerable amount of time at the expense of producing maps 30-40% larger in size.", IsRequired = false)]
    public bool DisableCompression { get; init; }

    [CommandOption("max-parallelism", 'p', Description = "The maximum amount of maps that may be stitched AND compressed in parallel. Lower values use less memory and CPU resources.", Validators = new[] { typeof(MaxParallelValidator) })]
    public int MaxParallelism { get; init; }

    [CommandOption("worlds", 'w', Description = "Limits map generation to the given worlds.")]
    public IReadOnlyList<AssetZone>? Worlds { get; set; }

    [CommandOption("lods", 'l', Description = "Limits map generation to the given LODs")]
    public IReadOnlyList<Lod>? Lods { get; set; }

    #endregion

    public StitchCommand()
    {
        _stopwatch = new Stopwatch();
        _compressionService = new OptiPngCompressionService();

        TileLoaderServiceRepository repo = new();
        repo.Add(new DdsTileLoaderService());
        repo.Add(new PngTileLoaderService());
        _imageStitchService = new TileStitchService(repo);

        TilesSource = string.Empty;
        OutputPath = string.Empty;
        MaxParallelism = 4;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        Setup(console);

        using TileBucket? tileBucket = await GetTileBucketAsync();
        if (tileBucket is null)
            return;

        _stopwatch.Start();
        foreach (string world in tileBucket.GetWorlds())
        {
            await Parallel.ForEachAsync
            (
                tileBucket.GetLods(world),
                new ParallelOptions
                {
                    CancellationToken = _ct,
                    MaxDegreeOfParallelism = MaxParallelism
                },
                async (lod, ct) =>
                {
                    _console.MarkupLine($"Stitching tiles for { Formatter.World(world) } at { Formatter.Lod(lod) }...");

                    string outputFilePath = Path.Combine(OutputPath, $"{world}_{lod}.png");
                    IList<ITileDataSource> tiles = tileBucket.GetTiles(world, lod);

                    using (Image<Rgba32> map = await _imageStitchService.StitchAsync(tiles, ct))
                    {
                        // Preemptively clean up tiles so we aren't holding on to more memory than necessary
                        foreach (ITileDataSource tile in tiles)
                        {
                            if (tile is IDisposable disposable)
                                disposable.Dispose();
                        }

                        // Save the stitched image.
                        _console.MarkupLine($"Saving { Formatter.World(world) } at { Formatter.Lod(lod) }...");
                        await map.SaveAsPngAsync(outputFilePath, ct);

                        _console.MarkupLine($"{ Formatter.Success("Completed") } saving { Formatter.World(world) } at { Formatter.Lod(lod) } to { Formatter.Path(outputFilePath) }");
                    }

                    if (!DisableCompression)
                    {
                        _console.MarkupLine($"Compressing { Formatter.World(world) } at { Formatter.Lod(lod) }...");
                        await _compressionService.CompressAsync(outputFilePath, ct);
                        _console.MarkupLine($"{ Formatter.Success("Completed") } compressing { Formatter.World(world) } at { Formatter.Lod(lod) }");
                    }
                }
            );
        }

        _console.WriteLine();
        _console.MarkupLine(Formatter.Success("Completed in " + _stopwatch.Elapsed));
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

        if (File.Exists(Path.Combine(TilesSource, "data_x64_0.pack2")))
            _dataLoader = new PackDataLoaderService(TilesSource);
        else
            _dataLoader = new DirectoryDataLoaderService(TilesSource, SearchOption.AllDirectories);

        Worlds ??= Enum.GetValues<AssetZone>();
        Lods ??= Enum.GetValues<Lod>();

        _console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(console.Output)
        });
    }

    /// <summary>
    /// Filters through all files in the source directory and sorts tiles into a <see cref="TileBucket"/>.
    /// </summary>
    private async Task<TileBucket?> GetTileBucketAsync()
    {
        _console.Write("Generating tile buckets...");
        TileBucket bucket = new();

        foreach (AssetZone w in Worlds!)
        {
            foreach (Lod l in Lods!)
            {
                IReadOnlyList<ITileDataSource> tiles = await _dataLoader.GetTilesAsync(w.ToString(), l, _ct);
                bucket.AddTiles(tiles);
            }
        }
        _console.MarkupLine("\t " + Formatter.Success("Done"));

        _console.WriteLine("Maps will be generated for:");
        foreach (string world in bucket.GetWorlds())
            _console.MarkupLine($"\t{ Formatter.World(world) }: { Formatter.Lod(string.Join(',', bucket.GetLods(world))) }");

        _console.WriteLine();

        if (_console.Confirm("Continue?"))
            return bucket;
        else
            return null;
    }
}
