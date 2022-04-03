using System;
using System.Diagnostics.CodeAnalysis;

namespace PS2MapTool.Tiles;

public static class TileHelpers
{
    public static bool TryParseName
    (
        string tileName,
        [NotNullWhen(true)] out string? worldName,
        out int x,
        out int y,
        out Lod lod,
        [NotNullWhen(true)] out string? fileExtension
    )
    {
        worldName = null;
        x = 0;
        y = 0;
        lod = Lod.Lod0;
        fileExtension = null;

        // Get the extension, if available
        string[] extensionComponents = tileName.Split('.');
        string[] nameComponents = extensionComponents[0].Split('_');

        if (extensionComponents.Length != 2)
            return false;
        fileExtension = extensionComponents[1];

        int tileComponentIndex = Array.IndexOf(nameComponents, "Tile");
        int worldNameEndIndex = tileComponentIndex;

        if (tileComponentIndex == -1)
            return false;

        if (nameComponents.Length != 4 + tileComponentIndex)
            return false;

        if (nameComponents[tileComponentIndex++] != "Tile")
            return false;

        if (!int.TryParse(nameComponents[tileComponentIndex++], out x))
            return false;

        if (!int.TryParse(nameComponents[tileComponentIndex++], out y))
            return false;

        if (!Enum.TryParse(nameComponents[tileComponentIndex], true, out lod))
            return false;

        worldName = string.Join("_", nameComponents[..worldNameEndIndex]);

        return true;
    }
}
