using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Pixel-art joystick appearance — square base outline with a square knob.
/// No anti-aliasing, no rounded corners.
/// </summary>
public record PixelArtJoystickAppearance : UiAppearance<UiJoystick>
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = false, Style = SKPaintStyle.Fill };
    private static readonly SKPaint StrokePaint = new() { IsAntialias = false, Style = SKPaintStyle.Stroke };

    public SKColor BaseColor { get; init; } = new(0x1A, 0x1A, 0x0E, 170);
    public SKColor BaseBorderColor { get; init; } = new(0xC8, 0xA8, 0x52, 200);
    public SKColor KnobColor { get; init; } = new(0xF5, 0xE6, 0xB8, 230);
    public SKColor KnobBorderColor { get; init; } = new(0x1A, 0x1A, 0x0E);
    public float BorderWidth { get; init; } = 3f;

    public static PixelArtJoystickAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiJoystick joystick)
    {
        byte alpha = (byte)(255 * Math.Clamp(joystick.Alpha, 0f, 1f));
        if (alpha == 0) return;

        float baseRadius = joystick.Radius;
        var knobDelta = joystick.Delta;

        // Square base
        var baseRect = new SKRect(-baseRadius, -baseRadius, baseRadius, baseRadius);
        FillPaint.Color = BaseColor.WithAlpha((byte)(BaseColor.Alpha * alpha / 255));
        canvas.DrawRect(baseRect, FillPaint);

        StrokePaint.Color = BaseBorderColor.WithAlpha((byte)(BaseBorderColor.Alpha * alpha / 255));
        StrokePaint.StrokeWidth = BorderWidth;
        canvas.DrawRect(baseRect, StrokePaint);

        // Square knob
        float knobSize = baseRadius * 0.42f;
        var knobRect = new SKRect(
            knobDelta.X - knobSize, knobDelta.Y - knobSize,
            knobDelta.X + knobSize, knobDelta.Y + knobSize);
        FillPaint.Color = KnobColor.WithAlpha((byte)(KnobColor.Alpha * alpha / 255));
        canvas.DrawRect(knobRect, FillPaint);

        StrokePaint.Color = KnobBorderColor.WithAlpha(alpha);
        canvas.DrawRect(knobRect, StrokePaint);
    }
}
