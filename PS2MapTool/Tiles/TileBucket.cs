using System;
using System.Collections.Generic;

namespace PS2MapTool.Tiles
{
    /// <summary>
    /// A dictionary wrapper for storing map tiles.
    /// </summary>
    public class TileBucket
    {
        private readonly Dictionary<World, Dictionary<Lod, List<TileInfo>>> _lodBucket;

        /// <summary>
        /// Initialises a new instance of the <see cref="WorldLodBucket"/> object.
        /// </summary>
        public TileBucket()
        {
            _lodBucket = new Dictionary<World, Dictionary<Lod, List<TileInfo>>>();
        }

        /// <summary>
        /// Adds a tile to the bucket.
        /// </summary>
        /// <param name="tile">The tile to add.</param>
        /// <returns>A value indicating if the tile was added.</returns>
        public bool AddLodTile(TileInfo tile)
        {
            // Ensure that the LOD bucket exists for this world
            if (!_lodBucket.ContainsKey(tile.World))
                _lodBucket[tile.World] = new Dictionary<Lod, List<TileInfo>>();

            // Ensure that the tile bucket exists for this LOD
            if (!_lodBucket[tile.World].ContainsKey(tile.Lod))
                _lodBucket[tile.World][tile.Lod] = new List<TileInfo>();

            _lodBucket[tile.World][tile.Lod].Add(tile);
            return true;
        }

        /// <summary>
        /// Gets all the worlds that this bucket is storing tiles of.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<World> GetWorlds() => _lodBucket.Keys;

        /// <summary>
        /// Gets all the LODs for a world that this bucket is storing tiles of.
        /// </summary>
        /// <param name="world">The world to get the LODs of.</param>
        /// <returns></returns>
        public IEnumerable<Lod> GetLods(World world) => _lodBucket[world].Keys;

        /// <summary>
        /// Gets the stored tiles for a particular world/LOD.
        /// </summary>
        /// <param name="world">The world to get tiles for.</param>
        /// <param name="lod">The LOD of the world to get tiles for."/></param>
        /// <returns></returns>
        public IEnumerable<TileInfo> GetTiles(World world, Lod lod)
        {
            if (!_lodBucket.ContainsKey(world))
                throw new ArgumentException("No tiles for that world have been stored in this bucket.", nameof(world));

            if (!_lodBucket[world].ContainsKey(lod))
                throw new ArgumentException("No tiles for the LOD of that world have been stored in this bucket.", nameof(lod));

            return _lodBucket[world][lod];
        }
    }
}
