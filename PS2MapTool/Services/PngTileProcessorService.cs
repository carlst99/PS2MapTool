using PS2MapTool.Services.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
{
    /// <inheritdoc cref="ITileProcessorService" />
    public class PngTileProcessorService : ITileProcessorService
    {
        /// <inheritdoc />
        /// <returns>An <see cref="Image{Rgba32}"/>.</returns>
        public virtual async Task<Image> LoadAsync(Stream tileSource, CancellationToken ct = default)
        {
            return await Image.LoadAsync<Rgba32>(Configuration.Default, tileSource, ct).ConfigureAwait(false);
        }
    }
}
