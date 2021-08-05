using PS2MapTool.Services.Abstractions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PS2MapTool.Services
{
    public sealed class TileLoaderServiceRepository
    {
        private readonly List<ITileLoaderService> _repository;

        public TileLoaderServiceRepository()
        {
            _repository = new List<ITileLoaderService>();
        }

        public void Add<T>(T processor) where T : ITileLoaderService
        {
            _repository.Add(processor);
        }

        /// <summary>
        /// Tries to get a <see cref="ITileLoaderService"/> that can load tiles of the given format.
        /// </summary>
        /// <param name="tileDataSource">The tile data to load.</param>
        /// <param name="tileLoaderService">The resolved processor service.</param>
        /// <returns>A value indicating if a processor service was found.</returns>
        public bool TryGet(Stream tileDataSource, [NotNullWhen(true)] out ITileLoaderService? tileLoaderService)
        {
            foreach (ITileLoaderService loader in _repository)
            {
                if (loader.CanLoad(tileDataSource))
                {
                    tileLoaderService = loader;
                    return true;
                }
            }

            tileLoaderService = null;
            return false;
        }

        public IReadOnlyList<ITileLoaderService> GetAll() => _repository.AsReadOnly();
    }
}
