using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Pixel-art switch appearance — flat rectangular track with a small square knob.
/// No anti-aliasing, no rounded corners.
/// </summary>
public record PixelArtSwitchAppearance : UiAppearance<UiSwitch>
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = false, Style = SKPaintStyle.Fill };
    private static readonly SKPaint StrokePaint = new() { IsAntialias = false, Style = SKPaintStyle.Stroke };

    public SKColor TrackOffColor { get; init; } = new(0x2A, 0x2A, 0x1E);
    public SKColor TrackOnColor { get; init; } = new(0x4A, 0x6B, 0x3A);
    public SKColor KnobColor { get; init; } = new(0xF5, 0xE6, 0xB8);
    public SKColor BorderColor { get; init; } = new(0x1A, 0x1A, 0x0E);
    public float BorderWidth { get; init; } = 3f;

    public static PixelArtSwitchAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiSwitch sw)
    {
        byte alpha = (byte)(255 * Math.Clamp(sw.Alpha, 0f, 1f));
        if (alpha == 0) return;

        var rect = sw.LocalRect;

        // Track
        FillPaint.Color = (sw.IsOn ? TrackOnColor : TrackOffColor).WithAlpha(alpha);
        canvas.DrawRect(rect, FillPaint);

        // Track border
        StrokePaint.StrokeWidth = BorderWidth;
        StrokePaint.Color = BorderColor.WithAlpha(alpha);
        canvas.DrawRect(rect, StrokePaint);

        // Square knob
        float margin = 4f;
        float knobSize = rect.Height - margin * 2f;
        float knobX = sw.IsOn
            ? rect.Right - margin - knobSize
            : rect.Left + margin;
        float knobY = rect.Top + margin;

        var knobRect = SKRect.Create(knobX, knobY, knobSize, knobSize);
        FillPaint.Color = KnobColor.WithAlpha(alpha);
        canvas.DrawRect(knobRect, FillPaint);

        StrokePaint.StrokeWidth = MathF.Max(1f, BorderWidth * 0.75f);
        StrokePaint.Color = BorderColor.WithAlpha(alpha);
        canvas.DrawRect(knobRect, StrokePaint);
    }
}
