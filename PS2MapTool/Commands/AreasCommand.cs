using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using PS2MapTool.Core;
using PS2MapTool.Core.Services;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Commands
{
    [Command("areas")]
    public class AreasCommand : ICommand
    {
        [CommandParameter(0)]
        public string AreasDirectory { get; init; }

        [CommandOption('o', Description = "The path to output the compiled no deploy zone images to.")]
        public string OutputFilePath { get; init; }

        public AreasCommand()
        {
            AreasDirectory = string.Empty;
            OutputFilePath = string.Empty;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (!Directory.Exists(AreasDirectory))
                throw new CommandException("Could not find the directory containing the area files.");

            CancellationToken ct = console.RegisterCancellationHandler();

            AreaService areaService = new(new DirectoryDataLoaderService(AreasDirectory, false));

            IList<AreaDefinition> noDeployZones = await areaService.GetNoDeployAreasAsync(World.Amerish, NoDeployType.Sunderer, ct).ConfigureAwait(false);

            using Image ndzImage = await areaService.CreateNoDeployZoneImageAsync(noDeployZones, Lod.Lod2, ct).ConfigureAwait(false);
            await ndzImage.SaveAsPngAsync("ndz.png").ConfigureAwait(false);
        }
    }
}
