namespace PS2MapTool.Census.Models
{
    public record MapRegion
    {
        /// <summary>
        /// Gets the ID of this map region.
        /// </summary>
        public uint MapRegionId { get; init; }

        /// <summary>
        /// Gets the <see cref="CensusZone"/> that this region belongs to.
        /// </summary>
        public CensusZone ZoneId { get; init; }

        /// <summary>
        /// Gets the ID of the facility contained within this region.
        /// </summary>
        public uint? FacilityId { get; init; }

        /// <summary>
        /// Gets the name of the facility contained within this region.
        /// </summary>
        public string FacilityName { get; init; }

        /// <summary>
        /// Gets the <see cref="Models.FacilityType"/> of the facility contained within this region.
        /// </summary>
        public FacilityType? FacilityTypeId { get; init; }

        /// <summary>
        /// Gets a friendly name of the facility type.
        /// </summary>
        public string? FacilityType { get; init; }

        /// <summary>
        /// Gets the X-coordinate (left-to-right) of the facility on the map.
        /// </summary>
        public double? LocationX { get; init; }

        /// <summary>
        /// Gets the Y-coordinate (height) of the facility on the map.
        /// </summary>
        public double? LocationY { get; init; }

        /// <summary>
        /// Gets the Z-coordinate (top-to-bottom) of the facility on the map.
        /// </summary>
        public double? LocationZ { get; init; }

        /// <summary>
        /// Gets the amount of currency rewarded for owning this base.
        /// </summary>
        public int? RewardAmount { get; init; }

        /// <summary>
        /// Gets the type of currency rewarded for owning this base.
        /// </summary>
        public uint? RewardCurrencyId { get; init; }

        public MapRegion()
        {
            FacilityName = "Unknown";
        }
    }
}
