using PS2MapTool.Census.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Census.Services.Abstractions
{
    /// <summary>
    /// Provides methods to retrieve data from the Census API.
    /// </summary>
    public interface ICensusService
    {
        /// <summary>
        /// Gets every lattice link within a zone.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A list of every lattice within the zone.</returns>
        Task<List<LatticeLink>> GetLatticeLinksAsync(CensusZone zone, CancellationToken ct = default);

        /// <summary>
        /// Gets every map hex within a zone.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A list of every map hex within the zone.</returns>
        Task<List<MapHex>> GetMapHexesAsync(CensusZone zone, CancellationToken ct = default);

        /// <summary>
        /// Gets every map region within a zone.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A list of every map region within the zone.</returns>
        Task<List<MapRegion>> GetMapRegionsAsync(CensusZone zone, CancellationToken ct = default);
    }
}
