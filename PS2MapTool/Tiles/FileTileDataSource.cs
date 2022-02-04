using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Win32.SafeHandles;
using PS2MapTool.Abstractions.Tiles;
using System;
using System.IO;
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
/// <param name="DataSource">The tile file handle.</param>
public record FileTileDataSource
(
    string WorldName,
    int X,
    int Y,
    Lod Lod,
    string FileExtension,
    SafeFileHandle DataSource
) : ITileDataSource, IDisposable
{
    /// <summary>
    /// Gets a value indicating whether or not this <see cref="FileTileDataSource"/> instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public async Task<MemoryOwner<byte>> GetTileDataAsync(CancellationToken ct = default)
    {
        MemoryOwner<byte> buffer = MemoryOwner<byte>.Allocate((int)RandomAccess.GetLength(DataSource));
        await RandomAccess.ReadAsync(DataSource, buffer.Memory, 0, ct).ConfigureAwait(false);

        return buffer;
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
            DataSource.Dispose();
        }

        IsDisposed = true;
    }
}
