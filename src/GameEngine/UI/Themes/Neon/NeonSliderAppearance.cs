using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Neon/cyberpunk slider — dark track with neon fill glow and glowing knob.
/// </summary>
public record NeonSliderAppearance : UiAppearance<UiSlider>
{
    private static readonly SKMaskFilter GlowFilter =
        SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f);

    private static readonly SKMaskFilter KnobGlowFilter =
        SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5f);

    public SKColor TrackColor { get; init; } = new(0x0A, 0x0A, 0x14);
    public SKColor FillColor { get; init; } = new(0xFF, 0x00, 0xFF);
    public SKColor KnobColor { get; init; } = new(0xFF, 0x00, 0xFF);
    public SKColor TrackBorderColor { get; init; } = new(0x44, 0x44, 0xFF);
    public float TrackHeight { get; init; } = 6f;
    public float KnobRadius { get; init; } = 10f;
    public byte GlowAlpha { get; init; } = 100;

    public static NeonSliderAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiSlider slider)
    {
        var rect = slider.LocalRect;
        float value = Math.Clamp(slider.Value, 0f, 1f);
        float cy = rect.MidY;
        float left = rect.Left;
        float right = rect.Right;
        float knobX = left + (right - left) * value;

        // Dark track
        using var trackPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = TrackHeight,
            Color = TrackColor
        };
        canvas.DrawLine(left, cy, right, cy, trackPaint);

        // Track border glow
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = TrackHeight + 2f,
            Color = TrackBorderColor.WithAlpha(GlowAlpha),
            MaskFilter = GlowFilter
        };
        canvas.DrawLine(left, cy, right, cy, glowPaint);

        // Neon fill glow
        if (value > 0.01f)
        {
            glowPaint.Color = FillColor.WithAlpha(GlowAlpha);
            glowPaint.StrokeWidth = TrackHeight + 2f;
            glowPaint.MaskFilter = GlowFilter;
            canvas.DrawLine(left, cy, knobX, cy, glowPaint);

            // Solid fill
            glowPaint.MaskFilter = null;
            glowPaint.StrokeWidth = TrackHeight;
            glowPaint.Color = FillColor;
            canvas.DrawLine(left, cy, knobX, cy, glowPaint);
        }

        // Knob glow
        using var knobPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = KnobColor.WithAlpha(GlowAlpha),
            MaskFilter = KnobGlowFilter
        };
        canvas.DrawCircle(knobX, cy, KnobRadius + 3f, knobPaint);

        // Solid knob
        knobPaint.MaskFilter = null;
        knobPaint.Color = KnobColor;
        canvas.DrawCircle(knobX, cy, KnobRadius, knobPaint);
    }
}
