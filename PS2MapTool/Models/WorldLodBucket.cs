using System;
using System.Collections.Generic;
using System.Linq;

namespace PS2MapTool.Models
{
    public class WorldLodBucket
    {
        private readonly Dictionary<string, Dictionary<string, List<Tile>>> _lodBucket;
        private readonly IEnumerable<string>? _worldsToIgnore;
        private readonly IEnumerable<string>? _lodsToIgnore;

        /// <summary>
        /// Initialises a new instance of the <see cref="WorldLodBucket"/> object.
        /// </summary>
        /// <param name="worlds">Only allows tiles for these worlds to be added.</param>
        /// <param name="lods">Only allows tiles for these LODs to be added.</param>
        public WorldLodBucket(IEnumerable<string>? worlds = null, IEnumerable<string>? lods = null)
        {
            _worldsToIgnore = worlds;
            _lodsToIgnore = lods;

            _lodBucket = new Dictionary<string, Dictionary<string, List<Tile>>>();
        }

        /// <summary>
        /// Adds a tile to the bucket.
        /// </summary>
        /// <param name="tile">The tile to add.</param>
        /// <returns>A value indicating if the tile was added.</returns>
        public bool AddLodTile(Tile tile)
        {
            // Discard the tile if we shouldn't be generating maps for its world
            if (_worldsToIgnore?.Contains(tile.World) == false)
                return false;

            // Discard the tile if we shouldn't be generating maps for its LOD
            if (_lodsToIgnore?.Contains(tile.Lod) == false)
                return false;

            // Ensure that the LOD bucket exists for this world
            if (!_lodBucket.ContainsKey(tile.World))
                _lodBucket[tile.World] = new Dictionary<string, List<Tile>>();

            // Ensure that the tile bucket exists for this LOD
            if (!_lodBucket[tile.World].ContainsKey(tile.Lod))
                _lodBucket[tile.World][tile.Lod] = new List<Tile>();

            _lodBucket[tile.World][tile.Lod].Add(tile);
            return true;
        }

        /// <summary>
        /// Gets all the worlds that this bucket is storing tiles of.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetWorlds() => _lodBucket.Keys;

        /// <summary>
        /// Gets all the LODs for a world that this bucket is storing tiles of.
        /// </summary>
        /// <param name="world">The world to get the LODs of.</param>
        /// <returns></returns>
        public IEnumerable<string> GetLods(string world) => _lodBucket[world].Keys;

        /// <summary>
        /// Gets the stored tiles for a particular world/LOD.
        /// </summary>
        /// <param name="world">The world to get tiles for.</param>
        /// <param name="lod">The LOD of the world to get tiles for."/></param>
        /// <returns></returns>
        public IEnumerable<Tile> GetTiles(string world, string lod)
        {
            if (!_lodBucket.ContainsKey(world))
                throw new ArgumentException("No tiles for that world have been stored in this bucket.", nameof(world));

            if (!_lodBucket[world].ContainsKey(lod))
                throw new ArgumentException("No tiles for the LOD of that world have been stored in this bucket.", nameof(lod));

            return _lodBucket[world][lod];
        }
    }
}
