using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Neon/cyberpunk label — neon-colored text with optional glow behind it.
/// </summary>
public record NeonLabelAppearance : HudAppearance<HudLabel>
{
    private static readonly SKMaskFilter GlowFilter =
        SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2f);

    public byte GlowAlpha { get; init; } = 80;

    public static NeonLabelAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudLabel label)
    {
        if (label.Alpha <= 0f || string.IsNullOrEmpty(label.Text))
            return;

        var font = DefaultLabelAppearance.GetFont(label.FontSize);
        byte a = (byte)(255 * Math.Clamp(label.Alpha, 0f, 1f));

        float drawX = label.Align switch
        {
            TextAlign.Center => -font.MeasureText(label.Text) / 2f,
            TextAlign.Right => -font.MeasureText(label.Text),
            _ => 0f
        };

        // Glow pass behind text
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = label.Color.WithAlpha((byte)(GlowAlpha * a / 255)),
            MaskFilter = GlowFilter
        };
        canvas.DrawText(label.Text, drawX, 0f, font, glowPaint);

        // Solid text on top
        glowPaint.MaskFilter = null;
        glowPaint.Color = label.Color.WithAlpha(a);
        canvas.DrawText(label.Text, drawX, 0f, font, glowPaint);
    }
}
