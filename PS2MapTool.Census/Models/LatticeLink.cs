namespace PS2MapTool.Census.Models
{
    /// <summary>
    /// Stores information about a lattice link.
    /// </summary>
    public record LatticeLink
    {
        /// <summary>
        /// Gets the <see cref="CensusZone"/> that this lattice link belongs to.
        /// </summary>
        public CensusZone ZoneId { get; init; }

        /// <summary>
        /// Gets the ID of the first facility in the link.
        /// </summary>
        public uint FacilityIdA { get; init; }

        /// <summary>
        /// Gets the ID of the second facility in the link.
        /// </summary>
        public uint FacilityIdB { get; init; }

        /// <summary>
        /// Gets the description of the lattice link, if present.
        /// </summary>
        public string? Description { get; init; }
    }
}
