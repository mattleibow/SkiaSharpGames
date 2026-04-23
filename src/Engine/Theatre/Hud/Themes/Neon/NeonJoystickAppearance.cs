using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Neon/cyberpunk joystick — dark circle with glowing neon ring and knob.
/// </summary>
public record NeonJoystickAppearance : HudAppearance<HudJoystick>
{
    private static readonly SKMaskFilter GlowFilter =
        SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f);

    private static readonly SKMaskFilter KnobGlowFilter =
        SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5f);

    public SKColor BaseColor { get; init; } = new(0x0A, 0x0A, 0x14, 200);
    public SKColor RingColor { get; init; } = new(0x00, 0xFF, 0xFF);
    public SKColor KnobColor { get; init; } = new(0xFF, 0x14, 0x93);
    public float BorderWidth { get; init; } = 1.5f;
    public byte GlowAlpha { get; init; } = 100;

    public static NeonJoystickAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudJoystick joystick)
    {
        byte alpha = (byte)(255 * Math.Clamp(joystick.Alpha, 0f, 1f));
        if (alpha == 0) return;

        float baseRadius = joystick.Radius;
        var knobDelta = joystick.Delta;

        // Dark base fill
        using var fillPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = BaseColor.WithAlpha((byte)(BaseColor.Alpha * alpha / 255))
        };
        canvas.DrawCircle(0f, 0f, baseRadius, fillPaint);

        // Ring glow
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = BorderWidth + 2f,
            Color = RingColor.WithAlpha((byte)(GlowAlpha * alpha / 255)),
            MaskFilter = GlowFilter
        };
        canvas.DrawCircle(0f, 0f, baseRadius, glowPaint);

        // Solid ring
        glowPaint.MaskFilter = null;
        glowPaint.StrokeWidth = BorderWidth;
        glowPaint.Color = RingColor.WithAlpha(alpha);
        canvas.DrawCircle(0f, 0f, baseRadius, glowPaint);

        // Knob
        float knobRadius = baseRadius * 0.42f;

        // Knob glow
        using var knobPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = KnobColor.WithAlpha((byte)(GlowAlpha * alpha / 255)),
            MaskFilter = KnobGlowFilter
        };
        canvas.DrawCircle(knobDelta.X, knobDelta.Y, knobRadius + 3f, knobPaint);

        // Solid knob
        knobPaint.MaskFilter = null;
        knobPaint.Color = KnobColor.WithAlpha(alpha);
        canvas.DrawCircle(knobDelta.X, knobDelta.Y, knobRadius, knobPaint);
    }
}
