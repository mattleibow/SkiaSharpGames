using SkiaSharp;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>All layout, physics, and colour constants for the Breakout game.</summary>
internal static class BreakoutConstants
{
    // ── Game dimensions ───────────────────────────────────────────────────
    public const int GameWidth  = 800;
    public const int GameHeight = 600;

    // ── Paddle ────────────────────────────────────────────────────────────
    public const float DefaultPaddleWidth  = 100f;
    public const float PaddleHeight        = 14f;
    public const float PaddleY             = 558f;
    public const float BigPaddleMultiplier = 1.8f;
    public const float BigPaddleDuration   = 8f;

    // ── Ball ──────────────────────────────────────────────────────────────
    public const float BallRadius         = 8f;
    public const float BallSpeed          = 350f;
    public const float StrongBallDuration = 5f;

    // ── Bricks ────────────────────────────────────────────────────────────
    public const int   BrickCols   = 10;
    public const int   BrickRows   = 5;
    public const float BrickWidth  = 72f;
    public const float BrickHeight = 22f;
    public const float BrickGap    = 4f;
    public const float BricksStartY = 60f;

    public static readonly float BricksStartX =
        (GameWidth - (BrickCols * (BrickWidth + BrickGap) - BrickGap)) / 2f;

    // ── Power-ups ─────────────────────────────────────────────────────────
    public const float PowerUpChance = 0.15f;
    public const float PowerUpSpeed  = 130f;
    public const float PowerUpW      = 34f;
    public const float PowerUpH      = 18f;

    // ── Colours ───────────────────────────────────────────────────────────
    public static readonly SKColor BackgroundColor = new(0x0D, 0x1B, 0x2A);
    public static readonly SKColor PaddleColor     = new(0x00, 0xD4, 0xFF);
    public static readonly SKColor AccentColor     = new(0x00, 0xD4, 0xFF);
    public static readonly SKColor DimColor        = new(0xAA, 0xAA, 0xAA);
    public static readonly SKColor StrongBallColor = new(0xFF, 0x6B, 0x00);
    public static readonly SKColor BigPaddleColor  = new(0x00, 0xE5, 0x76);

    public static readonly SKColor[] BrickColors =
    [
        new SKColor(0xFF, 0x2D, 0x55), // Red    (row 0 – top, highest score)
        new SKColor(0xFF, 0x9F, 0x0A), // Orange
        new SKColor(0xFF, 0xD6, 0x0A), // Yellow
        new SKColor(0x30, 0xD1, 0x58), // Green
        new SKColor(0x0A, 0x84, 0xFF), // Blue   (row 4 – bottom)
    ];
}
