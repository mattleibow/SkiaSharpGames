using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Appearance for <see cref="UiJoystick"/>. Owns visual properties and draw logic.
/// Draws at local origin (0,0); the entity transform handles positioning.
/// </summary>
public record UiJoystickAppearance : UiAppearance<UiJoystick>
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private static readonly SKPaint StrokePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

    public SKColor BaseColor { get; init; } = new(0x2A, 0x34, 0x44, 170);
    public SKColor BaseBorderColor { get; init; } = new(0xA0, 0xB6, 0xCC, 200);
    public SKColor KnobColor { get; init; } = new(0xE8, 0xF2, 0xFF, 230);
    public SKColor KnobBorderColor { get; init; } = new(0x10, 0x16, 0x20);
    public float BorderWidth { get; init; } = 2f;

    public static UiJoystickAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiJoystick joystick)
    {
        byte alpha = (byte)(255 * Math.Clamp(joystick.Alpha, 0f, 1f));
        if (alpha == 0) return;

        float baseRadius = joystick.Radius;
        var knobDelta = joystick.Delta;

        FillPaint.Color = BaseColor.WithAlpha((byte)(BaseColor.Alpha * alpha / 255));
        canvas.DrawCircle(0f, 0f, baseRadius, FillPaint);

        StrokePaint.Color = BaseBorderColor.WithAlpha((byte)(BaseBorderColor.Alpha * alpha / 255));
        StrokePaint.StrokeWidth = BorderWidth;
        canvas.DrawCircle(0f, 0f, baseRadius, StrokePaint);

        float knobRadius = baseRadius * 0.42f;
        FillPaint.Color = KnobColor.WithAlpha((byte)(KnobColor.Alpha * alpha / 255));
        canvas.DrawCircle(knobDelta.X, knobDelta.Y, knobRadius, FillPaint);

        StrokePaint.Color = KnobBorderColor.WithAlpha(alpha);
        canvas.DrawCircle(knobDelta.X, knobDelta.Y, knobRadius, StrokePaint);
    }
}
