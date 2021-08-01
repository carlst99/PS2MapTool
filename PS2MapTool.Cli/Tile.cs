using PS2MapTool.Tiles;
using System;

namespace PS2MapTool.Cli
{
    [Obsolete("Use" + nameof(TileInfo), true)]
    public struct Tile
    {
        public string Path { get; private set; }
        public World World { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public Lod Lod { get; private set; }

        public static bool TryParse(string filePath, out Tile tile)
        {
            tile = new Tile();
            string[] nameComponents = System.IO.Path.GetFileNameWithoutExtension(filePath).Split('_');

            if (nameComponents.Length != 5)
                return false;

            if (!Enum.TryParse(nameComponents[0], out World world))
                return false;

            if (nameComponents[1] != "Tile")
                return false;

            if (!int.TryParse(nameComponents[2], out int x))
                return false;

            if (!int.TryParse(nameComponents[3], out int y))
                return false;

            if (!Enum.TryParse(nameComponents[4], out Lod lod))
                return false;

            tile.Path = filePath;
            tile.World = world;
            tile.X = x;
            tile.Y = y;
            tile.Lod = lod;

            return true;
        }

        public override string ToString()
        {
            return $"{World}_Tile_{Y}_{X}_{Lod}";
        }
    }
}
