using PS2MapTool.Abstractions.Tiles.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
    /// <param name="buffer">The tile data to load.</param>
    /// <param name="loader">A loader that can handle the give tile data, or null if no valid loaders were found.</param>
    /// <returns>A value indicating whether or not a valid loader could be found.</returns>
    public bool TryGet(ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out ITileLoaderService? loader)
    {
        loader = null;

        foreach (ITileLoaderService l in _repository)
        {
            if (l.CanLoad(buffer))
            {
                loader = l;
                return true;
            }
        }

        return false;
    }

    public IReadOnlyList<ITileLoaderService> GetAll() => _repository.AsReadOnly();
}
