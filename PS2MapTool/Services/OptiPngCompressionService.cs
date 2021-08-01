using PS2MapTool.Exceptions;
using PS2MapTool.Services.Abstractions;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services
{
    /// <summary>
    /// Provides functions to compress PNG images using OptiPNG.
    /// </summary>
    public class OptiPngCompressionService : IImageCompressionService
    {
        public const string OPTIPNG_FILE_NAME = "optipng.exe";

        /// <inheritdoc />
        /// <exception cref="OptiPngNotFoundException">Thrown when the OptiPNG binary cannot be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file to compress cannot be found.</exception>
        /// <exception cref="OptiPngException">Thrown when an error occurs with the OptiPNG process.</exception>
        public virtual Task CompressAsync(string filePath, CancellationToken ct = default)
        {
            if (!File.Exists(OPTIPNG_FILE_NAME))
                throw new OptiPngNotFoundException();

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Could not find the file to compress", filePath);

            Process? process = Process.Start(new ProcessStartInfo(OPTIPNG_FILE_NAME, $"-o3 {filePath}")
            {
                CreateNoWindow = true,
                RedirectStandardError = true
            });

            if (process is null)
                throw new OptiPngException("Could not start OptiPNG process");

            // TODO: For unknown reasons, asynchronous tasks at this point seem to fail silently.
            process.WaitForExit();
            return Task.CompletedTask;
        }
    }
}
