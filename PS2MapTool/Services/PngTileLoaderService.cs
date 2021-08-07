using PS2MapTool.Services.Abstractions;
using PS2MapTool.Tiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
{
    /// <inheritdoc cref="ITileLoaderService" />
    public class PngTileLoaderService : ITileLoaderService
    {
        public static readonly byte[] PNG_MAGIC_ID = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        /// <inheritdoc />
        public bool CanLoad(TileInfo tile)
        {
            Stream tileSource = tile.DataSource;
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
        public virtual async Task<Image> LoadAsync(TileInfo tile, CancellationToken ct = default)
        {
            return await Image.LoadAsync<Rgba32>(Configuration.Default, tile.DataSource, ct).ConfigureAwait(false);
        }
    }
}
