using PS2MapTool.Abstractions.Tiles.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PS2MapTool.Tiles.Services;

/// <inheritdoc cref="ITileLoaderService" />
public class PngTileLoaderService : ITileLoaderService
{
    public static readonly byte[] PNG_MAGIC_ID = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    /// <inheritdoc />
    public bool CanLoad(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < PNG_MAGIC_ID.Length)
            return false;

        for (int i = 0; i < PNG_MAGIC_ID.Length; i++)
        {
            if (buffer[i] != PNG_MAGIC_ID[i])
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    /// <returns>An <see cref="Image{Rgba32}"/>.</returns>
    public virtual Image Load(ReadOnlySpan<byte> buffer)
        => Image.Load<Rgba32>(buffer);
}
