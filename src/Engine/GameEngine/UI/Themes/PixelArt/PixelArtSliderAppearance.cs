using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Pixel-art slider appearance — flat rectangular track with a square knob.
/// No anti-aliasing, no rounded corners.
/// </summary>
public record PixelArtSliderAppearance : UiAppearance<UiSlider>
{
    public SKColor TrackColor { get; init; } = new(0x2A, 0x2A, 0x1E);
    public SKColor FillColor { get; init; } = new(0x6B, 0x8E, 0x3A);
    public SKColor KnobColor { get; init; } = new(0xF5, 0xE6, 0xB8);
    public SKColor BorderColor { get; init; } = new(0x1A, 0x1A, 0x0E);
    public float TrackHeight { get; init; } = 10f;
    public float KnobSize { get; init; } = 20f;
    public float BorderWidth { get; init; } = 3f;

    public static PixelArtSliderAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiSlider slider)
    {
        var rect = slider.LocalRect;
        float value = Math.Clamp(slider.Value, 0f, 1f);

        float cy = rect.MidY;
        float left = rect.Left;
        float right = rect.Right;
        float knobX = left + (right - left) * value;

        using var fillPaint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Stroke };

        // Track background
        var trackRect = new SKRect(left, cy - TrackHeight / 2f, right, cy + TrackHeight / 2f);
        fillPaint.Color = TrackColor;
        canvas.DrawRect(trackRect, fillPaint);

        // Filled portion
        if (knobX > left)
        {
            var fillRect = new SKRect(left, cy - TrackHeight / 2f, knobX, cy + TrackHeight / 2f);
            fillPaint.Color = FillColor;
            canvas.DrawRect(fillRect, fillPaint);
        }

        // Track border
        strokePaint.StrokeWidth = BorderWidth;
        strokePaint.Color = BorderColor;
        canvas.DrawRect(trackRect, strokePaint);

        // Square knob
        float halfKnob = KnobSize / 2f;
        var knobRect = new SKRect(knobX - halfKnob, cy - halfKnob, knobX + halfKnob, cy + halfKnob);
        fillPaint.Color = KnobColor;
        canvas.DrawRect(knobRect, fillPaint);

        strokePaint.Color = BorderColor;
        canvas.DrawRect(knobRect, strokePaint);
    }
}
