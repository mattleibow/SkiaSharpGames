using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Pixel-art switch appearance — flat rectangular track with a small square knob.
/// No anti-aliasing, no rounded corners.
/// </summary>
public record PixelArtSwitchAppearance : UiAppearance<UiSwitch>
{
    public SKColor TrackOffColor { get; init; } = new(0x2A, 0x2A, 0x1E);
    public SKColor TrackOnColor { get; init; } = new(0x4A, 0x6B, 0x3A);
    public SKColor KnobColor { get; init; } = new(0xF5, 0xE6, 0xB8);
    public SKColor BorderColor { get; init; } = new(0x1A, 0x1A, 0x0E);
    public float BorderWidth { get; init; } = 3f;

    public static PixelArtSwitchAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiSwitch sw)
    {
        var rect = sw.LocalRect;

        using var fillPaint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Stroke };

        // Track
        fillPaint.Color = sw.IsOn ? TrackOnColor : TrackOffColor;
        canvas.DrawRect(rect, fillPaint);

        // Track border
        strokePaint.StrokeWidth = BorderWidth;
        strokePaint.Color = BorderColor;
        canvas.DrawRect(rect, strokePaint);

        // Square knob
        float margin = 4f;
        float knobSize = rect.Height - margin * 2f;
        float knobX = sw.IsOn
            ? rect.Right - margin - knobSize
            : rect.Left + margin;
        float knobY = rect.Top + margin;

        var knobRect = SKRect.Create(knobX, knobY, knobSize, knobSize);
        fillPaint.Color = KnobColor;
        canvas.DrawRect(knobRect, fillPaint);

        strokePaint.StrokeWidth = MathF.Max(1f, BorderWidth * 0.75f);
        strokePaint.Color = BorderColor;
        canvas.DrawRect(knobRect, strokePaint);
    }
}
