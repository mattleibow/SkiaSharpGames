using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.Neon;

/// <summary>
/// Neon/cyberpunk crosshair pointer — glowing crosshair lines with neon color and blur.
/// </summary>
public record NeonPointerAppearance : HudAppearance<HudPointer>
{
    private static readonly SKMaskFilter GlowFilter = SKMaskFilter.CreateBlur(
        SKBlurStyle.Normal,
        3f
    );

    public SKColor NeonColor { get; init; } = new(0x00, 0xFF, 0xFF);
    public float StrokeWidth { get; init; } = 1.5f;
    public byte GlowAlpha { get; init; } = 120;

    public static NeonPointerAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudPointer pointer)
    {
        float size = pointer.IsDown ? 7f : 10f;
        float gap = pointer.IsDown ? 2f : 3f;

        // Glow pass
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = StrokeWidth + 2f,
            Color = NeonColor.WithAlpha(GlowAlpha),
            MaskFilter = GlowFilter,
        };
        DrawCrosshair(canvas, size, gap, glowPaint);

        // Solid pass
        glowPaint.MaskFilter = null;
        glowPaint.StrokeWidth = StrokeWidth;
        glowPaint.Color = NeonColor;
        DrawCrosshair(canvas, size, gap, glowPaint);

        // Center dot glow
        using var dotPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = NeonColor.WithAlpha(GlowAlpha),
            MaskFilter = GlowFilter,
        };
        canvas.DrawCircle(0, 0, 1.5f, dotPaint);

        dotPaint.MaskFilter = null;
        dotPaint.Color = NeonColor;
        canvas.DrawCircle(0, 0, 0.8f, dotPaint);
    }

    private static void DrawCrosshair(SKCanvas canvas, float size, float gap, SKPaint paint)
    {
        canvas.DrawLine(-size, 0, -gap, 0, paint);
        canvas.DrawLine(gap, 0, size, 0, paint);
        canvas.DrawLine(0, -size, 0, -gap, paint);
        canvas.DrawLine(0, gap, 0, size, paint);
    }
}
