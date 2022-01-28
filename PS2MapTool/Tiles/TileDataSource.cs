using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PS2MapTool.Tiles;

/// <summary>
/// Represents a tile data source.
/// </summary>
/// <param name="WorldName">The world that this tile belongs to.</param>
/// <param name="X">The X coordinate of the tile.</param>
/// <param name="Y">The Y coordinate of the tile.</param>
/// <param name="Lod">The level of detail that this tile is for.</param>
/// <param name="FileExtension">The file extension of the tile data source.</param>
/// <param name="Data">The tile data.</param>
public record TileDataSource
(
    string WorldName,
    int X,
    int Y,
    Lod Lod,
    string FileExtension,
    MemoryOwner<byte> Data
) : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether or not this <see cref="TileDataSource"/> instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Attempts to initialise a tile object with values parsed from the given name.
    /// You must fill the <see cref="Data"/> property before using the parsed <see cref="TileDataSource"/>.
    /// </summary>
    /// <param name="tileName">The name of the tile data source.</param>
    /// <param name="tile">The parsed tile, or null if the operation was unsuccessful.</param>
    /// <returns>A value indicating if the operation was successful.</returns>
    public static bool TryParseName(string tileName, [NotNullWhen(true)] out TileDataSource? tile)
    {
        tile = null;

        // Get the extension, if available
        string[] extensionComponents = tileName.Split('.');
        string[] nameComponents = extensionComponents[0].Split('_');

        if (extensionComponents.Length != 2)
            return false;
        string fileExtension = extensionComponents[1];

        int tileComponentIndex = Array.IndexOf(nameComponents, "Tile");
        if (tileComponentIndex == -1)
            return false;

        if (nameComponents.Length != 4 + tileComponentIndex)
            return false;

        if (nameComponents[tileComponentIndex++] != "Tile")
            return false;

        if (!int.TryParse(nameComponents[tileComponentIndex++], out int x))
            return false;

        if (!int.TryParse(nameComponents[tileComponentIndex++], out int y))
            return false;

        if (!Enum.TryParse(nameComponents[tileComponentIndex++], true, out Lod lod))
            return false;

        string name = string.Join("_", nameComponents[0..tileComponentIndex]);
        tile = new TileDataSource(name, x, y, lod, fileExtension, null!);

        return true;
    }

    public override string ToString()
        => $"{WorldName}_Tile_{Y}_{X}_{Lod}";

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposedManaged">A value indicating whether or not to dispose of managed resources.</param>
    protected virtual void Dispose(bool disposedManaged)
    {
        if (IsDisposed)
            return;

        if (disposedManaged)
        {
            Data.Dispose();
        }

        IsDisposed = true;
    }
}
