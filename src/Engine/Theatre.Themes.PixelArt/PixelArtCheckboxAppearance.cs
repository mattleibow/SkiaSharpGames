using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.PixelArt;

/// <summary>
/// Pixel-art checkbox appearance — sharp square with a blocky X checkmark
/// made of small rectangles. No anti-aliasing, no rounded corners.
/// </summary>
public record PixelArtCheckboxAppearance : HudAppearance<HudCheckbox>
{
    private static readonly SKPaint FillPaint = new()
    {
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };
    private static readonly SKPaint StrokePaint = new()
    {
        IsAntialias = false,
        Style = SKPaintStyle.Stroke,
    };

    public SKColor FillColor { get; init; } = new(0x1A, 0x1A, 0x0E);
    public SKColor BorderColor { get; init; } = new(0xC8, 0xA8, 0x52);
    public SKColor CheckColor { get; init; } = new(0xA2, 0xD9, 0x4A);
    public float BorderWidth { get; init; } = 3f;

    public static PixelArtCheckboxAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudCheckbox checkbox)
    {
        byte alpha = (byte)(255 * Math.Clamp(checkbox.Alpha, 0f, 1f));
        if (alpha == 0)
            return;

        var rect = checkbox.LocalRect;

        // Background fill
        FillPaint.Color = FillColor.WithAlpha(alpha);
        canvas.DrawRect(rect, FillPaint);

        // Border
        StrokePaint.StrokeWidth = BorderWidth;
        StrokePaint.Color = BorderColor.WithAlpha(alpha);
        canvas.DrawRect(rect, StrokePaint);

        if (!checkbox.IsChecked)
            return;

        // Blocky X checkmark — two diagonal lines drawn with square stroke caps
        StrokePaint.Color = CheckColor.WithAlpha(alpha);
        StrokePaint.StrokeCap = SKStrokeCap.Butt;
        StrokePaint.StrokeWidth = MathF.Max(3f, rect.Width * 0.14f);

        float inset = rect.Width * 0.22f;
        canvas.DrawLine(
            rect.Left + inset,
            rect.Top + inset,
            rect.Right - inset,
            rect.Bottom - inset,
            StrokePaint
        );
        canvas.DrawLine(
            rect.Right - inset,
            rect.Top + inset,
            rect.Left + inset,
            rect.Bottom - inset,
            StrokePaint
        );
    }
}