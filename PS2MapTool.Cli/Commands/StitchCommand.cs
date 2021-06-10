using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using PS2MapTool.Cli.Models;
using PS2MapTool.Cli.Services;
using PS2MapTool.Cli.Validators;
using SixLabors.ImageSharp;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Cli.Commands
{
    [Command("stitch", Description = "Stitches LOD tiles together to form a complete map. Maps will be created for all World/LOD combinations found in the source directory, unless otherwise specified.")]
    public class StitchCommand : ICommand
    {
        private readonly Stopwatch _stopwatch;

        private ParallelTaskRunner _taskRunner;
        private IAnsiConsole _console;
        private CancellationToken _ct;

        #region Command Parameters/Options

        [CommandParameter(0, Description = "The path to the directory containing the LOD tiles. Each tile should be named in the format <World>_Tile_<Y>_<X>_LOD*.")]
        public string TilesSource { get; init; }

        [CommandOption("output", 'o', Description = "The path to output the stitched map/s to.")]
        public string OutputPath { get; private set; }

        [CommandOption("disable-compression", 'd', Description = "Prevents the stitched map/s from being compressed using OptiPNG. Saves a considerable amount of time at the expense of producing maps 40-50% larger in size.", IsRequired = false)]
        public bool DisableCompression { get; init; }

        [CommandOption("max-parallelism", 'p', Description = "The maximum amount of maps that may be stitched AND compressed in parallel. Lower values use less memory and CPU resouces.", Validators = new Type[] { typeof(MaxParallelValidator) })]
        public int MaxParallelism { get; init; }

        [CommandOption("worlds", 'w', Description = "Limits map generation to the given worlds.")]
        public IReadOnlyList<string>? Worlds { get; init; }

        [CommandOption("lods", 'l', Description = "Limits map generation to the given LODs", Validators = new Type[] { typeof(LODNumberValidator) })]
        public IReadOnlyList<int>? Lods { get; init; }

        #endregion

        public StitchCommand()
        {
            _stopwatch = new Stopwatch();

            TilesSource = string.Empty;
            OutputPath = string.Empty;
            MaxParallelism = 4;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            Setup(console);

            _stopwatch.Start();

            WorldLodBucket? tileBucket = GetTileBuckets();
            if (tileBucket is null)
                return;

            StitchService stitchService = new(_console, _taskRunner);
            CompressionService compressionService = new(_console, _taskRunner);

            stitchService.StitchTiles(tileBucket, OutputPath, _ct, (s) => compressionService.Compress(s, _ct));

            // Job done, wait for all tasks to complete.
            await _taskRunner.WaitForAll().ConfigureAwait(false);
            _taskRunner.Stop();

            _stopwatch.Stop();
            _console.WriteLine();
            _console.MarkupLine(Formatter.Success("Completed in " + _stopwatch.Elapsed.ToString(@"hh\h\ mm\m\ ss\s")));
        }

        private void Setup(IConsole console)
        {
            if (!Directory.Exists(TilesSource))
                throw new CommandException("The provided tiles source directory does not exist.");

            if (!string.IsNullOrWhiteSpace(OutputPath) && !Directory.Exists(OutputPath))
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
            else
            {
                OutputPath = TilesSource;
            }

            _console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(console.Output)
            });

            _ct = console.RegisterCancellationHandler();

            _taskRunner = new ParallelTaskRunner((e) => console.Error.WriteLine(e));
            _taskRunner.Start(_ct, MaxParallelism);
        }

        /// <summary>
        /// Filters through all files in the source directory and sorts tiles into applicable <see cref="_worldLodBuckets"/>.
        /// </summary>
        private WorldLodBucket? GetTileBuckets()
        {
            _console.Write("Generating tile buckets...");
            WorldLodBucket bucket = StitchService.GenerateTileBuckets(TilesSource, Worlds, Lods);
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
}
