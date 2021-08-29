using PS2MapTool.Census.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Census.Services.Abstractions
{
    public interface ICensusService
    {
        Task<List<LatticeLink>> GetLatticeLinksAsync(CancellationToken ct = default);
        Task<List<MapHex>> GetMapHexesAsync(CancellationToken ct = default);
        Task<List<MapRegion>> GetMapRegionsAsync(CancellationToken ct = default);
    }
}
