using PS2MapTool.Abstractions.Services;
using PS2MapTool.Areas;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace PS2MapTool.Services;

/// <inheritdoc cref="IAreasService"/>
public class AreasService : IAreasService
{
    /// <summary>
    /// The image size, in pixels, that the area definitions are designed to fit within.
    /// </summary>
    public const int IMAGE_PIXEL_SIZE = 8192;

    /// <inheritdoc />
    public virtual async Task<IList<AreaDefinition>> GetNoDeployAreasAsync(AreasSourceInfo areasSourceInfo, NoDeployType type, CancellationToken ct = default)
    {
        List<AreaDefinition> areas = new();
        AreaDefinition? lastZone = null;

        XmlReaderSettings xmlSettings = new()
        {
            Async = true,
            CloseInput = false,
            ConformanceLevel = ConformanceLevel.Fragment // This is required as the area definitions are stored in one file as multiple root-level objects.
        };
        using XmlReader reader = XmlReader.Create(areasSourceInfo.DataSource, xmlSettings);

        while (await reader.ReadAsync())
        {
            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();

            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name == "AreaDefinition")
                    {
                        _ = TryParseNoDeployInfo(reader, out lastZone);
                    }
                    if (reader.Name == "Property" // Given the data structure, we can assume that this is nested within an AreaDefinition (that may or may not be a valid no-deploy zone).
                        && reader.GetAttribute("type") == "SundererNoDeploy" // Check that this property is for a no-deploy zone.
                        && lastZone is not null // Ensure that we have a valid zone for this nested property.
                        && TryParseNdzPropertyInfo(reader, out NoDeployAreaInfo? info)
                        && info is not null // Ensure that we got valid info about the no-deploy zone.
                        && info.Requirement == type) // Check that the no-deploy zone matches the type that we're interested in.
                    {
                        lastZone.Info = info;
                        areas.Add(lastZone);
                    }
                    break;
            }
        }

        areasSourceInfo.DataSource.Seek(0, System.IO.SeekOrigin.Begin);

        return areas;
    }

    /// <inheritdoc />
    public virtual async Task<Image<Rgba32>> CreateNoDeployZoneImageAsync(IEnumerable<AreaDefinition> noDeployZones, Lod lod = Lod.Lod0, CancellationToken ct = default)
    {
        Image<Rgba32> ndzImage = new(IMAGE_PIXEL_SIZE / GetLodScalar(lod), IMAGE_PIXEL_SIZE / GetLodScalar(lod));

        Task t = new(() =>
        {
            foreach (AreaDefinition ndz in noDeployZones)
            {
                if (ct.IsCancellationRequested)
                    throw new TaskCanceledException();

                AreaDefinition nomalised = NormaliseNoDeployZone(ndz, lod);
                IPath zonePoly = new EllipsePolygon(nomalised.X, nomalised.Z, nomalised.Radius);

                ndzImage.Mutate(x => x.Fill(Color.Red, zonePoly));
            }

            ndzImage.Mutate(x => x.Rotate(RotateMode.Rotate270));
        }, ct, TaskCreationOptions.LongRunning);
        t.Start();
        await t;

        return ndzImage;
    }

    /// <summary>
    /// Attempts to parse a <see cref="AreaDefinition"/> object from the XML data.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> to use.</param>
    /// <param name="area">The parsed zone.</param>
    /// <returns>A value indicating if the parse operation completed successfully.</returns>
    private static bool TryParseNoDeployInfo(XmlReader reader, [NotNullWhen(true)] out AreaDefinition? area)
    {
        area = null;
        string? xVal = reader.GetAttribute("x1");
        string? yVal = reader.GetAttribute("z1");
        string? radVal = reader.GetAttribute("radius");

        if (xVal is null || yVal is null || radVal is null)
            return false;

        if (!float.TryParse(xVal, out float x))
            return false;

        if (!float.TryParse(yVal, out float y))
            return false;

        if (!float.TryParse(radVal, out float radius))
            return false;

        area = new AreaDefinition
        {
            X = x,
            Z = y,
            Radius = radius,
            Name = reader.GetAttribute("name")
        };
        return true;
    }

    /// <summary>
    /// Attempts to parse a <see cref="NoDeployAreaInfo"/> object from the XML data.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> to use.</param>
    /// <param name="info">The parsed info.</param>
    /// <returns>A value indicating if the parse operation completed successfully.</returns>
    private static bool TryParseNdzPropertyInfo(XmlReader reader, out NoDeployAreaInfo? info)
    {
        info = null;
        string? reqVal = reader.GetAttribute("Requirement");
        string? facVal = reader.GetAttribute("FacilityId");

        if (!int.TryParse(reqVal, out int req))
            return false;

        if (!int.TryParse(facVal, out int fac))
            return false;

        info = new NoDeployAreaInfo
        {
            Requirement = (NoDeployType)req,
            FacilityId = fac
        };
        return true;
    }

    /// <summary>
    /// Normalises the coordinates of an <see cref="AreaDefinition"/> for use with ImageSharp.
    /// </summary>
    /// <param name="area">The <see cref="AreaDefinition"/> to normalise.</param>
    /// <param name="lod">The LOD to scale this <see cref="AreaDefinition"/> for.</param>
    /// <returns>The normalised <see cref="AreaDefinition"/>.</returns>
    private static AreaDefinition NormaliseNoDeployZone(AreaDefinition area, Lod lod)
        => new()
        {
            X = (area.X + (IMAGE_PIXEL_SIZE / 2)) / GetLodScalar(lod),
            Z = (area.Z + (IMAGE_PIXEL_SIZE / 2)) / GetLodScalar(lod),
            Radius = area.Radius / GetLodScalar(lod)
        };

    /// <summary>
    /// Gets a value from a <see cref="Lod"/> that can be used to scale pixel values from their default value, to a value that is in line with the expectations of the provided <see cref="Lod"/>.
    /// </summary>
    /// <param name="lod">The <see cref="Lod"/> to get the scalar of.</param>
    /// <returns>The scalar.</returns>
    private static int GetLodScalar(Lod lod) => (int)lod + 1;
}
