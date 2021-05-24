# PS2MapTool

Each of PlanetSide 2's continent maps is stored in the game files as a number of 256x256 tiles, for various levels of detail. This is a tool to manipulate and stitch together those tiles into a full map.

### Usage

1. Ensure that the [.NET 5 Runtime](https://dotnet.microsoft.com/download) is installed.
2. Grab a copy of PS2MapTool from the [releases](https://github.com/carlst99/PS2MapTool/releases). Only Windows x64 releases are available.
3. Open your favourite terminal and run the following command. The built-in help will guide you from here.
    ```
    PS2MapTool.exe
    ```
4. :warning: See [Tile Extraction](#tile-extraction)


#### Tile Extraction

You will first need to extract the tiles from the game assets. The capability to do this will be coming to this tool, but in the meantime I suggest you use Rhett's [forgelight-toolbox](https://github.com/RhettVX/forgelight-toolbox).

Tiles can be found in the world data packs, which have the naming format `<World>_x64_(0-9).pack2`. Each tile is a DDS image file, named in the format `<World>_Tile_<YPos>_<XPos>_LOD(0-3).dds`. Take all the tiles you want, and place them into one folder.

### Tile/Stitching Information

Each tile is 256x256 pixels in dimension. Tiles are present for four levels of detail (LODs), ranging from 0-3. LOD3 is comprised of 16 tiles, with each LOD increasing the tile count by four times, up to 1024 tiles for LOD0.

To form a map from the tiles, the following process is used

1. Rotate each tile by 270 degrees clockwise.
2. Stitch the tiles together in order of decreasing X value and increase Y value going left->right, top->bottom, i.e
    | (x-, y-) | (x+, y-) |
    |----------|----------|
    | (x-, y+) | (x+, y+) |
3. Rotate the final product by 90 degrees clockwise and flip it vertically.

### Acknowledgements

PS2MapTool is built using the following libraries:

- [CliFx](https://github.com/Tyrrrz/CliFx)
- [Pfim](https://github.com/nickbabcock/Pfim)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)
- [Spectre.Console](https://github.com/spectreconsole/spectre.console)