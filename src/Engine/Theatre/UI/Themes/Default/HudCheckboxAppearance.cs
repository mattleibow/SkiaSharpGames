using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Appearance for <see cref="HudCheckbox"/>. Owns visual properties and draw logic.
/// </summary>
public record HudCheckboxAppearance : HudAppearance<HudCheckbox>
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private static readonly SKPaint StrokePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

    public SKColor FillColor { get; init; } = new(0x22, 0x2A, 0x35);
    public SKColor BorderColor { get; init; } = new(0x8D, 0xA2, 0xB8);
    public SKColor CheckColor { get; init; } = new(0x61, 0xD0, 0x7D);
    public float CornerRadius { get; init; } = 6f;
    public float BorderWidth { get; init; } = 2f;

    public static HudCheckboxAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudCheckbox checkbox)
    {
        byte alpha = (byte)(255 * Math.Clamp(checkbox.Alpha, 0f, 1f));
        if (alpha == 0) return;

        var rect = checkbox.LocalRect;

        FillPaint.Color = FillColor.WithAlpha(alpha);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, FillPaint);

        StrokePaint.StrokeWidth = BorderWidth;
        StrokePaint.Color = BorderColor.WithAlpha(alpha);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, StrokePaint);

        if (!checkbox.IsChecked)
            return;

        StrokePaint.Color = CheckColor.WithAlpha(alpha);
        StrokePaint.StrokeCap = SKStrokeCap.Round;
        StrokePaint.StrokeWidth = MathF.Max(2f, rect.Width * 0.12f);
        canvas.DrawLine(rect.Left + rect.Width * 0.2f, rect.MidY, rect.Left + rect.Width * 0.45f, rect.Bottom - rect.Height * 0.2f, StrokePaint);
        canvas.DrawLine(rect.Left + rect.Width * 0.45f, rect.Bottom - rect.Height * 0.2f, rect.Right - rect.Width * 0.2f, rect.Top + rect.Height * 0.2f, StrokePaint);
    }
}
