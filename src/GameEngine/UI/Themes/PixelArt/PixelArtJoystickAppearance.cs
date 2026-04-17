using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Pixel-art joystick appearance — square base outline with a square knob.
/// No anti-aliasing, no rounded corners.
/// </summary>
public record PixelArtJoystickAppearance : UiAppearance<UiJoystick>
{
    public SKColor BaseColor { get; init; } = new(0x1A, 0x1A, 0x0E, 170);
    public SKColor BaseBorderColor { get; init; } = new(0xC8, 0xA8, 0x52, 200);
    public SKColor KnobColor { get; init; } = new(0xF5, 0xE6, 0xB8, 230);
    public SKColor KnobBorderColor { get; init; } = new(0x1A, 0x1A, 0x0E);
    public float BorderWidth { get; init; } = 3f;

    public static PixelArtJoystickAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiJoystick joystick)
    {
        float baseRadius = joystick.Radius;
        var knobDelta = joystick.Delta;

        using var fillPaint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Stroke };

        // Square base
        var baseRect = new SKRect(-baseRadius, -baseRadius, baseRadius, baseRadius);
        fillPaint.Color = BaseColor;
        canvas.DrawRect(baseRect, fillPaint);

        strokePaint.Color = BaseBorderColor;
        strokePaint.StrokeWidth = BorderWidth;
        canvas.DrawRect(baseRect, strokePaint);

        // Square knob
        float knobSize = baseRadius * 0.42f;
        var knobRect = new SKRect(
            knobDelta.X - knobSize, knobDelta.Y - knobSize,
            knobDelta.X + knobSize, knobDelta.Y + knobSize);
        fillPaint.Color = KnobColor;
        canvas.DrawRect(knobRect, fillPaint);

        strokePaint.Color = KnobBorderColor;
        canvas.DrawRect(knobRect, strokePaint);
    }
}
