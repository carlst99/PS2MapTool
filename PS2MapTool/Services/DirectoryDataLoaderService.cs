using PS2MapTool.Areas;
using PS2MapTool.Abstractions.Services;
using PS2MapTool.Tiles;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Win32.SafeHandles;
using PS2MapTool.Abstractions.Tiles;

namespace PS2MapTool.Services;

/// <summary>
/// Implements a <see cref="IDataLoaderService"/> that loads mapping data from a directory.
/// </summary>
public class DirectoryDataLoaderService : IDataLoaderService
{
    protected readonly string _directory;
    protected readonly SearchOption _searchOption;

    /// <summary>
    /// Initialises a new instance of the <see cref="DirectoryDataLoaderService"/> object.
    /// </summary>
    /// <param name="directory">The directory to search for files in.</param>
    /// <param name="searchOption">The search option to use.</param>
    public DirectoryDataLoaderService(string directory, SearchOption searchOption)
    {
        _directory = directory;

        if (!Enum.IsDefined(searchOption))
            throw new ArgumentException("The provided value is not a valid enum member", nameof(searchOption));

        _searchOption = searchOption;
    }

    /// <inheritdoc />
    /// <exception cref="DirectoryNotFoundException">Thrown when the supplied directory does not exist.</exception>
    public virtual Task<IReadOnlyList<ITileDataSource>> GetTilesAsync
    (
        string worldName,
        Lod lod,
        CancellationToken ct = default
    )
    {
        if (!Directory.Exists(_directory))
            throw new DirectoryNotFoundException("The supplied directory does not exist: " + _directory);

        EnumerationOptions enumerationOptions = new()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = _searchOption is SearchOption.AllDirectories
        };

        string searchPattern = worldName + $"_Tile_???_???_{lod}.*";
        List<FileTileDataSource> tiles = new();

        foreach (string path in Directory.EnumerateFiles(_directory, searchPattern, enumerationOptions))
        {
            ct.ThrowIfCancellationRequested();

            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();

            bool canParseName = TileHelpers.TryParseName
            (
                Path.GetFileName(path),
                out string? tileWorldName,
                out int x,
                out int y,
                out Lod tileLod,
                out string? fileExtension
            );

            if (!canParseName)
                continue;

            SafeFileHandle outputHandle = File.OpenHandle
            (
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileOptions.Asynchronous
            );
            tiles.Add(new FileTileDataSource(tileWorldName!, x, y, tileLod, fileExtension!, outputHandle));
        }

        return Task.FromResult((IReadOnlyList<ITileDataSource>)tiles);
    }

    /// <inheritdoc />
    /// <exception cref="DirectoryNotFoundException">Thrown when the supplied directory does not exist.</exception>
    /// <exception cref="FileNotFoundException">Thrown when an areas file could not be found.</exception>
    public virtual async Task<AreasSourceInfo> GetAreasAsync(string worldName, CancellationToken ct = default)
    {
        if (!Directory.Exists(_directory))
            throw new DirectoryNotFoundException("The supplied directory does not exist: " + _directory);

        string fileName = worldName + "Areas.xml";

        string? filePath = await Task.Run
        (
            () =>
            {
                if (_searchOption == SearchOption.AllDirectories)
                    return RecursiveFileSearch(fileName, _directory, ct);

                string path = Path.Combine(_directory, fileName);

                return File.Exists(path)
                    ? path
                    : null;
            },
            ct
        ).ConfigureAwait(false);

        if (filePath is null)
            throw new FileNotFoundException("The Areas file for this world could not be found.");

        return new AreasSourceInfo(worldName, new FileStream(filePath, FileMode.Open, FileAccess.Read));
    }

    /// <summary>
    /// Recursively searches a directory, and all subdirectories, for a single file.
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
