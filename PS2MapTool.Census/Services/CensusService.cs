using DbgCensus.Rest.Abstractions;
using DbgCensus.Rest.Abstractions.Queries;
using PS2MapTool.Census.Models;
using PS2MapTool.Census.Services.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Census.Services
{
    public class CensusService : ICensusService
    {
        private const uint PAGE_LIMIT = 500;

        private readonly IQueryService _queryService;

        public CensusService(IQueryService queryService)
        {
            _queryService = queryService;
        }

        public async Task<List<LatticeLink>> GetLatticeLinksAsync(CancellationToken ct = default)
        {
            IQueryBuilder query = _queryService.CreateQuery()
                .OnCollection("facility_link");

            return await GetEntireCollection<LatticeLink>(query, ct).ConfigureAwait(false);
        }

        public async Task<List<MapHex>> GetMapHexesAsync(CancellationToken ct = default)
        {
            IQueryBuilder query = _queryService.CreateQuery()
                .OnCollection("map_hex");

            return await GetEntireCollection<MapHex>(query, ct).ConfigureAwait(false);
        }

        public async Task<List<MapRegion>> GetMapRegionsAsync(CancellationToken ct = default)
        {
            IQueryBuilder query = _queryService.CreateQuery()
                .OnCollection("map_region");

            return await GetEntireCollection<MapRegion>(query, ct).ConfigureAwait(false);
        }

        private async Task<List<T>> GetEntireCollection<T>(IQueryBuilder query, CancellationToken ct = default)
        {
            List<T> elements = new();
            uint startAt = 0;
            query.WithLimit(PAGE_LIMIT);

            do
            {
                query.WithStartIndex(startAt);
                List<T>? tempElements = await _queryService.GetAsync<List<T>>(query, ct).ConfigureAwait(false);

                if (tempElements is not null)
                    elements.AddRange(tempElements);

                startAt += PAGE_LIMIT;
            }
            while (elements.Count == startAt);

            return elements;
        }
    }
}
