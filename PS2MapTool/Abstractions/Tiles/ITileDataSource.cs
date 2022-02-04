using CommunityToolkit.HighPerformance.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Abstractions.Tiles;

/// <summary>
/// Represents a tile data source.
/// </summary>
public interface ITileDataSource
{
    /// <summary>
    /// Gets the world that this tile belongs to.
    /// </summary>
    string WorldName { get; }

    /// <summary>
    /// Gets the X coordinate of this tile.
    /// </summary>
    int X { get; }

    /// <summary>
    /// Gets the Y coordinate of this tile.
    /// </summary>
    int Y { get; }

    /// <summary>
    /// Gets the level of detail that this tile is for.
    /// </summary>
    Lod Lod { get; }

    /// <summary>
    /// Gets the file extension of the tile data source.
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Gets the tile data.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The tile data.</returns>
    Task<MemoryOwner<byte>> GetTileDataAsync(CancellationToken ct = default);
}
