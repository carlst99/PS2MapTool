using Spectre.Console;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Cli.Services
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
                _console.MarkupLine($"{ PREFIX }{ Formatter.Error("Compression failed:") } { OPTIPNG_FILE_NAME } cannot be found.");
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
                        RedirectStandardError = true
                    });

                    if (process is null)
                        return;

                    // OptiPNG outputs to stderr, and also seems to use a 'starter process', resulting in Process.HasExited being set initially. Waiting for output solves this.
                    bool canWait = false;
                    process.BeginErrorReadLine();
                    process.ErrorDataReceived += (_, __) => canWait = true;
                    while (!canWait)
                        Task.Delay(100).Wait();

                    process.WaitForExit();
                    process.Dispose();
                }
                catch (Exception ex)
                {
                    _console.MarkupLine($"{ PREFIX }{ Formatter.Error("Compression failed:") } {ex}");
                }

                _console.MarkupLine($"{ PREFIX }{ Formatter.Success("Completed") } compressing { Formatter.Path(Path.GetFileName(filePath)) }");
            }, ct, TaskCreationOptions.LongRunning);

            _taskRunner.EnqueueTask(compressionTask);
        }
    }
}
