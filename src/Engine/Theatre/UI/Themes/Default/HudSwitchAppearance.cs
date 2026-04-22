using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Default sliding switch appearance — track with a sliding knob.
/// </summary>
public record HudSwitchAppearance : HudAppearance<HudSwitch>
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private static readonly SKPaint StrokePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

    public SKColor TrackOffColor { get; init; } = new(0x44, 0x4F, 0x5E);
    public SKColor TrackOnColor { get; init; } = new(0x46, 0xA4, 0xF6);
    public SKColor KnobColor { get; init; } = SKColors.White;
    public SKColor BorderColor { get; init; } = new(0x15, 0x1D, 0x27);
    public float CornerRadius { get; init; } = 14f;
    public float BorderWidth { get; init; } = 2f;

    public static HudSwitchAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudSwitch sw)
    {
        byte alpha = (byte)(255 * Math.Clamp(sw.Alpha, 0f, 1f));
        if (alpha == 0) return;

        var rect = sw.LocalRect;

        FillPaint.Color = (sw.IsOn ? TrackOnColor : TrackOffColor).WithAlpha(alpha);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, FillPaint);

        StrokePaint.StrokeWidth = BorderWidth;
        StrokePaint.Color = BorderColor.WithAlpha(alpha);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, StrokePaint);

        float margin = 4f;
        float knobRadius = MathF.Max(6f, rect.Height * 0.5f - margin);
        float knobX = sw.IsOn ? rect.Right - margin - knobRadius : rect.Left + margin + knobRadius;

        FillPaint.Color = KnobColor.WithAlpha(alpha);
        canvas.DrawCircle(knobX, rect.MidY, knobRadius, FillPaint);
        StrokePaint.Color = BorderColor.WithAlpha(alpha);
        StrokePaint.StrokeWidth = MathF.Max(1f, BorderWidth * 0.75f);
        canvas.DrawCircle(knobX, rect.MidY, knobRadius, StrokePaint);
    }
}
