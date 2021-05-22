namespace PS2MapTools
{
    public struct Tile
    {
        public string Path { get; private set; }
        public string World { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public string LOD { get; private set; }

        public static bool TryParse(string filePath, out Tile tile)
        {
            tile = new Tile();
            string[] nameComponents = System.IO.Path.GetFileNameWithoutExtension(filePath).Split('_');

            if (nameComponents.Length != 5)
                return false;

            if (nameComponents[1] != "Tile")
                return false;

            if (!int.TryParse(nameComponents[3], out int y))
                return false;

            if (!int.TryParse(nameComponents[2], out int x))
                return false;

            tile.Path = filePath;
            tile.World = nameComponents[0];
            tile.X = x;
            tile.Y = y;
            tile.LOD = nameComponents[4];

            return true;
        }
    }
}
