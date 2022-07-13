using DbgCensus.Rest.Abstractions;
using DbgCensus.Rest.Abstractions.Queries;
using PS2MapTool.Census.Models;
using PS2MapTool.Census.Services.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Census.Services;

/// <inheritdoc cref="ICensusService"/>
public class CensusService : ICensusService
{
    /// <summary>
    /// The maximum number of elements to return with each query.
    /// </summary>
    protected const int PAGE_LIMIT = 500;

    protected readonly IQueryService _queryService;

    /// <summary>
    /// Initialises a new instance of the <see cref="CensusService"/> class.
    /// </summary>
    /// <param name="queryService">The query service.</param>
    public CensusService(IQueryService queryService)
    {
        _queryService = queryService;
    }

    /// <inheritdoc />
    public virtual async Task<List<LatticeLink>> GetLatticeLinksAsync(CensusZone zone, CancellationToken ct = default)
    {
        IQueryBuilder query = _queryService.CreateQuery()
            .OnCollection("facility_link")
            .Where("zone_id", SearchModifier.Equals, (int)zone);

        return await GetEntireCollection<LatticeLink>(query, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<List<MapHex>> GetMapHexesAsync(CensusZone zone, CancellationToken ct = default)
    {
        IQueryBuilder query = _queryService.CreateQuery()
            .OnCollection("map_hex")
            .Where("zone_id", SearchModifier.Equals, (int)zone);

        return await GetEntireCollection<MapHex>(query, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<List<MapRegion>> GetMapRegionsAsync(CensusZone zone, CancellationToken ct = default)
    {
        IQueryBuilder query = _queryService.CreateQuery()
            .OnCollection("map_region")
            .Where("zone_id", SearchModifier.Equals, (int)zone);

        return await GetEntireCollection<MapRegion>(query, ct).ConfigureAwait(false);
    }

    protected async Task<List<T>> GetEntireCollection<T>(IQueryBuilder query, CancellationToken ct = default)
    {
        List<T> elements = new();
        int startAt = 0;
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
