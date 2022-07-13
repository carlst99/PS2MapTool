using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack2;
using Mandible.Pack2;
using Mandible.Services;
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
/// <param name="PackPath">The path to the pack file in which this tile asset is stored.</param>
/// <param name="TileHeader">The tile asset header.</param>
public record Pack2TileDataSource
(
    string WorldName,
    int X,
    int Y,
    Lod Lod,
    string FileExtension,
    string PackPath,
    Asset2Header TileHeader
) : ITileDataSource
{
    /// <inheritdoc />
    public async Task<MemoryOwner<byte>> GetTileDataAsync(CancellationToken ct = default)
    {
        using RandomAccessDataReaderService radrs = new(PackPath);
        using Pack2Reader reader = new(radrs);
        return await reader.ReadAssetDataAsync(TileHeader, ct).ConfigureAwait(false);
    }

    public override string ToString()
        => $"{WorldName}_Tile_{Y}_{X}_{Lod}";
}
