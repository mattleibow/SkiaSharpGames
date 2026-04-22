using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Pixel-art label appearance — same text rendering but with aliased (pixel-sharp)
/// font edging instead of anti-aliased.
/// </summary>
public record PixelArtLabelAppearance : UiAppearance<UiLabel>
{
    private static readonly Dictionary<float, SKFont> FontCache = [];
    private static readonly SKPaint TextPaint = new() { IsAntialias = false };

    public static PixelArtLabelAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiLabel label)
    {
        if (label.Alpha <= 0f || string.IsNullOrEmpty(label.Text))
            return;

        var font = GetPixelFont(label.FontSize);
        TextPaint.Color = label.Color.WithAlpha((byte)(255 * Math.Clamp(label.Alpha, 0f, 1f)));

        float drawX = label.Align switch
        {
            TextAlign.Center => -font.MeasureText(label.Text) / 2f,
            TextAlign.Right => -font.MeasureText(label.Text),
            _ => 0f
        };

        canvas.DrawText(label.Text, drawX, 0f, font, TextPaint);
    }

    private static SKFont GetPixelFont(float size)
    {
        float key = MathF.Round(size, 2);
        if (!FontCache.TryGetValue(key, out var font))
        {
            font = new SKFont(SKTypeface.Default, key) { Edging = SKFontEdging.Alias };
            FontCache[key] = font;
        }
        return font;
    }
}
