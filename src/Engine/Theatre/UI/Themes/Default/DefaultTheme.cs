namespace SkiaSharp.Theatre;

/// <summary>
/// Factory for the default UI theme. All appearances use their built-in defaults.
/// </summary>
public static class DefaultTheme
{
    /// <summary>
    /// Creates a new <see cref="HudTheme"/> with default appearances for all controls.
    /// </summary>
    public static HudTheme Create() => new();
}
