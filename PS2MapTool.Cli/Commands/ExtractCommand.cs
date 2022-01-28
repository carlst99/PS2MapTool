using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using System.Threading.Tasks;

namespace PS2MapTool.Cli.Commands;

[Command("extract", Description = "Extracts LOD tiles from the game client assets.")]
public class ExtractCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine("Hello extract!");
        return default;
    }
}
