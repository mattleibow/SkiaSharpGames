using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Appearance for <see cref="HudLabel"/>. Owns visual properties and draw logic.
/// Handles font caching, alignment, colour, and alpha.
/// </summary>
public record DefaultLabelAppearance : HudAppearance<HudLabel>
{
    // Font cache is intentionally unbounded — the game loop is single-threaded and
    // games typically use only ~5-10 distinct font sizes, so growth is negligible.
    private static readonly Dictionary<float, SKFont> FontCache = [];
    private static readonly SKPaint TextPaint = new() { IsAntialias = true };

    public static DefaultLabelAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudLabel label)
    {
        if (label.Alpha <= 0f || string.IsNullOrEmpty(label.Text))
            return;

        var font = GetFont(label.FontSize);
        TextPaint.Color = label.Color.WithAlpha((byte)(255 * Math.Clamp(label.Alpha, 0f, 1f)));

        float drawX = label.Align switch
        {
            TextAlign.Center => -font.MeasureText(label.Text) / 2f,
            TextAlign.Right => -font.MeasureText(label.Text),
            _ => 0f
        };

        canvas.DrawText(label.Text, drawX, 0f, font, TextPaint);
    }

    /// <summary>
    /// Gets or creates a cached font for the given size.
    /// </summary>
    internal static SKFont GetFont(float size)
    {
        float key = MathF.Round(size, 2);
        if (!FontCache.TryGetValue(key, out var font))
        {
            font = new SKFont(SKTypeface.Default, key) { Edging = SKFontEdging.Antialias };
            FontCache[key] = font;
        }
        return font;
    }
}
