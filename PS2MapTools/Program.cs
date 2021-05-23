using CliFx;
using System.Threading.Tasks;

namespace PS2MapTools
{
    public static class Program
    {
        public static async Task<int> Main() =>
            await new CliApplicationBuilder()
                .SetDescription("Tools to extract and stitch together PlanetSide 2 world map LODs.")
                .SetTitle("PS2 Map Tools")
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync()
                .ConfigureAwait(false);
    }
}
