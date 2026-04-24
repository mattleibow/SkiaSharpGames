using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.Neon;

/// <summary>
/// Neon/cyberpunk switch — dark track with glowing neon border and knob.
/// </summary>
public record NeonSwitchAppearance : HudAppearance<HudSwitch>
{
    private static readonly SKMaskFilter GlowFilter = SKMaskFilter.CreateBlur(
        SKBlurStyle.Normal,
        3f
    );

    private static readonly SKMaskFilter KnobGlowFilter = SKMaskFilter.CreateBlur(
        SKBlurStyle.Normal,
        5f
    );

    public SKColor TrackColor { get; init; } = new(0x0A, 0x0A, 0x14);
    public SKColor BorderColor { get; init; } = new(0x44, 0x44, 0xFF);
    public SKColor KnobOffColor { get; init; } = new(0x44, 0x44, 0xFF);
    public SKColor KnobOnColor { get; init; } = new(0x00, 0xFF, 0xFF);
    public float CornerRadius { get; init; } = 14f;
    public float BorderWidth { get; init; } = 1.5f;
    public byte GlowAlpha { get; init; } = 100;

    public static NeonSwitchAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudSwitch sw)
    {
        byte alpha = (byte)(255 * Math.Clamp(sw.Alpha, 0f, 1f));
        if (alpha == 0)
            return;

        var rect = sw.LocalRect;
        float cr = CornerRadius;
        var knobColor = sw.IsOn ? KnobOnColor : KnobOffColor;

        // Dark track fill
        using var fillPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = TrackColor.WithAlpha(alpha),
        };
        canvas.DrawRoundRect(rect, cr, cr, fillPaint);

        // Glow border
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = BorderWidth + 2f,
            Color = BorderColor.WithAlpha((byte)(GlowAlpha * alpha / 255)),
            MaskFilter = GlowFilter,
        };
        canvas.DrawRoundRect(rect, cr, cr, glowPaint);

        // Solid border
        glowPaint.MaskFilter = null;
        glowPaint.StrokeWidth = BorderWidth;
        glowPaint.Color = BorderColor.WithAlpha(alpha);
        canvas.DrawRoundRect(rect, cr, cr, glowPaint);

        // Knob
        float margin = 4f;
        float knobRadius = MathF.Max(6f, rect.Height * 0.5f - margin);
        float knobX = sw.IsOn ? rect.Right - margin - knobRadius : rect.Left + margin + knobRadius;
        float knobY = rect.MidY;

        // Knob glow
        using var knobGlowPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = knobColor.WithAlpha((byte)(GlowAlpha * alpha / 255)),
            MaskFilter = KnobGlowFilter,
        };
        canvas.DrawCircle(knobX, knobY, knobRadius + 2f, knobGlowPaint);

        // Solid knob
        knobGlowPaint.MaskFilter = null;
        knobGlowPaint.Color = knobColor.WithAlpha(alpha);
        canvas.DrawCircle(knobX, knobY, knobRadius, knobGlowPaint);
    }
}