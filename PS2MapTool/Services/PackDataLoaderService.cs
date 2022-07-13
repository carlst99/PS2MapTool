using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Pack2;
using Mandible.Services;
using Mandible.Util;
using PS2MapTool.Abstractions.Services;
using PS2MapTool.Abstractions.Tiles;
using PS2MapTool.Areas;
using PS2MapTool.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services;

/// <summary>
/// Provides functions to load mapping data from pack2 files.
/// </summary>
public class PackDataLoaderService : IDataLoaderService
{
    protected readonly string _packsLocation;

    /// <summary>
    /// Initialises a new instance of the <see cref="PackDataLoaderService"/> object.
    /// </summary>
    /// <param name="packsPath">The path to the pack2 files.</param>
    public PackDataLoaderService(string packsPath)
    {
        _packsLocation = packsPath;
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<ITileDataSource>> GetTilesAsync
    (
        string worldName,
        Lod lod,
        CancellationToken ct = default
    )
    {
        IEnumerable<string> worldPacks = Directory.EnumerateFiles(_packsLocation, worldName + "_x64_?.pack2");
        List<PackedTileInfo> infos = PregenPackedTileInfo(worldName, lod);
        List<Pack2TileDataSource> tiles = new();

        foreach (string worldPack in worldPacks)
        {
            ct.ThrowIfCancellationRequested();

            using RandomAccessDataReaderService radrs = new(worldPack);
            using Pack2Reader reader = new(radrs);

            IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);
            foreach (Asset2Header assetHeader in assetHeaders)
            {
                PackedTileInfo? tileInfo = infos.FirstOrDefault(ti => ti.NameHash == assetHeader.NameHash);
                if (tileInfo is null)
                    continue;

                Pack2TileDataSource tileSource = new
                (
                    worldName,
                    tileInfo.X,
                    tileInfo.Y,
                    tileInfo.lod,
                    ".dds",
                    worldPack,
                    assetHeader
                );
                tiles.Add(tileSource);
            }
        }

        return tiles;
    }

    /// <inheritdoc />
    public virtual async Task<AreasSourceInfo> GetAreasAsync(string worldName, CancellationToken ct = default)
    {
        using RandomAccessDataReaderService radrs = new(Path.Combine(_packsLocation, "data_x64_0.pack2"));
        using Pack2Reader reader = new(radrs);

        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);
        ulong areasFileNameHash = PackCrc64.Calculate(worldName + "Areas.xml");
        Asset2Header? areasFileHeader = assetHeaders.FirstOrDefault(a => a.NameHash == areasFileNameHash);
        if (areasFileHeader is null)
            throw new FileNotFoundException("Could not find an areas file for the given world");

        using MemoryOwner<byte> areasData = await reader.ReadAssetDataAsync(areasFileHeader, ct).ConfigureAwait(false);
        return new AreasSourceInfo(worldName, areasData.AsStream());
    }

    private static List<PackedTileInfo> PregenPackedTileInfo(string worldName, Lod lod)
    {
        List<PackedTileInfo> infos = new();
        int lodNum = (int)lod;
        int increment = 4 * (int)Math.Pow(2, lodNum);

        for (int x = -64; x < 64; x += increment)
        {
            for (int y = -64; y < 64; y += increment)
            {
                string xs = x >= 0
                    ? x.ToString("D3")
                    : x.ToString("D2");

                string ys = y >= 0
                    ? y.ToString("D3")
                    : y.ToString("D2");

                ulong nameHash = PackCrc64.Calculate($"{worldName}_Tile_{xs}_{ys}_LOD{lodNum}.dds");
                    infos.Add(new PackedTileInfo(nameHash, x, y, lod));
            }
        }

        return infos;
    }

    private record PackedTileInfo(ulong NameHash, int X, int Y, Lod lod);
}
