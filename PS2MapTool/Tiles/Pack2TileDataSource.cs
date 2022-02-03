using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack2;
using Mandible.Pack2;
using PS2MapTool.Abstractions.Tiles;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Tiles;

/// <summary>
/// Represents a tile data source.
/// </summary>
/// <param name="WorldName">The world that this tile belongs to.</param>
/// <param name="X">The X coordinate of the tile.</param>
/// <param name="Y">The Y coordinate of the tile.</param>
/// <param name="Lod">The level of detail that this tile is for.</param>
/// <param name="FileExtension">The file extension of the tile data source.</param>
/// <param name="PackReader">The pack reader to obtain this tile from.</param>
/// <param name="TileHeader">The tile asset header.</param>
public record Pack2TileDataSource
(
    string WorldName,
    int X,
    int Y,
    Lod Lod,
    string FileExtension,
    IPack2Reader PackReader,
    Asset2Header TileHeader
) : ITileDataSource, IDisposable
{
    /// <summary>
    /// Gets a value indicating whether or not this <see cref="Pack2TileDataSource"/> instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public async Task<MemoryOwner<byte>> GetTileDataAsync(CancellationToken ct = default)
        => await PackReader.ReadAssetDataAsync(TileHeader, ct).ConfigureAwait(false);

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
            if (PackReader is IDisposable disposable)
                disposable.Dispose();
        }

        IsDisposed = true;
    }
}
