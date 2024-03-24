using Pfim;
using PS2MapTool.Abstractions.Tiles.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.IO;

namespace PS2MapTool.Tiles.Services;

/// <inheritdoc cref="ITileLoaderService"/>
public class DdsTileLoaderService : ITileLoaderService
{
    public static readonly byte[] DDS_MAGIC_ID = new byte[] { 0x44, 0x44, 0x53, 0x20 };

    /// <inheritdoc />
    public bool CanLoad(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < DDS_MAGIC_ID.Length)
            return false;

        for (int i = 0; i < DDS_MAGIC_ID.Length; i++)
        {
            if (buffer[i] != DDS_MAGIC_ID[i])
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    /// <remarks>Adapted from <see href="https://github.com/nickbabcock/Pfim/blob/master/src/Pfim.ImageSharp/Program.cs"/>.</remarks>
    /// <returns>An <see cref="Image{Bgra32}"/>.</returns>
    public virtual Image Load(ReadOnlySpan<byte> buffer)
    {
        byte[] tempBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);

        buffer.CopyTo(tempBuffer);
        using MemoryStream ms = new(tempBuffer, 0, buffer.Length);

        PooledAllocator allocator = new();
        PfimConfig pfimConfig = new(allocator: allocator);

        using IImage image = Pfimage.FromStream(ms, pfimConfig);
        ArrayPool<byte>.Shared.Return(tempBuffer);

        bool returnNewData = false;
        byte[] newData;

        // Since image sharp can't handle data with line padding in a stride
        // we create an stripped down array if any padding is detected
        int tightStride = image.Width * image.BitsPerPixel / 8;
        if (image.Stride != tightStride)
        {
            newData = ArrayPool<byte>.Shared.Rent(image.Height * tightStride);
            returnNewData = true;

            for (int i = 0; i < image.Height; i++)
            {
                Buffer.BlockCopy(image.Data, i * image.Stride, newData, i * tightStride, tightStride);
            }
        }
        else
        {
            newData = image.Data;
        }

        Image<Bgra32> result = Image.LoadPixelData<Bgra32>(newData, image.Width, image.Height);

        if (returnNewData)
            ArrayPool<byte>.Shared.Return(newData);

        return result;
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
