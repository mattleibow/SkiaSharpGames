using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Pixel-art checkbox appearance — sharp square with a blocky X checkmark
/// made of small rectangles. No anti-aliasing, no rounded corners.
/// </summary>
public record PixelArtCheckboxAppearance : UiAppearance<UiCheckbox>
{
    public SKColor FillColor { get; init; } = new(0x1A, 0x1A, 0x0E);
    public SKColor BorderColor { get; init; } = new(0xC8, 0xA8, 0x52);
    public SKColor CheckColor { get; init; } = new(0xA2, 0xD9, 0x4A);
    public float BorderWidth { get; init; } = 3f;

    public static PixelArtCheckboxAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiCheckbox checkbox)
    {
        var rect = checkbox.LocalRect;

        using var fillPaint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Stroke };

        // Background fill
        fillPaint.Color = FillColor;
        canvas.DrawRect(rect, fillPaint);

        // Border
        strokePaint.StrokeWidth = BorderWidth;
        strokePaint.Color = BorderColor;
        canvas.DrawRect(rect, strokePaint);

        if (!checkbox.IsChecked)
            return;

        // Blocky X checkmark — two diagonal lines drawn with square stroke caps
        strokePaint.Color = CheckColor;
        strokePaint.StrokeCap = SKStrokeCap.Butt;
        strokePaint.StrokeWidth = MathF.Max(3f, rect.Width * 0.14f);

        float inset = rect.Width * 0.22f;
        canvas.DrawLine(rect.Left + inset, rect.Top + inset,
            rect.Right - inset, rect.Bottom - inset, strokePaint);
        canvas.DrawLine(rect.Right - inset, rect.Top + inset,
            rect.Left + inset, rect.Bottom - inset, strokePaint);
    }
}
