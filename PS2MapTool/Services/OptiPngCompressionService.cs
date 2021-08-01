using PS2MapTool.Exceptions;
using PS2MapTool.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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

            Process? process = Process.Start(new ProcessStartInfo(OPTIPNG_FILE_NAME, $"-o3 {filePath}")
            {
                CreateNoWindow = true,
                RedirectStandardError = true
            });

            if (process is null)
                throw new OptiPngException("Could not start OptiPNG process");

            process.WaitForExit();
            return;

            // OptiPNG uses a starter process, resulting in the main process exiting almost immediately. Hence, we'll try to find its children on Windows.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (Process child in process.GetChildren())
                    process.WaitForExit();

                process.WaitForExit();
                //await process.WaitForExitAsync(ct).ConfigureAwait(false);
            }
            else // Else, we'll wait for output, which gets bubbled up somehow (and somewhat unreliably) and then seems to 'un-exit' the process. Note that OptiPNG outputs to stderr.
            {
                bool canWait = false;
                process.BeginErrorReadLine();
                process.ErrorDataReceived += (_, __) => canWait = true;
                while (!canWait)
                    await Task.Delay(100, ct).ConfigureAwait(false);

                await process.WaitForExitAsync(ct).ConfigureAwait(false);
            }
        }
    }
}
