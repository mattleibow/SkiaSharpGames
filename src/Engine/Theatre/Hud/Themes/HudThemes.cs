namespace SkiaSharp.Theatre;

/// <summary>
/// Aggregates all built-in themes. Each property creates a fresh
/// <see cref="HudTheme"/> instance suitable for use with
/// <see cref="HudTheme.ApplyFrom"/>.
/// </summary>
public static class HudThemes
{
    /// <summary>Default theme — all appearances at their built-in defaults.</summary>
    public static HudTheme Default => DefaultTheme.Create();

    /// <summary>Bold/cute pink colour variation of the default appearances.</summary>
    public static HudTheme BoldCute => BoldCuteTheme.Create();

    /// <summary>Retro earthy green/gold colour variation of the default appearances.</summary>
    public static HudTheme Retro => RetroTheme.Create();

    /// <summary>Pixel-art retro CRT-style theme with no anti-aliasing.</summary>
    public static HudTheme PixelArt => PixelArtTheme.Create();

    /// <summary>Neon/cyberpunk theme with glow effects.</summary>
    public static HudTheme Neon => NeonTheme.Create();

    /// <summary>Alias for <see cref="Default"/> — kept for backward compatibility.</summary>
    public static HudTheme Simple => Default;
}
