using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Appearance for <see cref="UiLabel"/>. Owns visual properties and draw logic.
/// Handles font caching, alignment, colour, and alpha.
/// </summary>
public record UiLabelAppearance : UiAppearance<UiLabel>
{
    private static readonly Dictionary<float, SKFont> FontCache = [];

    public static UiLabelAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiLabel label)
    {
        if (label.Alpha <= 0f || string.IsNullOrEmpty(label.Text))
            return;

        var font = GetFont(label.FontSize);
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Color = label.Color.WithAlpha((byte)(255 * Math.Clamp(label.Alpha, 0f, 1f)))
        };

        float drawX = label.Align switch
        {
            TextAlign.Center => -font.MeasureText(label.Text) / 2f,
            TextAlign.Right => -font.MeasureText(label.Text),
            _ => 0f
        };

        canvas.DrawText(label.Text, drawX, 0f, font, paint);
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
