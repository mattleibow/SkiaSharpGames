using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.PixelArt;

/// <summary>
/// Pixel-art slider appearance — flat rectangular track with a square knob.
/// No anti-aliasing, no rounded corners.
/// </summary>
public record PixelArtSliderAppearance : HudAppearance<HudSlider>
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = false, Style = SKPaintStyle.Fill };
    private static readonly SKPaint StrokePaint = new() { IsAntialias = false, Style = SKPaintStyle.Stroke };

    public SKColor TrackColor { get; init; } = new(0x2A, 0x2A, 0x1E);
    public SKColor FillColor { get; init; } = new(0x6B, 0x8E, 0x3A);
    public SKColor KnobColor { get; init; } = new(0xF5, 0xE6, 0xB8);
    public SKColor BorderColor { get; init; } = new(0x1A, 0x1A, 0x0E);
    public float TrackHeight { get; init; } = 10f;
    public float KnobSize { get; init; } = 20f;
    public float BorderWidth { get; init; } = 3f;

    public static PixelArtSliderAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudSlider slider)
    {
        byte alpha = (byte)(255 * Math.Clamp(slider.Alpha, 0f, 1f));
        if (alpha == 0) return;

        var rect = slider.LocalRect;
        float value = Math.Clamp(slider.Value, 0f, 1f);

        float cy = rect.MidY;
        float left = rect.Left;
        float right = rect.Right;
        float knobX = left + (right - left) * value;

        // Track background
        var trackRect = new SKRect(left, cy - TrackHeight / 2f, right, cy + TrackHeight / 2f);
        FillPaint.Color = TrackColor.WithAlpha(alpha);
        canvas.DrawRect(trackRect, FillPaint);

        // Filled portion
        if (knobX > left)
        {
            var fillRect = new SKRect(left, cy - TrackHeight / 2f, knobX, cy + TrackHeight / 2f);
            FillPaint.Color = FillColor.WithAlpha(alpha);
            canvas.DrawRect(fillRect, FillPaint);
        }

        // Track border
        StrokePaint.StrokeWidth = BorderWidth;
        StrokePaint.Color = BorderColor.WithAlpha(alpha);
        canvas.DrawRect(trackRect, StrokePaint);

        // Square knob
        float halfKnob = KnobSize / 2f;
        var knobRect = new SKRect(knobX - halfKnob, cy - halfKnob, knobX + halfKnob, cy + halfKnob);
        FillPaint.Color = KnobColor.WithAlpha(alpha);
        canvas.DrawRect(knobRect, FillPaint);

        StrokePaint.Color = BorderColor.WithAlpha(alpha);
        canvas.DrawRect(knobRect, StrokePaint);
    }
}
