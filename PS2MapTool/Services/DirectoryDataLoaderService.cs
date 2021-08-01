using PS2MapTool.Areas;
using PS2MapTool.Services.Abstractions;
using PS2MapTool.Tiles;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
{
    /// <summary>
    /// Provides functions to load mapping data from a directory.
    /// </summary>
    public sealed class DirectoryDataLoaderService : IDataLoaderService
    {
        private readonly string _directory;
        private readonly SearchOption _searchOption;

        /// <summary>
        /// Initialises a new instance of the <see cref="DirectoryDataLoaderService"/> object.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="searchSubdirectories"></param>
        public DirectoryDataLoaderService(string directory, SearchOption searchOption)
        {
            _directory = directory;
            _searchOption = searchOption;
        }

        /// <inheritdoc />
        /// <exception cref="DirectoryNotFoundException">Thrown when the supplied directory does not exist.</exception>
        public IEnumerable<TileInfo> GetTiles(World world, Lod lod, CancellationToken ct = default)
        {
            if (!Directory.Exists(_directory))
                throw new DirectoryNotFoundException("The supplied directory does not exist: " + _directory);

            EnumerationOptions enumerationOptions = new()
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = _searchOption == SearchOption.AllDirectories
            };
            string searchPattern = world.ToString() + "_Tile_???_???_" + lod.ToString();

            foreach (string path in Directory.EnumerateFiles(_directory, searchPattern, enumerationOptions))
            {
                FileStream fs = new(path, FileMode.Open, FileAccess.Read);

                if (TileInfo.TryParse(Path.GetFileNameWithoutExtension(path), fs, out TileInfo? tile))
                    yield return tile;
                else
                    fs.Dispose();
            }
        }

        /// <inheritdoc />
        /// <exception cref="DirectoryNotFoundException">Thrown when the supplied directory does not exist.</exception>
        /// <exception cref="FileNotFoundException">Thrown when an areas file could not be found.</exception>
        public async Task<AreasSourceInfo> GetAreasAsync(World world, CancellationToken ct = default)
        {
            if (!Directory.Exists(_directory))
                throw new DirectoryNotFoundException("The supplied directory does not exist: " + _directory);

            string fileName = world.ToString() + "Areas.xml";

            string? filePath = await Task.Run(() =>
            {
                if (_searchOption == SearchOption.AllDirectories)
                {
                    return RecursiveFileSearch(fileName, _directory, ct);
                }
                else
                {
                    string path = Path.Combine(_directory, fileName);
                    if (File.Exists(path))
                        return path;
                    else
                        return null;
                }
            }, ct).ConfigureAwait(false);

            if (filePath is null)
                throw new FileNotFoundException("The Areas file for this world could not be found.");
            else
                return new AreasSourceInfo(world, new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        /// <summary>
        /// Recursively searches a directory, and all subdirectories, for a file. Faster than searching for a pattern as not all files are enumeration.
        /// </summary>
        /// <param name="fileName">The name of the file to find (including the extension).</param>
        /// <param name="directory">The root directory to search.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the search with.</param>
        /// <returns>The path to the file, or a null value if the file could not be found.</returns>
        /// <remarks>The search is performed in a depth-first manner.</remarks>
        private static string? RecursiveFileSearch(string fileName, string directory, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
                return null;

            string? filePath = Path.Combine(directory, fileName);

            if (File.Exists(filePath))
                return filePath;

            foreach (string subdirectory in Directory.EnumerateDirectories(directory))
            {
                filePath = RecursiveFileSearch(fileName, subdirectory, ct);
                if (filePath is not null)
                    return filePath;
            }

            return null;
        }
    }
}
