using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.Default;

/// <summary>
/// Appearance for <see cref="HudSlider"/>. Owns visual properties and draw logic.
/// </summary>
public record DefaultSliderAppearance : HudAppearance<HudSlider>
{
    private static readonly SKPaint FillPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };
    private static readonly SKPaint StrokePaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    public SKColor TrackColor { get; init; } = new(0x35, 0x3F, 0x4E);
    public SKColor FillColor { get; init; } = new(0x5A, 0xB5, 0xFF);
    public SKColor KnobColor { get; init; } = SKColors.White;
    public SKColor BorderColor { get; init; } = new(0x17, 0x1D, 0x27);
    public float TrackHeight { get; init; } = 10f;
    public float KnobRadius { get; init; } = 11f;
    public float BorderWidth { get; init; } = 2f;

    public static DefaultSliderAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudSlider slider)
    {
        var rect = slider.LocalRect;
        float value = Math.Clamp(slider.Value, 0f, 1f);

        float cy = rect.MidY;
        float left = rect.Left;
        float right = rect.Right;
        float knobX = left + (right - left) * value;

        StrokePaint.StrokeCap = SKStrokeCap.Round;
        StrokePaint.StrokeWidth = TrackHeight;
        StrokePaint.Color = TrackColor;
        canvas.DrawLine(left, cy, right, cy, StrokePaint);

        StrokePaint.Color = FillColor;
        canvas.DrawLine(left, cy, knobX, cy, StrokePaint);

        FillPaint.Color = KnobColor;
        canvas.DrawCircle(knobX, cy, KnobRadius, FillPaint);

        StrokePaint.StrokeCap = SKStrokeCap.Butt;
        StrokePaint.StrokeWidth = BorderWidth;
        StrokePaint.Color = BorderColor;
        canvas.DrawCircle(knobX, cy, KnobRadius, StrokePaint);
    }
}
