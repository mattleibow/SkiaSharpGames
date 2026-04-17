using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Appearance for <see cref="UiJoystick"/>. Owns visual properties and draw logic.
/// Draws at local origin (0,0); the entity transform handles positioning.
/// </summary>
public record UiJoystickAppearance : UiAppearance<UiJoystick>
{
    public SKColor BaseColor { get; init; } = new(0x2A, 0x34, 0x44, 170);
    public SKColor BaseBorderColor { get; init; } = new(0xA0, 0xB6, 0xCC, 200);
    public SKColor KnobColor { get; init; } = new(0xE8, 0xF2, 0xFF, 230);
    public SKColor KnobBorderColor { get; init; } = new(0x10, 0x16, 0x20);
    public float BorderWidth { get; init; } = 2f;

    public static UiJoystickAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiJoystick joystick)
    {
        float baseRadius = joystick.Radius;
        var knobDelta = joystick.Delta;

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };

        fillPaint.Color = BaseColor;
        canvas.DrawCircle(0f, 0f, baseRadius, fillPaint);

        strokePaint.Color = BaseBorderColor;
        strokePaint.StrokeWidth = BorderWidth;
        canvas.DrawCircle(0f, 0f, baseRadius, strokePaint);

        float knobRadius = baseRadius * 0.42f;
        fillPaint.Color = KnobColor;
        canvas.DrawCircle(knobDelta.X, knobDelta.Y, knobRadius, fillPaint);

        strokePaint.Color = KnobBorderColor;
        canvas.DrawCircle(knobDelta.X, knobDelta.Y, knobRadius, strokePaint);
    }
}
