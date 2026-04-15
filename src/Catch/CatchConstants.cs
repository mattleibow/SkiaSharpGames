using SkiaSharp;

namespace SkiaSharpGames.Catch;

internal static class CatchConstants
{
    // ── Game dimensions ───────────────────────────────────────────────────
    public const int GameWidth = 900;
    public const int GameHeight = 600;

    // ── Player bar ────────────────────────────────────────────────────────
    public const float BarWidth = 150f;
    public const float BarHeight = 18f;
    public const float BarY = GameHeight - 54f;
    public const float BarSpeed = 470f;

    // ── Falling circle ────────────────────────────────────────────────────
    public const float CircleRadius = 16f;
    public const float InitialFallSpeed = 165f;
    public const float FallSpeedIncrement = 16f;

    // ── Colours ───────────────────────────────────────────────────────────
    public static readonly SKColor BackgroundColor = new(0x10, 0x14, 0x23);
    public static readonly SKColor AccentColor = new(0x5E, 0xD6, 0xFF);
    public static readonly SKColor DimColor = new(0xB8, 0xC2, 0xD9);
    public static readonly SKColor DangerColor = new(0xFF, 0x7A, 0x7A);
}
