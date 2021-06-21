# PS2MapTool

This is a tool to create PlanetSide 2 continent maps from the data stored in the game assets. Current features:

- Stitching of continent map tiles
- Creation of no-deployment zone images

## Usage

1. Ensure that the [.NET 5 Runtime](https://dotnet.microsoft.com/download) is installed.
2. Grab a copy of PS2MapTool from the [releases](https://github.com/carlst99/PS2MapTool/releases). Only Windows x64 binaries are available.
3. Open your favourite terminal and run the following command in the folder you downloaded the binary to. The built-in help will guide you from here.
    ```
    PS2MapTool.exe
    ```
4. :warning: See [Map Data Extraction](#map-data-extraction)

### Map Data Extraction

You will first need to extract the map data from the game assets. The capability to do this will (eventually) be coming to this tool, but in the meantime I suggest you use Rhett's [forgelight-toolbox](https://github.com/RhettVX/forgelight-toolbox).

Game assets (`*.pack2` files) are found in the PlanetSide installation folder (`...\PlanetSide 2\Resources\Assets`).

Tiles can be found in the world data packs, which have the naming format `<World>_x64_(0-9).pack2`. Each tile is a DDS image file, named in the format `<World>_Tile_<YPos>_<XPos>_LOD(0-3).dds`.

Area definition files (for obtaining no-deploy maps) can be found in `data_x64_0.pack2`.

Place all the extracted tiles into one folder before using this tool to stitch them.

## Tile/Stitching Information

Each tile is 256x256 pixels in dimension. Tiles are present for four levels of detail (LODs), ranging from 0-3. LOD3 is comprised of 16 tiles, with each LOD multiplying the tile count by four, up to 1024 tiles for LOD0.

To form a map from the tiles, the following process is used

1. Rotate each tile by 270 degrees clockwise.
2. Stitch the tiles together in order of decreasing X value and increasing Y value going left->right, top->bottom, i.e
    (x+, y-) | (x-, y-)
    --- | ---
    (x+, y+) | (x-, y+)
3. Rotate the final product by 90 degrees clockwise and flip it vertically.

## No-deployment Zone Information

No-deploy zones are found in the *area definition* files, which have the naming format `<World>Areas.xml`. These XML files define the area of effect of every world modifier, such as no-deploy zones or gravity lifts. Be aware when reading them that they break XML schema by having multiple root-level objects.

Here's an example XML object for a **no-deploy** area:

```xml
<AreaDefinition id="744869687" name="Amerish.SO25.SundyArea" shape="sphere" x1="-2719.131348" y1="71.812500" z1="-309.012238" radius="100.000000">
    <Property type="SundererNoDeploy" id="1964854664" Requirement="200" FacilityId="222240" DeployableClientReqId="0" />
</AreaDefinition>
```

It is important to note that the `Type` attribute on the `Property` element for a no-deploy area will always be set to `SundererNoDeploy`, regardless of whether it is intended for Sunderers or ANTs. The `Requirement` attribute is used to differentiate, with the current values being:

- **200** - Sunderer no-deploy
- **2336** - ANT no-deploy

## Acknowledgements

PS2MapTool makes use of the following libraries:

- [CliFx](https://github.com/Tyrrrz/CliFx)
- [Pfim](https://github.com/nickbabcock/Pfim)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)
- [Spectre.Console](https://github.com/spectreconsole/spectre.console)