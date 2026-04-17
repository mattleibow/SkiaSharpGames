using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Default sliding switch appearance — track with a sliding knob.
/// </summary>
public record UiSwitchAppearance : UiAppearance<UiSwitch>
{
    public SKColor TrackOffColor { get; init; } = new(0x44, 0x4F, 0x5E);
    public SKColor TrackOnColor { get; init; } = new(0x46, 0xA4, 0xF6);
    public SKColor KnobColor { get; init; } = SKColors.White;
    public SKColor BorderColor { get; init; } = new(0x15, 0x1D, 0x27);
    public float CornerRadius { get; init; } = 14f;
    public float BorderWidth { get; init; } = 2f;

    public static UiSwitchAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiSwitch sw)
    {
        var rect = sw.LocalRect;

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };

        fillPaint.Color = sw.IsOn ? TrackOnColor : TrackOffColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, fillPaint);

        strokePaint.StrokeWidth = BorderWidth;
        strokePaint.Color = BorderColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, strokePaint);

        float margin = 4f;
        float knobRadius = MathF.Max(6f, rect.Height * 0.5f - margin);
        float knobX = sw.IsOn ? rect.Right - margin - knobRadius : rect.Left + margin + knobRadius;

        fillPaint.Color = KnobColor;
        canvas.DrawCircle(knobX, rect.MidY, knobRadius, fillPaint);
        strokePaint.Color = BorderColor;
        strokePaint.StrokeWidth = MathF.Max(1f, BorderWidth * 0.75f);
        canvas.DrawCircle(knobX, rect.MidY, knobRadius, strokePaint);
    }
}
