using PS2MapTool.Services.Abstractions;
using PS2MapTool.Tiles;
using System;
using System.Collections.Generic;

namespace PS2MapTool.Services
{
    public sealed class TileProcessorServiceRepository
    {
        private readonly Dictionary<TileImageFormatType, ITileProcessorService> _repository;

        public TileProcessorServiceRepository()
        {
            _repository = new Dictionary<TileImageFormatType, ITileProcessorService>();
        }

        public void Add<T>(TileImageFormatType type, T processor) where T : ITileProcessorService
        {
            if (_repository.ContainsKey(type))
                throw new ArgumentException("A processor of that type has already been added");

            _repository.Add(type, processor);
        }

        public ITileProcessorService Get(TileImageFormatType type) => _repository[type];
    }
}
