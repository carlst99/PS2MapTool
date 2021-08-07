using Pfim;
using PS2MapTool.Services.Abstractions;
using PS2MapTool.Tiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
{
    /// <inheritdoc cref="ITileLoaderService"/>
    public class DdsTileLoaderService : ITileLoaderService
    {
        public static readonly byte[] DDS_MAGIC_ID = new byte[] { 0x44, 0x44, 0x53, 0x20 };

        /// <inheritdoc />
        public bool CanLoad(TileInfo tile)
        {
            Stream tileSource = tile.DataSource;
            if (!tileSource.CanSeek || !tileSource.CanRead)
                return false;

            // Read the tile magic identifier and reset the stream position
            long oldPos = tileSource.Position;
            byte[] tileMagic = new byte[4];
            tileSource.Read(tileMagic);
            tileSource.Seek(oldPos, SeekOrigin.Begin);

            return DDS_MAGIC_ID.SequenceEqual(tileMagic);
        }

        /// <summary>
        /// <inheritdoc/>
        /// Note that this operation does NOT complete asynchronously.
        /// </summary>
        /// <remarks>Adapted from <see href="https://github.com/nickbabcock/Pfim/blob/master/src/Pfim.ImageSharp/Program.cs"/>.</remarks>
        /// <returns>An <see cref="Image{Bgra32}"/>.</returns>
        public virtual Task<Image> LoadAsync(TileInfo tile, CancellationToken ct = default)
        {
            Stream tileSource = tile.DataSource;
            PooledAllocator allocator = new();
            PfimConfig pfimConfig = new(allocator: allocator);

            using Pfim.IImage image = Pfim.Pfim.FromStream(tileSource, pfimConfig);
            byte[] newData;

            // Since image sharp can't handle data with line padding in a stride
            // we create an stripped down array if any padding is detected
            int tightStride = image.Width * image.BitsPerPixel / 8;
            if (image.Stride != tightStride)
            {
                newData = allocator.Rent(image.Height * tightStride); //new byte[image.Height * tightStride];
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

            Image convertedImage = Image.LoadPixelData<Bgra32>(newData, image.Width, image.Height);
            return Task.FromResult(convertedImage);
        }

        private class PooledAllocator : IImageAllocator
        {
            private readonly ArrayPool<byte> _shared = ArrayPool<byte>.Shared;

            public byte[] Rent(int size)
            {
                return _shared.Rent(size);
            }

            public void Return(byte[] data)
            {
                _shared.Return(data);
            }
        }
    }
}
