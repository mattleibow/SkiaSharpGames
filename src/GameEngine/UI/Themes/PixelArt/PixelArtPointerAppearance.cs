using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Pixel-art pointer appearance — simple 4px cross with no gap, 1px thick lines.
/// No anti-aliasing for crisp pixel edges.
/// </summary>
public record PixelArtPointerAppearance : UiAppearance<UiPointer>
{
    public SKColor Color { get; init; } = new(0xF5, 0xE6, 0xB8);
    public SKColor ShadowColor { get; init; } = new(0x1A, 0x1A, 0x0E, 180);
    public float Size { get; init; } = 4f;
    public float StrokeWidth { get; init; } = 1f;

    public static PixelArtPointerAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiPointer pointer)
    {
        using var shadowPaint = new SKPaint
        {
            IsAntialias = false,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = StrokeWidth + 1f,
            Color = ShadowColor
        };

        using var paint = new SKPaint
        {
            IsAntialias = false,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = StrokeWidth,
            Color = Color
        };

        float s = pointer.IsDown ? Size * 0.75f : Size;

        // Shadow layer
        canvas.DrawLine(-s, 0, s, 0, shadowPaint);
        canvas.DrawLine(0, -s, 0, s, shadowPaint);

        // Foreground cross — no gap
        canvas.DrawLine(-s, 0, s, 0, paint);
        canvas.DrawLine(0, -s, 0, s, paint);
    }
}
