using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PS2MapTool.Tiles;

/// <summary>
/// Contains information about a tile data source.
/// </summary>
public record TileInfo : IDisposable
{
    /// <summary>
    /// The data source.
    /// </summary>
    public Stream DataSource { get; init; }

    /// <summary>
    /// The world that this tile belongs to.
    /// </summary>
    public string World { get; init; }

    /// <summary>
    /// The X coordinate of the tile.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    /// The Y coordinate of the tile.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    /// The level of detail that this tile is for.
    /// </summary>
    public Lod Lod { get; init; }

    /// <summary>
    /// Gets the extension of the file this tile was saved in.
    /// </summary>
    public string FileExtension { get; init; }

    /// <summary>
    /// Gets a value indicating if this <see cref="TileInfo"/> object has been disposed.
    /// </summary>
    public bool IsDisposed { get; protected set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="TileInfo"/> object.
    /// </summary>
    /// <param name="world">The world that this tile belongs to.</param>
    /// <param name="x">The X coordinate of the tile.</param>
    /// <param name="y">The Y coordinate of the tile.</param>
    /// <param name="lod">The level of detail that this tile is for.</param>
    /// <param name="dataSource">The data source.</param>
    public TileInfo(string world, int x, int y, Lod lod, Stream dataSource, string fileExtension)
    {
        World = world;
        X = x;
        Y = y;
        Lod = lod;
        DataSource = dataSource;
        FileExtension = fileExtension;
    }

    /// <summary>
    /// Attempts to initialise a tile object with values parsed from the given name.
    /// </summary>
    /// <param name="tileName">The name of the tile.</param>
    /// <param name="dataSource">The tile data.</param>
    /// <param name="tile">The parsed tile, or null if the operation was unsuccessful.</param>
    /// <returns>A value indicating if the operation was successful.</returns>
    public static bool TryParse(string tileName, Stream dataSource, [NotNullWhen(true)] out TileInfo? tile)
    {
        if (!dataSource.CanSeek)
            throw new ArgumentException("The data stream must be able to seek", nameof(dataSource));

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

        tile = new TileInfo(nameComponents[0], x, y, lod, dataSource, fileExtension);

        return true;
    }

    public override string ToString()
        => $"{World}_Tile_{Y}_{X}_{Lod}";

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
            DataSource.Dispose();
        }

        IsDisposed = true;
    }
}
