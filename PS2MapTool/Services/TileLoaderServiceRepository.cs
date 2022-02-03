using PS2MapTool.Abstractions.Tiles;
using PS2MapTool.Abstractions.Tiles.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool.Services;

public sealed class TileLoaderServiceRepository
{
    private readonly List<ITileLoaderService> _repository;

    public TileLoaderServiceRepository()
    {
        _repository = new List<ITileLoaderService>();
    }

    public void Add<T>(T processor) where T : ITileLoaderService
        => _repository.Add(processor);

    /// <summary>
    /// Tries to get a <see cref="ITileLoaderService"/> that can load tiles of the given format.
    /// </summary>
    /// <param name="tile">The tile data to load.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>An applicable <see cref="ITileLoaderService"/>, or null if none can be used to load the given tile.</returns>
    public async Task<ITileLoaderService?> TryGetAsync(ITileDataSource tile, CancellationToken ct = default)
    {
        foreach (ITileLoaderService loader in _repository)
        {
            bool canLoad = await loader.CanLoadAsync(tile, ct).ConfigureAwait(false);

            if (canLoad)
                return loader;
        }

        return null;
    }

    public IReadOnlyList<ITileLoaderService> GetAll() => _repository.AsReadOnly();
}
