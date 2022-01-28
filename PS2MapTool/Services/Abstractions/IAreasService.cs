using PS2MapTool.Areas;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services.Abstractions;

/// <summary>
/// Provides functions to get and manipulate no-deploy areas.
/// </summary>
public interface IAreasService
{
    /// <summary>
    /// Gets no-deployment areas from an areas data source.
    /// </summary>
    /// <param name="type">The type of no-deployment area to get.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to stop the operation with.</param>
    /// <returns>A list of <see cref="AreaDefinition"/>s that define the relevant no-deploy areas.</returns>
    Task<IList<AreaDefinition>> GetNoDeployAreasAsync(AreasSourceInfo areasSourceInfo, NoDeployType type, CancellationToken ct = default);

    /// <summary>
    /// Creates a no-deploy area image designed to be used as an overlay, with areas rendered in red.
    /// </summary>
    /// <param name="noDeployAreas">The no-deploy areas to render.</param>
    /// <param name="lod">The map LOD to create the overlay at.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to stop the operation with.</param>
    /// <returns>An Rgba32 image.</returns>
    Task<Image<Rgba32>> CreateNoDeployZoneImageAsync(IEnumerable<AreaDefinition> noDeployAreas, Lod lod = Lod.Lod0, CancellationToken ct = default);
}
