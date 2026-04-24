using SkiaSharp;

namespace SkiaSharpGames.TwoZeroFourEight;

internal static class TwoZeroFourEightConstants
{
    public const int GridSize = 4;
    public const float GameWidth = 900f;
    public const float GameHeight = 600f;

    public const float TileSize = 110f;
    public const float TileGap = 14f;
    public const float BoardPadding = 16f;

    public static float BoardPixelSize =>
        GridSize * TileSize + (GridSize - 1) * TileGap + BoardPadding * 2f;
    public static float BoardX => (GameWidth - BoardPixelSize) / 2f;
    public static float BoardY => 96f;

    public static readonly SKColor BackgroundColor = new(0xFA, 0xF8, 0xEF);
    public static readonly SKColor HeaderColor = new(0x77, 0x6E, 0x65);
    public static readonly SKColor BoardColor = new(0xBB, 0xAD, 0xA0);
    public static readonly SKColor EmptyTileColor = new(0xCD, 0xC1, 0xB4);

    public static readonly SKColor DarkTextColor = new(0x77, 0x6E, 0x65);
    public static readonly SKColor LightTextColor = new(0xF9, 0xF6, 0xF2);
}