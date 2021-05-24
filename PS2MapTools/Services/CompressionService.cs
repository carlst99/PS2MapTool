using Spectre.Console;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTools.Services
{
    public class CompressionService
    {
        private const string OPTIPNG_FILE_NAME = "optipng.exe";
        private const string PREFIX = "[teal]COMPRESS [/]";

        private readonly IAnsiConsole _console;
        private readonly ParallelTaskRunner _taskRunner;

        public CompressionService(IAnsiConsole console, ParallelTaskRunner taskRunner)
        {
            _console = console;
            _taskRunner = taskRunner;
        }

        public void Compress(string filePath, CancellationToken ct)
        {
            if (!File.Exists(OPTIPNG_FILE_NAME))
            {
                _console.MarkupLine($"{ PREFIX }{ Formatter.Error("Compression failed:") } OptiPNG cannot be found.");
                return;
            }

            Task compressionTask = new(() =>
            {
                _console.MarkupLine($"{ PREFIX }Beginning compression on { Formatter.Path(Path.GetFileName(filePath)) }");
                try
                {
                    Process? process = Process.Start(new ProcessStartInfo(OPTIPNG_FILE_NAME, filePath)
                    {
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    });

                    if (process is null)
                        return;

                    // Fixes issues with OptiPNG spawning a child process
                    process.BeginOutputReadLine();
                    bool canWait = false;
                    process.OutputDataReceived += (_, __) => canWait = true;
                    while (!canWait)
                        Task.Delay(100).Wait();

                    process.WaitForExit();
                    process.Dispose();
                    _console.MarkupLine($"{ PREFIX }{ Formatter.Success("Completed") } compressing { Formatter.Path(Path.GetFileName(filePath)) }");
                }
                catch (Exception ex)
                {
                    _console.MarkupLine($"{ PREFIX }{ Formatter.Error("Compression failed:") } {ex}");
                }
            }, ct, TaskCreationOptions.LongRunning);

            _taskRunner.EnqueueTask(compressionTask);
        }
    }
}
