using SkiaSharp;

namespace SkiaSharpGames.Snake;

internal static class SnakeConstants
{
    // ── Grid ──────────────────────────────────────────────────────────────
    public const int CellSize = 24;
    public const int GridCols = 25;
    public const int GridRows = 25;

    // ── Stage dimensions ───────────────────────────────────────────────────
    public const int GameWidth = CellSize * GridCols;   // 600
    public const int GameHeight = CellSize * GridRows;  // 600

    // ── Timing ────────────────────────────────────────────────────────────
    public const float InitialStepInterval = 0.15f;
    public const float MinStepInterval = 0.06f;
    public const float SpeedIncrement = 0.003f;

    // ── Colours ───────────────────────────────────────────────────────────
    public static readonly SKColor BackgroundColor = new(0x10, 0x14, 0x23);
    public static readonly SKColor GridLineColor = new(0x1A, 0x1F, 0x30);
    public static readonly SKColor SnakeHeadColor = new(0x5E, 0xD6, 0xFF);
    public static readonly SKColor SnakeBodyColor = new(0x3A, 0xA8, 0xD0);
    public static readonly SKColor FoodColor = new(0xFF, 0x7A, 0x7A);
    public static readonly SKColor AccentColor = new(0x5E, 0xD6, 0xFF);
    public static readonly SKColor DimColor = new(0xB8, 0xC2, 0xD9);
    public static readonly SKColor DangerColor = new(0xFF, 0x7A, 0x7A);
}
