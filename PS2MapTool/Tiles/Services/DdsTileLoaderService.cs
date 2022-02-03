using CommunityToolkit.HighPerformance.Buffers;
using Pfim;
using PS2MapTool.Abstractions.Tiles;
using PS2MapTool.Abstractions.Tiles.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Tiles.Services;

/// <inheritdoc cref="ITileLoaderService"/>
public class DdsTileLoaderService : ITileLoaderService
{
    public static readonly byte[] DDS_MAGIC_ID = new byte[] { 0x44, 0x44, 0x53, 0x20 };

    /// <inheritdoc />
    public async Task<bool> CanLoadAsync(ITileDataSource tile, CancellationToken ct = default)
    {
        using MemoryOwner<byte> buffer = await tile.GetTileDataAsync(ct).ConfigureAwait(false);

        for (int i = 0; i < DDS_MAGIC_ID.Length; i++)
        {
            if (buffer.Span[i] != DDS_MAGIC_ID[i])
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    /// <remarks>Adapted from <see href="https://github.com/nickbabcock/Pfim/blob/master/src/Pfim.ImageSharp/Program.cs"/>.</remarks>
    /// <returns>An <see cref="Image{Bgra32}"/>.</returns>
    public virtual async Task<Image> LoadAsync(ITileDataSource tile, CancellationToken ct = default)
    {
        using MemoryOwner<byte> buffer = await tile.GetTileDataAsync(ct).ConfigureAwait(false);
        byte[] tempBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);

        buffer.Span.CopyTo(tempBuffer);
        using MemoryStream ms = new(tempBuffer, 0, buffer.Length);

        PooledAllocator allocator = new();
        PfimConfig pfimConfig = new(allocator: allocator);

        using Pfim.IImage image = Pfim.Pfim.FromStream(ms, pfimConfig);
        ArrayPool<byte>.Shared.Return(tempBuffer);

        byte[] newData;

        // Since image sharp can't handle data with line padding in a stride
        // we create an stripped down array if any padding is detected
        int tightStride = image.Width * image.BitsPerPixel / 8;
        if (image.Stride != tightStride)
        {
            newData = new byte[image.Height * tightStride];
            for (int i = 0; i < image.Height; i++)
            {
                if (ct.IsCancellationRequested)
                    throw new TaskCanceledException();

                Buffer.BlockCopy(image.Data, i * image.Stride, newData, i * tightStride, tightStride);
            }
        }
        else
        {
            newData = image.Data;
        }

        return Image.LoadPixelData<Bgra32>(newData, image.Width, image.Height);
    }

    private class PooledAllocator : IImageAllocator
    {
        private readonly ArrayPool<byte> _shared = ArrayPool<byte>.Shared;

        public byte[] Rent(int size)
            => _shared.Rent(size);

        public void Return(byte[] data)
            => _shared.Return(data);
    }
}
