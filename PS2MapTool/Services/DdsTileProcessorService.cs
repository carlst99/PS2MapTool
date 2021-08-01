using PS2MapTool.Services.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
{
    /// <inheritdoc cref="DdsTileProcessorService"/>
    public class DdsTileProcessorService : ITileProcessorService
    {
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
