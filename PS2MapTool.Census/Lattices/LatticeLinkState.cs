namespace PS2MapTool.Census.Lattices;

/// <summary>
/// Defines the state of a lattice link.
/// </summary>
public enum LatticeLinkState
{
    /// <summary>
    /// The lattice link connects two facilities owned by different factions.
    /// </summary>
    Partial = 0,

    /// <summary>
    /// The lattice link connects two facilities owned by the same faction.
    /// </summary>
    Full = 1
}
