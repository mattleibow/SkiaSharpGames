using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Appearance for <see cref="HudJoystick"/>. Owns visual properties and draw logic.
/// Draws at local origin (0,0); the actor transform handles positioning.
/// </summary>
public record DefaultJoystickAppearance : HudAppearance<HudJoystick>
{
    private static readonly SKPaint FillPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };
    private static readonly SKPaint StrokePaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    public SKColor BaseColor { get; init; } = new(0x2A, 0x34, 0x44, 170);
    public SKColor BaseBorderColor { get; init; } = new(0xA0, 0xB6, 0xCC, 200);
    public SKColor KnobColor { get; init; } = new(0xE8, 0xF2, 0xFF, 230);
    public SKColor KnobBorderColor { get; init; } = new(0x10, 0x16, 0x20);
    public float BorderWidth { get; init; } = 2f;

    public static DefaultJoystickAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudJoystick joystick)
    {
        float baseRadius = joystick.Radius;
        var knobDelta = joystick.Delta;

        FillPaint.Color = BaseColor;
        canvas.DrawCircle(0f, 0f, baseRadius, FillPaint);

        StrokePaint.Color = BaseBorderColor;
        StrokePaint.StrokeWidth = BorderWidth;
        canvas.DrawCircle(0f, 0f, baseRadius, StrokePaint);

        float knobRadius = baseRadius * 0.42f;
        FillPaint.Color = KnobColor;
        canvas.DrawCircle(knobDelta.X, knobDelta.Y, knobRadius, FillPaint);

        StrokePaint.Color = KnobBorderColor;
        canvas.DrawCircle(knobDelta.X, knobDelta.Y, knobRadius, StrokePaint);
    }
}
