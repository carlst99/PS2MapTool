using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PS2MapBuilder
{
    public static class Program
    {
        private const string OPTIPNG_FILE_NAME = "optipng.exe";

        private static readonly Dictionary<string, Dictionary<string, List<Tile>>> _worldLodBuckets = new();

        public static void Main(string[] args)
        {
            bool noCompression = false;
            bool parallelCompression = true;

            if (args.Length < 1)
                WriteUsage();

            if (!Directory.Exists(args[0]))
                WriteUsage();

            if (args.Contains("-nc"))
                noCompression = true;

            if (args.Contains("-npc"))
                parallelCompression = false;

            foreach (string filePath in Directory.EnumerateFiles(args[0]))
            {
                if (!Tile.TryParse(filePath, out Tile tile))
                    continue;

                if (!_worldLodBuckets.ContainsKey(tile.World))
                    _worldLodBuckets[tile.World] = new Dictionary<string, List<Tile>>();

                if (!_worldLodBuckets[tile.World].ContainsKey(tile.LOD))
                    _worldLodBuckets[tile.World][tile.LOD] = new List<Tile>();

                _worldLodBuckets[tile.World][tile.LOD].Add(tile);
            }

            MagickGeometry tileGeometry = new(256);

            foreach (Dictionary<string, List<Tile>> worldBucket in _worldLodBuckets.Values)
            {
                foreach (List<Tile> lodBucket in worldBucket.Values)
                {
                    Tile referenceTile = lodBucket[0];
                    Console.WriteLine($"Stitching tiles for world {referenceTile.World} at LOD {referenceTile.LOD}");

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
                    string outputFilePath = Path.Combine(Path.GetDirectoryName(referenceTile.Path), $"{referenceTile.World}_{referenceTile.LOD}.png");
#pragma warning restore CS8604 // Possible null reference argument.

                    mosaic.Write(outputFilePath, MagickFormat.Png);
                    Console.WriteLine("Wrote output file: " + outputFilePath);

                    if (!noCompression)
                        TryCompress(outputFilePath, parallelCompression);
                }
            }
        }

        private static bool TryCompress(string filePath, bool parallelCompression)
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

                if (!parallelCompression)
                    process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Compression failed: " + ex.ToString());
                return false;
            }

            return true;
        }

        private static void WriteUsage()
        {
            Console.WriteLine("Usage: PS2MapBuilder.exe [-h | OPTIONS] {MODE} <ARGUMENTS> [-nc] [-npc]" +
                "\n\n{MODES}" +
                "\n\n\textract" +
                "\n\t\tAttempts to extract map LODs from the game client assets.");

                string temp = "\nSubdirectories will not be searched. Maps will be generated for all worlds/LODs available." +
                "\n\t-nc\tExplicitly disables compression. Compression will not be performed if optipng.exe is not alongside the PS2MapBuilder executable." +
                "\n\t-npc\tPrevents parallel compression.";

            Environment.Exit(0);
        }
    }
}
