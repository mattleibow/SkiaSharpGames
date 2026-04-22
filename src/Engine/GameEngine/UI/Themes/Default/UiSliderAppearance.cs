using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Appearance for <see cref="UiSlider"/>. Owns visual properties and draw logic.
/// </summary>
public record UiSliderAppearance : UiAppearance<UiSlider>
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private static readonly SKPaint StrokePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

    public SKColor TrackColor { get; init; } = new(0x35, 0x3F, 0x4E);
    public SKColor FillColor { get; init; } = new(0x5A, 0xB5, 0xFF);
    public SKColor KnobColor { get; init; } = SKColors.White;
    public SKColor BorderColor { get; init; } = new(0x17, 0x1D, 0x27);
    public float TrackHeight { get; init; } = 10f;
    public float KnobRadius { get; init; } = 11f;
    public float BorderWidth { get; init; } = 2f;

    public static UiSliderAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiSlider slider)
    {
        byte alpha = (byte)(255 * Math.Clamp(slider.Alpha, 0f, 1f));
        if (alpha == 0) return;

        var rect = slider.LocalRect;
        float value = Math.Clamp(slider.Value, 0f, 1f);

        float cy = rect.MidY;
        float left = rect.Left;
        float right = rect.Right;
        float knobX = left + (right - left) * value;

        StrokePaint.StrokeCap = SKStrokeCap.Round;
        StrokePaint.StrokeWidth = TrackHeight;
        StrokePaint.Color = TrackColor.WithAlpha(alpha);
        canvas.DrawLine(left, cy, right, cy, StrokePaint);

        StrokePaint.Color = FillColor.WithAlpha(alpha);
        canvas.DrawLine(left, cy, knobX, cy, StrokePaint);

        FillPaint.Color = KnobColor.WithAlpha(alpha);
        canvas.DrawCircle(knobX, cy, KnobRadius, FillPaint);

        StrokePaint.StrokeCap = SKStrokeCap.Butt;
        StrokePaint.StrokeWidth = BorderWidth;
        StrokePaint.Color = BorderColor.WithAlpha(alpha);
        canvas.DrawCircle(knobX, cy, KnobRadius, StrokePaint);
    }
}
