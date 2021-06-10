using PS2MapTool.Core.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Core.Services
{
    /// <summary>
    /// Provides functions to load mapping data from a directory.
    /// </summary>
    public sealed class DirectoryDataLoaderService : IDataLoaderService
    {
        private readonly string _directory;
        private readonly bool _searchSubdirectories;

        /// <summary>
        /// Initialises a new instance of the <see cref="DirectoryDataLoaderService"/> object.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="searchSubdirectories"></param>
        public DirectoryDataLoaderService(string directory, bool searchSubdirectories)
        {
            _directory = directory;
            _searchSubdirectories = searchSubdirectories;
        }

        /// <inheritdoc />
        public IAsyncEnumerable<TileInfo> GetTilesAsync(World world, Lod lod, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<AreasInfo> GetAreasInfoAsync(World world, CancellationToken ct = default)
        {
            if (!Directory.Exists(_directory))
                throw new DirectoryNotFoundException("The supplied directory does not exist.");

            string fileName = world.ToString() + "Areas.xml";

            string? filePath = await Task.Run(() =>
            {
                if (_searchSubdirectories)
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
                return new AreasInfo(world, new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        /// <summary>
        /// Recursively searches a directory, and all subdirectories, for a file.
        /// </summary>
        /// <param name="fileName">The name of the file to find (including the extension).</param>
        /// <param name="directory">The root directory to search.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the search with.</param>
        /// <returns>The path to the file, or a null value if the file could not be found.</returns>
        private string? RecursiveFileSearch(string fileName, string directory, CancellationToken ct = default)
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
