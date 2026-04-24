using SkiaSharp;

namespace SkiaSharpGames.LunarLander;

/// <summary>All layout, physics, and colour constants for the Lunar Lander game.</summary>
internal static class LunarLanderConstants
{
    // ── Stage dimensions ───────────────────────────────────────────────────
    public const int GameWidth = 800;
    public const int GameHeight = 600;

    // ── Physics ───────────────────────────────────────────────────────────
    public const float Gravity = 30f; // px/s²
    public const float ThrustForce = 70f; // px/s² when thrusting
    public const float RotationSpeed = 3f; // rad/s

    // ── Landing ───────────────────────────────────────────────────────────
    public const float MaxLandingSpeed = 40f; // safe landing threshold
    public const float MaxLandingAngle = 0.3f; // ~17°

    // ── Lander ────────────────────────────────────────────────────────────
    public const float LanderWidth = 30f;
    public const float LanderHeight = 24f;

    // ── Fuel ──────────────────────────────────────────────────────────────
    public const float FuelMax = 100f;
    public const float FuelBurnRate = 15f; // units/s when thrusting

    // ── Landing pad ───────────────────────────────────────────────────────
    public const float LandingPadWidth = 80f;
    public const float LandingPadHeight = 6f;

    // ── Terrain ───────────────────────────────────────────────────────────
    public const int TerrainSegments = 40;
    public const float TerrainMinY = 400f;
    public const float TerrainMaxY = 560f;

    // ── Stars ─────────────────────────────────────────────────────────────
    public const int StarCount = 80;

    // ── Colours ───────────────────────────────────────────────────────────
    public static readonly SKColor BackgroundColor = new(0x0A, 0x0A, 0x2A);
    public static readonly SKColor PadColor = new(0x30, 0xD1, 0x58);
    public static readonly SKColor LanderColor = SKColors.White;
    public static readonly SKColor FlameColor = new(0xFF, 0x9F, 0x0A);
    public static readonly SKColor FlameInnerColor = new(0xFF, 0xD6, 0x0A);
    public static readonly SKColor TerrainColor = new(0x55, 0x55, 0x66);
    public static readonly SKColor HudColor = SKColors.White;
    public static readonly SKColor DimColor = new(0xAA, 0xAA, 0xAA);
    public static readonly SKColor AccentColor = new(0x00, 0xD4, 0xFF);
    public static readonly SKColor DangerColor = new(0xFF, 0x2D, 0x55);
    public static readonly SKColor SuccessColor = new(0x30, 0xD1, 0x58);
}