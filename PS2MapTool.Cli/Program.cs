using CliFx;
using System.Threading.Tasks;

namespace PS2MapTool.Cli;

public static class Program
{
    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .SetDescription("A tool to extract and stitch together PlanetSide 2 maps and no-deploy zones.")
            .SetTitle("PS2 Map Tools")
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync()
            .ConfigureAwait(false);
}
