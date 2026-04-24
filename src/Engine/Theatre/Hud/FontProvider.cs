namespace SkiaSharp.Theatre;

/// <summary>
/// Per-theme font configuration with a size-keyed cache. Used by
/// <see cref="HudTheme"/> to provide fonts for label measurement and rendering.
/// </summary>
public class FontProvider
{
    /// <summary>Shared default provider (system typeface, antialias edging).</summary>
    public static FontProvider Default { get; } = new();

    private readonly Dictionary<float, SKFont> _cache = [];

    /// <summary>The typeface used to create fonts. Defaults to the system default.</summary>
    public SKTypeface Typeface { get; init; } = SKTypeface.Default;

    /// <summary>Font edging mode. Defaults to antialias.</summary>
    public SKFontEdging Edging { get; init; } = SKFontEdging.Antialias;

    /// <summary>
    /// Gets or creates a cached <see cref="SKFont"/> for the given size.
    /// </summary>
    public SKFont GetFont(float size)
    {
        float key = MathF.Round(size, 2);
        if (!_cache.TryGetValue(key, out var font))
        {
            font = new SKFont(Typeface, key) { Edging = Edging };
            _cache[key] = font;
        }
        return font;
    }
}