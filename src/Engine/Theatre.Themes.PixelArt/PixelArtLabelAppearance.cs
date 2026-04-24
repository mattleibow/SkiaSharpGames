using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.PixelArt;

/// <summary>
/// Pixel-art label appearance — same text rendering but with aliased (pixel-sharp)
/// font edging instead of anti-aliased.
/// </summary>
public record PixelArtLabelAppearance : HudAppearance<HudLabel>
{
    private static readonly FontProvider Fonts = new() { Edging = SKFontEdging.Alias };
    private static readonly SKPaint TextPaint = new() { IsAntialias = false };

    public static PixelArtLabelAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudLabel label)
    {
        if (label.Alpha <= 0f || string.IsNullOrEmpty(label.Text))
            return;

        var font = Fonts.GetFont(label.FontSize);
        TextPaint.Color = label.Color.WithAlpha((byte)(255 * Math.Clamp(label.Alpha, 0f, 1f)));

        float drawX = label.Align switch
        {
            TextAlign.Center => -font.MeasureText(label.Text) / 2f,
            TextAlign.Right => -font.MeasureText(label.Text),
            _ => 0f,
        };

        canvas.DrawText(label.Text, drawX, 0f, font, TextPaint);
    }
}
