using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Pixel-art pointer appearance — simple 4px cross with no gap, 1px thick lines.
/// No anti-aliasing for crisp pixel edges.
/// </summary>
public record PixelArtPointerAppearance : UiAppearance<Spotlight>
{
    private static readonly SKPaint ShadowPaint = new()
    {
        IsAntialias = false, Style = SKPaintStyle.Stroke
    };
    private static readonly SKPaint MainPaint = new()
    {
        IsAntialias = false, Style = SKPaintStyle.Stroke
    };

    public SKColor Color { get; init; } = new(0xF5, 0xE6, 0xB8);
    public SKColor ShadowColor { get; init; } = new(0x1A, 0x1A, 0x0E, 180);
    public float Size { get; init; } = 4f;
    public float StrokeWidth { get; init; } = 1f;

    public static PixelArtPointerAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, Spotlight pointer)
    {
        byte alpha = (byte)(255 * Math.Clamp(pointer.Alpha, 0f, 1f));
        if (alpha == 0) return;

        float s = pointer.IsDown ? Size * 0.75f : Size;

        ShadowPaint.StrokeWidth = StrokeWidth + 1f;
        ShadowPaint.Color = ShadowColor.WithAlpha((byte)(ShadowColor.Alpha * alpha / 255));

        MainPaint.StrokeWidth = StrokeWidth;
        MainPaint.Color = Color.WithAlpha(alpha);

        // Shadow layer
        canvas.DrawLine(-s, 0, s, 0, ShadowPaint);
        canvas.DrawLine(0, -s, 0, s, ShadowPaint);

        // Foreground cross — no gap
        canvas.DrawLine(-s, 0, s, 0, MainPaint);
        canvas.DrawLine(0, -s, 0, s, MainPaint);
    }
}
