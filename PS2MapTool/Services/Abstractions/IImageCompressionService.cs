using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services.Abstractions;

/// <summary>
/// Provides functions to compress images
/// </summary>
public interface IImageCompressionService
{
    /// <summary>
    /// Compresses an image.
    /// </summary>
    /// <param name="filePath">The path to the image to compress.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to stop the compression process with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CompressAsync(string filePath, CancellationToken ct = default);
}
