using PS2MapTool.Services.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
{
    /// <inheritdoc cref="DdsTileProcessorService"/>
    public class DdsTileProcessorService : ITileProcessorService
    {
        public static readonly byte[] DDS_MAGIC_ID = new byte[] { 0x44, 0x44, 0x53, 0x20 };

        /// <inheritdoc />
        public bool CanLoad(Stream tileSource)
        {
            if (!tileSource.CanSeek || !tileSource.CanRead)
                return false;

            // Read the tile magic identifier and reset the stream position
            long oldPos = tileSource.Position;
            byte[] tileMagic = new byte[4];
            tileSource.Read(tileMagic);
            tileSource.Seek(oldPos, SeekOrigin.Begin);

            return DDS_MAGIC_ID.SequenceEqual(tileMagic);
        }

        /// <inheritdoc />
        /// <remarks>This operation does not complete asynchronously.</remarks>
        /// <returns>An <see cref="Image{Bgra32}"/>.</returns>
        public virtual Task<Image> LoadAsync(Stream tileSource, CancellationToken ct = default)
        {
            using Pfim.IImage image = Pfim.Pfim.FromStream(tileSource);
            byte[] newData;

            int tightStride = image.Width * image.BitsPerPixel / 8;
            if (image.Stride != tightStride)
            {
                newData = new byte[image.Height * tightStride];
                for (int i = 0; i < image.Height; i++)
                {
                    Buffer.BlockCopy(image.Data, i * image.Stride, newData, i * tightStride, tightStride);
                }
            }
            else
            {
                newData = image.Data;
            }

            return Task.FromResult((Image)Image.LoadPixelData<Bgra32>(newData, image.Width, image.Height));
        }
    }
}
