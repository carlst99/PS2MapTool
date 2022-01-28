using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using PS2MapTool.Abstractions.Services;
using PS2MapTool.Areas;
using PS2MapTool.Services;
using PS2MapTool.Services.Abstractions;
using SixLabors.ImageSharp;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Cli.Commands;

[Command("areas", Description = "Extract no-deploy zones.")]
public class AreasCommand : ICommand
{
    private readonly Stopwatch _stopwatch;
    private readonly IAreasService _areasService;

    private IDataLoaderService _dataLoaderService;
    private IAnsiConsole _console;
    private CancellationToken _ct;

    #region Command Options/Parameters

    [CommandParameter(0, Description = "The directory containing an areas file. Areas files should be named in the format <World>Areas.xml")]
    public string AreasFileSource { get; init; }

    [CommandOption('o', Description = "The path to output the compiled no-deploy-zone images to.")]
    public string OutputPath { get; set; }

    [CommandOption("worlds", 'w', Description = "Limits no-deploy-zone generation to the given worlds.")]
    public IReadOnlyList<AssetZone>? Worlds { get; set; }

    [CommandOption("lods", 'l', Description = "Limits no-deploy-zone generation to the given LODs.")]
    public IReadOnlyList<Lod>? Lods { get; set; }

    [CommandOption("ndz-type", 't', Description = "Limits the type of no-deploy-zones that are generated to the given types.")]
    public IReadOnlyList<NoDeployType>? NoDeployTypes { get; set; }

    #endregion

    public AreasCommand()
    {
        _stopwatch = new Stopwatch();
        _areasService = new AreasService();

        AreasFileSource = string.Empty;
        OutputPath = string.Empty;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        Setup(console);
        _stopwatch.Start();

        foreach (AssetZone world in Worlds!)
        {
            AreasSourceInfo? areasSourceInfo = null;
            try
            {
                areasSourceInfo = await _dataLoaderService.GetAreasAsync(world.ToString(), _ct).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                _console.MarkupLine(Formatter.Warning($"Areas file for the world { Formatter.World(world) } could not be found."));
                continue;
            }

            foreach (NoDeployType type in NoDeployTypes!)
            {
                IList<AreaDefinition> noDeployZones = await _areasService.GetNoDeployAreasAsync(areasSourceInfo!, type, _ct).ConfigureAwait(false);
                if (noDeployZones.Count == 0)
                {
                    _console.MarkupLine(Formatter.Warning($"The areas file for { Formatter.World(world) } does not contain any no-deploy-zones of the type { Formatter.NdzType(type) }."));
                    continue;
                }

                foreach (Lod lod in Lods!)
                {
                    _console.MarkupLine($"Creating { Formatter.NdzType(type) } no-deploy-zone image for { Formatter.World(world) } at { Formatter.Lod(lod) }...");

                    using Image ndzImage = await _areasService.CreateNoDeployZoneImageAsync(noDeployZones, lod, _ct).ConfigureAwait(false);

                    string outputPath = Path.Combine(OutputPath, $"{world}_{type}_{lod}.png");
                    await ndzImage.SaveAsPngAsync(outputPath, _ct).ConfigureAwait(false);

                    _console.MarkupLine($"Image saved to { Formatter.Path(outputPath) }");
                }
            }

            areasSourceInfo.Dispose();
        }

        _console.MarkupLine(Formatter.Success("Completed in " + _stopwatch.Elapsed.ToString(@"hh\h\ mm\m\ ss\s")));
        _stopwatch.Reset();
    }

    private void Setup(IConsole console)
    {
        if (!Directory.Exists(AreasFileSource))
            throw new CommandException("The provided areas source directory does not exist.");

        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            OutputPath = AreasFileSource;
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
        _dataLoaderService = new DirectoryDataLoaderService(AreasFileSource, SearchOption.AllDirectories);

        Worlds ??= Enum.GetValues<AssetZone>();
        Lods ??= Enum.GetValues<Lod>();
        NoDeployTypes ??= Enum.GetValues<NoDeployType>();

        _console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(console.Output)
        });
    }
}
