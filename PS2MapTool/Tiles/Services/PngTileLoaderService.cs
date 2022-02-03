using CommunityToolkit.HighPerformance.Buffers;
using PS2MapTool.Abstractions.Tiles;
using PS2MapTool.Abstractions.Tiles.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Tiles.Services;

/// <inheritdoc cref="ITileLoaderService" />
public class PngTileLoaderService : ITileLoaderService
{
    public static readonly byte[] PNG_MAGIC_ID = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    /// <inheritdoc />
    public async Task<bool> CanLoadAsync(ITileDataSource tile, CancellationToken ct = default)
    {
        using MemoryOwner<byte> buffer = await tile.GetTileDataAsync(ct).ConfigureAwait(false);

        for (int i = 0; i < PNG_MAGIC_ID.Length; i++)
        {
            if (buffer.Span[i] != PNG_MAGIC_ID[i])
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    /// <returns>An <see cref="Image{Rgba32}"/>.</returns>
    public virtual async Task<Image> LoadAsync(ITileDataSource tile, CancellationToken ct = default)
    {
        using MemoryOwner<byte> buffer = await tile.GetTileDataAsync(ct).ConfigureAwait(false);
        return Image.Load<Rgba32>(buffer.Span);
    }
}
