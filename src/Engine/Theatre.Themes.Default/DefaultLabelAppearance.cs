using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.Default;

/// <summary>
/// Appearance for <see cref="HudLabel"/>. Owns visual properties and draw logic.
/// Handles font caching, alignment, colour, and alpha.
/// </summary>
public record DefaultLabelAppearance : HudAppearance<HudLabel>
{
    private static readonly FontProvider Fonts = new();
    private static readonly SKPaint TextPaint = new() { IsAntialias = true };

    public static DefaultLabelAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudLabel label)
    {
        if (string.IsNullOrEmpty(label.Text))
            return;

        var font = Fonts.GetFont(label.FontSize);
        TextPaint.Color = label.Color;

        float drawX = label.Align switch
        {
            TextAlign.Center => -font.MeasureText(label.Text) / 2f,
            TextAlign.Right => -font.MeasureText(label.Text),
            _ => 0f,
        };

        canvas.DrawText(label.Text, drawX, 0f, font, TextPaint);
    }

    /// <summary>
    /// Gets or creates a cached font for the given size.
    /// </summary>
    internal static SKFont GetFont(float size) => Fonts.GetFont(size);
}
