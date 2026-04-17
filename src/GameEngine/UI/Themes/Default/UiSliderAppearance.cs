using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Appearance for <see cref="UiSlider"/>. Owns visual properties and draw logic.
/// </summary>
public record UiSliderAppearance : UiAppearance<UiSlider>
{
    public SKColor TrackColor { get; init; } = new(0x35, 0x3F, 0x4E);
    public SKColor FillColor { get; init; } = new(0x5A, 0xB5, 0xFF);
    public SKColor KnobColor { get; init; } = SKColors.White;
    public SKColor BorderColor { get; init; } = new(0x17, 0x1D, 0x27);
    public float TrackHeight { get; init; } = 10f;
    public float KnobRadius { get; init; } = 11f;
    public float BorderWidth { get; init; } = 2f;

    public static UiSliderAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiSlider slider)
    {
        var rect = slider.LocalRect;
        float value = Math.Clamp(slider.Value, 0f, 1f);

        float cy = rect.MidY;
        float left = rect.Left;
        float right = rect.Right;
        float knobX = left + (right - left) * value;

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };

        strokePaint.StrokeCap = SKStrokeCap.Round;
        strokePaint.StrokeWidth = TrackHeight;
        strokePaint.Color = TrackColor;
        canvas.DrawLine(left, cy, right, cy, strokePaint);

        strokePaint.Color = FillColor;
        canvas.DrawLine(left, cy, knobX, cy, strokePaint);

        fillPaint.Color = KnobColor;
        canvas.DrawCircle(knobX, cy, KnobRadius, fillPaint);

        strokePaint.StrokeCap = SKStrokeCap.Butt;
        strokePaint.StrokeWidth = BorderWidth;
        strokePaint.Color = BorderColor;
        canvas.DrawCircle(knobX, cy, KnobRadius, strokePaint);
    }
}
