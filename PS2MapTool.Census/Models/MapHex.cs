namespace PS2MapTool.Census.Models;

/// <summary>
/// Stores information about a map hex.
/// </summary>
public record MapHex
{
    /// <summary>
    /// Gets the <see cref="CensusZone"/> that this hex belongs to.
    /// </summary>
    public CensusZone ZoneId { get; init; }

    /// <summary>
    /// Gets the ID of the region that this hex belongs to.
    /// </summary>
    public uint MapRegionId { get; init; }

    /// <summary>
    /// Gets the X-coordinate of this hex within the axial coordinate system.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    /// Gets the Y-coordinate of this hex within the axial coordinate system.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    /// It is unknown what purpose this field holds.
    /// </summary>
    public int HexType { get; init; }

    /// <summary>
    /// It is unknown what purpose this field holds.
    /// </summary>
    public string TypeName { get; init; }

    public MapHex()
    {
        TypeName = string.Empty;
    }
}
