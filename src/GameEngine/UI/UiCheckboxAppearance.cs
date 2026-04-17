using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Appearance for <see cref="UiCheckbox"/>. Owns visual properties and draw logic.
/// </summary>
public record UiCheckboxAppearance : UiAppearance<UiCheckbox>
{
    public SKColor FillColor { get; init; } = new(0x22, 0x2A, 0x35);
    public SKColor BorderColor { get; init; } = new(0x8D, 0xA2, 0xB8);
    public SKColor CheckColor { get; init; } = new(0x61, 0xD0, 0x7D);
    public float CornerRadius { get; init; } = 6f;
    public float BorderWidth { get; init; } = 2f;

    public static UiCheckboxAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiCheckbox checkbox)
    {
        var rect = checkbox.LocalRect;

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };

        fillPaint.Color = FillColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, fillPaint);

        strokePaint.StrokeWidth = BorderWidth;
        strokePaint.Color = BorderColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, strokePaint);

        if (!checkbox.IsChecked)
            return;

        strokePaint.Color = CheckColor;
        strokePaint.StrokeCap = SKStrokeCap.Round;
        strokePaint.StrokeWidth = MathF.Max(2f, rect.Width * 0.12f);
        canvas.DrawLine(rect.Left + rect.Width * 0.2f, rect.MidY, rect.Left + rect.Width * 0.45f, rect.Bottom - rect.Height * 0.2f, strokePaint);
        canvas.DrawLine(rect.Left + rect.Width * 0.45f, rect.Bottom - rect.Height * 0.2f, rect.Right - rect.Width * 0.2f, rect.Top + rect.Height * 0.2f, strokePaint);
    }
}
