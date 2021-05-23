using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using ImageMagick;
using PS2MapTools.Validators;
using System;
using System.Collections.Concurrent;
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

        private readonly Dictionary<string, Dictionary<string, List<Tile>>> _worldLodBuckets = new();
        private readonly ConcurrentQueue<Action<IConsole, CancellationToken>> _taskQueue;

        private bool _noTasksToEnqueue;
        private Task _taskRunner;

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

        public StitchCommand()
        {
            TilesSource = string.Empty;
            MaxParallelism = 4;
            _taskQueue = new ConcurrentQueue<Action<IConsole, CancellationToken>>();
            _taskRunner = new Task(() => throw new InvalidOperationException("This task runner has not been setup."));
        }

        public ValueTask ExecuteAsync(IConsole console)
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

            CancellationToken ct = console.RegisterCancellationHandler();
            SetupTaskRunner(console, ct);

            GenerateWorldLodBuckets(console);
            EnqueueStitchTasks();

            _noTasksToEnqueue = true;
            // Job done, wait for all tasks to complete.
            _taskRunner.Wait();

            return default;
        }

        /// <summary>
        /// Initialises and starts the <see cref="_taskRunner"/>.
        /// </summary>
        /// <param name="console">The console instance for tasks to use.</param>
        /// <param name="ct"></param>
        private void SetupTaskRunner(IConsole console, CancellationToken ct)
        {
            // TODO: Refactor logic to new class

            _taskRunner = new Task(() =>
            {
                List<Task> cachedTasks = new();

                int taskCount = 0;
                while ((!_noTasksToEnqueue || !_taskQueue.IsEmpty) && !ct.IsCancellationRequested)
                {
                    while (taskCount < MaxParallelism && _taskQueue.TryDequeue(out Action<IConsole, CancellationToken>? a))
                    {
                        Task t = new(() => a.Invoke(console, ct), TaskCreationOptions.LongRunning);
                        t.ContinueWith((t) =>
                        {
                            cachedTasks.Remove(t);
                            Interlocked.Decrement(ref taskCount);
                        });
                        cachedTasks.Add(t);
                        t.Start();

                        Interlocked.Increment(ref taskCount);
                        // Cache tasks and wait on them completing before exiting
                    }

                    Task.Delay(100).Wait();
                }

                Task.WhenAll(cachedTasks).Wait();
            }, TaskCreationOptions.LongRunning);
            _taskRunner.Start();
        }

        /// <summary>
        /// Filters through all files in the source directory and sorts tiles into applicable <see cref="_worldLodBuckets"/>.
        /// </summary>
        private void GenerateWorldLodBuckets(IConsole console)
        {
            console.Output.Write("Generating map buckets...");
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

            console.Output.WriteLine("\t Done");
            console.Output.WriteLine("Generating maps for:");
            foreach (string world in _worldLodBuckets.Keys)
                console.Output.WriteLine("\t" + world + ": " + string.Join(',', _worldLodBuckets[world].Keys));
            console.Output.WriteLine();
        }

        private void EnqueueStitchTasks()
        {
            // Each tile is 256x256 pixels, regardless of the LOD
            MagickGeometry tileGeometry = new(256);

            foreach (Dictionary<string, List<Tile>> worldBucket in _worldLodBuckets.Values)
            {
                foreach (List<Tile> lodBucket in worldBucket.Values)
                {
                    _taskQueue.Enqueue((c, _) =>
                    {
                        Tile referenceTile = lodBucket[0];
                        c.Output.WriteLine($"Stitching tiles for world {referenceTile.World} at LOD {referenceTile.LOD}...");

                        IEnumerable<Tile> orderedBucket = lodBucket.OrderByDescending((b) => b.X).ThenBy((b) => b.Y);

                        using MagickImageCollection images = new();

                        foreach (Tile tile in orderedBucket)
                        {
                            MagickImage image = new(tile.Path);
                            image.Rotate(270);
                            images.Add(image);
                        }

                        using IMagickImage<float> mosaic = images.Montage(new MontageSettings
                        {
                            BorderWidth = 0,
                            Geometry = tileGeometry
                        });
                        mosaic.Rotate(90);
                        mosaic.Flip();

#pragma warning disable CS8604 // Possible null reference argument.
                        string? outputDirectory = OutputPath ?? Path.GetDirectoryName(referenceTile.Path);
                        string outputFilePath = Path.Combine(outputDirectory, $"{referenceTile.World}_{referenceTile.LOD}.png");
#pragma warning restore CS8604 // Possible null reference argument.

                        mosaic.Write(outputFilePath, MagickFormat.Png);
                        c.Output.WriteLine($"Completed stitching tiles for world {referenceTile.World} at LOD {referenceTile.LOD}");
                        c.Output.WriteLine("Wrote output file: " + outputFilePath);

                        //if (!DisableCompression)
                        //    EnqueueCompression(outputFilePath);
                    });
                }
            }
        }

        private bool EnqueueCompression(string filePath)
        {
            if (!File.Exists(OPTIPNG_FILE_NAME))
                return false;

            Console.WriteLine("Beginning compression on " + filePath);
            try
            {
                Process? process = Process.Start(new ProcessStartInfo(OPTIPNG_FILE_NAME, filePath)
                {
                    UseShellExecute = true
                });

                if (process is null)
                    return false;

                process.Exited += (s, e) => Console.WriteLine("Completed compressing " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Compression failed: " + ex.ToString());
                return false;
            }

            return true;
        }
    }
}
