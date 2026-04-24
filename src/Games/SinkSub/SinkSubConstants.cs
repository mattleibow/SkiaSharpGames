using SkiaSharp;

namespace SkiaSharpGames.SinkSub;

internal static class SinkSubConstants
{
    public const float GameWidth = 900f;
    public const float GameHeight = 600f;

    public const float WaterlineY = 118f;
    public const float ShipY = 64f;
    public const float ShipWidth = 124f;
    public const float ShipHeight = 24f;
    public const float ShipSpeed = 320f;

    public const float ChargeRadius = 6f;
    public const float ChargeSpeed = 185f;
    public const float ChargeSpawnOffsetX = 38f;
    public const float ChargeSpawnOffsetY = 16f;
    public const int MaxCharges = 4;

    public const float SubWidth = 88f;
    public const float SubHeight = 24f;
    public const float SubMinY = 220f;
    public const float SubMaxY = 470f;

    public const float MineRadius = 7f;
    public const float MineSpeed = 78f;

    public static readonly SKColor SkyColor = new(0x8C, 0xD3, 0xFF);
    public static readonly SKColor WaterColor = new(0x0E, 0x4D, 0x92);
    public static readonly SKColor DeepWaterColor = new(0x08, 0x2E, 0x5A);
    public static readonly SKColor AccentColor = new(0xFF, 0xD6, 0x0A);
    public static readonly SKColor DimColor = new(0xDB, 0xE9, 0xF7);
}