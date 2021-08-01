using SixLabors.ImageSharp;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services.Abstractions
{
    /// <summary>
    /// Defines a service used to load tile data into a usable format.
    /// </summary>
    public interface ITileProcessorService
    {
        /// <summary>
        /// Gets a value indicating whether or not this <see cref="ITileProcessorService"/> can load the given type of tile.
        /// </summary>
        /// <param name="tileSource">The tile data source.</param>
        bool CanLoad(Stream tileSource);

        /// <summary>
        /// Loads a tile into a usable in-memory representation.
        /// </summary>
        /// <param name="tileSource">The tile data stream.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>An <see cref="Image"/>. The type pixel buffer is determined by the implementing class.</returns>
        Task<Image> LoadAsync(Stream tileSource, CancellationToken ct = default);
    }
}
