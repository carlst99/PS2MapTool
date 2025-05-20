using DbgCensus.Rest.Abstractions;
using DbgCensus.Rest.Abstractions.Queries;
using PS2MapTool.Census.Models;
using PS2MapTool.Census.Services.Abstractions;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
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

        return await GetEntireCollection<IEnumerable<LatticeLink>, LatticeLink>
        (
            query,
            JsonContext.Default.IEnumerableLatticeLink,
            ct
        );
    }

    /// <inheritdoc />
    public virtual async Task<List<MapHex>> GetMapHexesAsync(CensusZone zone, CancellationToken ct = default)
    {
        IQueryBuilder query = _queryService.CreateQuery()
            .OnCollection("map_hex")
            .Where("zone_id", SearchModifier.Equals, (int)zone);

        return await GetEntireCollection<IEnumerable<MapHex>, MapHex>
        (
            query,
            JsonContext.Default.IEnumerableMapHex,
            ct
        );
    }

    /// <inheritdoc />
    public virtual async Task<List<MapRegion>> GetMapRegionsAsync(CensusZone zone, CancellationToken ct = default)
    {
        IQueryBuilder query = _queryService.CreateQuery()
            .OnCollection("map_region")
            .Where("zone_id", SearchModifier.Equals, (int)zone);

        return await GetEntireCollection<IEnumerable<MapRegion>, MapRegion>
        (
            query,
            JsonContext.Default.IEnumerableMapRegion,
            ct
        );
    }

    protected async Task<List<TElement>> GetEntireCollection<T, TElement>
    (
        IQueryBuilder query,
        JsonTypeInfo<T> typeInfo,
        CancellationToken ct = default
    ) where T : IEnumerable<TElement>
    {
        List<TElement> elements = [];

        await foreach (T element in _queryService.GetPaginatedAsync<T, TElement>(query, PAGE_LIMIT, typeInfo, ct: ct))
            elements.AddRange(element);

        return elements;
    }
}
