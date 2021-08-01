using PS2MapTool.Services.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
{
    /// <inheritdoc cref="ITileProcessorService" />
    public class PngTileProcessorService : ITileProcessorService
    {
        public static readonly byte[] PNG_MAGIC_ID = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        /// <inheritdoc />
        public bool CanLoad(Stream tileSource)
        {
            if (!tileSource.CanSeek || !tileSource.CanRead)
                return false;

            // Read the tile magic identifier and reset the stream position
            long oldPos = tileSource.Position;
            byte[] tileMagic = new byte[8];
            tileSource.Read(tileMagic);
            tileSource.Seek(oldPos, SeekOrigin.Begin);

            return PNG_MAGIC_ID.SequenceEqual(tileMagic);
        }

        /// <inheritdoc />
        /// <returns>An <see cref="Image{Rgba32}"/>.</returns>
        public virtual async Task<Image> LoadAsync(Stream tileSource, CancellationToken ct = default)
        {
            return await Image.LoadAsync<Rgba32>(Configuration.Default, tileSource, ct).ConfigureAwait(false);
        }
    }
}
