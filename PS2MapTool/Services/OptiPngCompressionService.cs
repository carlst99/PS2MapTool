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
        public virtual async Task CompressAsync(string filePath, CancellationToken ct = default)
        {
            if (!File.Exists(OPTIPNG_FILE_NAME))
                throw new OptiPngNotFoundException();

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Could not find the file to compress", filePath);

            Process? process = Process.Start(new ProcessStartInfo(OPTIPNG_FILE_NAME, filePath)
            {
                CreateNoWindow = true,
                RedirectStandardError = true
            });

            if (process is null)
                throw new OptiPngException("Could not start OptiPNG process");

            // OptiPNG outputs to stderr, and also seems to use a 'starter process', resulting in Process.HasExited being set initially. Waiting for output solves this.
            bool canWait = false;
            process.BeginErrorReadLine();
            process.ErrorDataReceived += (_, __) => canWait = true;
            while (!canWait)
                await Task.Delay(100, ct).ConfigureAwait(false);

            await process.WaitForExitAsync(ct).ConfigureAwait(false);
        }
    }
}
