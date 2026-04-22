using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Crosshair pointer appearance — two thin lines with a gap in the center.
/// This is the default pointer appearance.
/// </summary>
public record HudCrosshairAppearance : HudAppearance<HudPointer>
{
    private static readonly SKPaint StrokeDark = new()
    {
        IsAntialias = true, Style = SKPaintStyle.Stroke,
        StrokeWidth = 2.5f, Color = new SKColor(0, 0, 0, 180)
    };

    private static readonly SKPaint StrokeLight = new()
    {
        IsAntialias = true, Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.5f, Color = SKColors.White
    };

    public static HudCrosshairAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudPointer pointer)
    {
        byte alpha = (byte)(255 * Math.Clamp(pointer.Alpha, 0f, 1f));
        if (alpha == 0) return;

        float size = pointer.IsDown ? 6f : 8f;
        float gap = pointer.IsDown ? 1.5f : 2.5f;

        StrokeDark.Color = new SKColor(0, 0, 0, (byte)(180 * alpha / 255));
        canvas.DrawLine(-size, 0, -gap, 0, StrokeDark);
        canvas.DrawLine(gap, 0, size, 0, StrokeDark);
        canvas.DrawLine(0, -size, 0, -gap, StrokeDark);
        canvas.DrawLine(0, gap, 0, size, StrokeDark);

        StrokeLight.Color = SKColors.White.WithAlpha(alpha);
        canvas.DrawLine(-size, 0, -gap, 0, StrokeLight);
        canvas.DrawLine(gap, 0, size, 0, StrokeLight);
        canvas.DrawLine(0, -size, 0, -gap, StrokeLight);
        canvas.DrawLine(0, gap, 0, size, StrokeLight);
    }
}

/// <summary>
/// Dot pointer appearance — small filled circle with a contrasting outline.
/// </summary>
public record HudDotAppearance : HudAppearance<HudPointer>
{
    private static readonly SKPaint FillLight = new()
    {
        IsAntialias = true, Style = SKPaintStyle.Fill, Color = SKColors.White
    };

    private static readonly SKPaint FillDark = new()
    {
        IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(0, 0, 0, 180)
    };

    public static HudDotAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudPointer pointer)
    {
        byte alpha = (byte)(255 * Math.Clamp(pointer.Alpha, 0f, 1f));
        if (alpha == 0) return;

        float r = pointer.IsDown ? 2f : 3f;
        FillDark.Color = new SKColor(0, 0, 0, (byte)(180 * alpha / 255));
        canvas.DrawCircle(0, 0, r + 1f, FillDark);
        FillLight.Color = SKColors.White.WithAlpha(alpha);
        canvas.DrawCircle(0, 0, r, FillLight);
    }
}

/// <summary>
/// Ring pointer appearance — hollow circle that shrinks on press.
/// </summary>
public record HudRingAppearance : HudAppearance<HudPointer>
{
    private static readonly SKPaint StrokeDark = new()
    {
        IsAntialias = true, Style = SKPaintStyle.Stroke,
        StrokeWidth = 2.5f, Color = new SKColor(0, 0, 0, 180)
    };

    private static readonly SKPaint StrokeLight = new()
    {
        IsAntialias = true, Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.5f, Color = SKColors.White
    };

    public static HudRingAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudPointer pointer)
    {
        byte alpha = (byte)(255 * Math.Clamp(pointer.Alpha, 0f, 1f));
        if (alpha == 0) return;

        float r = pointer.IsDown ? 4f : 6f;
        StrokeDark.Color = new SKColor(0, 0, 0, (byte)(180 * alpha / 255));
        canvas.DrawCircle(0, 0, r, StrokeDark);
        StrokeLight.Color = SKColors.White.WithAlpha(alpha);
        canvas.DrawCircle(0, 0, r, StrokeLight);
    }
}

/// <summary>Provides <c>HudPointerAppearance.Default</c> as an alias for <see cref="HudCrosshairAppearance"/>.</summary>
public static class HudPointerAppearance
{
    /// <summary>The default pointer appearance (crosshair).</summary>
    public static HudAppearance<HudPointer> Default { get; } = HudCrosshairAppearance.Default;
}
