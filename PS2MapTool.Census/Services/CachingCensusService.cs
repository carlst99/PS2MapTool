using DbgCensus.Rest.Abstractions;

namespace PS2MapTool.Census.Services
{
    public class CachingCensusService : CensusService
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CensusService"/> class.
        /// </summary>
        /// <param name="queryService">The query service.</param>
        public CachingCensusService(IQueryService queryService)
            : base(queryService)
        {
        }
    }
}
