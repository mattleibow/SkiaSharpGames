namespace SkiaSharp.Theatre;

/// <summary>
/// Configurable crosshair pointer appearance. A pair of crossed lines with a gap
/// in the center, drawn with a shadow stroke behind a coloured accent stroke.
/// Use <c>with { AccentColor = … }</c> to create game-specific variants.
/// </summary>
public record CrosshairPointerAppearance : HudAppearance<HudPointer>
{
    /// <summary>The bright foreground colour of the crosshair lines.</summary>
    public SKColor AccentColor { get; init; } = SKColors.White;

    /// <summary>The shadow/outline colour drawn behind the accent lines.</summary>
    public SKColor ShadowColor { get; init; } = new SKColor(0, 0, 0, 180);

    /// <summary>Width of the accent stroke.</summary>
    public float StrokeWidth { get; init; } = 1.5f;

    /// <summary>Width of the shadow stroke (should be larger than <see cref="StrokeWidth"/>).</summary>
    public float ShadowStrokeWidth { get; init; } = 2.5f;

    /// <summary>Half-length of each arm when the pointer is not pressed.</summary>
    public float Size { get; init; } = 8f;

    /// <summary>Half-length of the gap at the center when not pressed.</summary>
    public float Gap { get; init; } = 2.5f;

    /// <summary>Half-length of each arm when the pointer is pressed.</summary>
    public float PressedSize { get; init; } = 6f;

    /// <summary>Half-length of the gap at the center when pressed.</summary>
    public float PressedGap { get; init; } = 1.5f;

    /// <summary>Shared default instance (white accent, dark shadow).</summary>
    public static CrosshairPointerAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudPointer pointer)
    {
        float size = pointer.IsDown ? PressedSize : Size;
        float gap = pointer.IsDown ? PressedGap : Gap;

        using var shadow = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = ShadowStrokeWidth,
            Color = ShadowColor,
        };
        DrawCrosshair(canvas, size, gap, shadow);

        using var accent = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = StrokeWidth,
            Color = AccentColor,
        };
        DrawCrosshair(canvas, size, gap, accent);
    }

    private static void DrawCrosshair(SKCanvas canvas, float size, float gap, SKPaint paint)
    {
        canvas.DrawLine(-size, 0, -gap, 0, paint);
        canvas.DrawLine(gap, 0, size, 0, paint);
        canvas.DrawLine(0, -size, 0, -gap, paint);
        canvas.DrawLine(0, gap, 0, size, paint);
    }
}
