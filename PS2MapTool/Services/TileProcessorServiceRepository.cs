using PS2MapTool.Services.Abstractions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PS2MapTool.Services
{
    public sealed class TileProcessorServiceRepository
    {
        private readonly List<ITileProcessorService> _repository;

        public TileProcessorServiceRepository()
        {
            _repository = new List<ITileProcessorService>();
        }

        public void Add<T>(T processor) where T : ITileProcessorService
        {
            _repository.Add(processor);
        }

        /// <summary>
        /// Tries to get a <see cref="ITileProcessorService"/> that can load tiles of the given format.
        /// </summary>
        /// <param name="tileDataSource">The tile data to load.</param>
        /// <param name="tileProcessorService">The resolved processor service.</param>
        /// <returns>A value indicating if a processor service was found.</returns>
        public bool TryGet(Stream tileDataSource, [NotNullWhen(true)] out ITileProcessorService? tileProcessorService)
        {
            foreach (ITileProcessorService processor in _repository)
            {
                if (processor.CanLoad(tileDataSource))
                {
                    tileProcessorService = processor;
                    return true;
                }
            }

            tileProcessorService = null;
            return false;
        }

        public IReadOnlyList<ITileProcessorService> GetAll() => _repository.AsReadOnly();
    }
}
