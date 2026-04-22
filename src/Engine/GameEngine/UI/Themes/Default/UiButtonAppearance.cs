using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Appearance for <see cref="UiButton"/>. Owns visual properties and draw logic.
/// Use <c>with { }</c> to create customised variants.
/// </summary>
public record UiButtonAppearance : UiAppearance<UiButton>
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private static readonly SKPaint StrokePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private static readonly SKPaint TextPaint = new() { IsAntialias = true };

    public SKColor FillColor { get; init; } = new(0x36, 0x44, 0x56);
    public SKColor PressedFillColor { get; init; } = new(0x22, 0x2D, 0x3D);
    public SKColor TextColor { get; init; } = SKColors.White;
    public SKColor BorderColor { get; init; } = new(0x95, 0xA5, 0xB5);
    public SKColor BevelLightColor { get; init; } = new(0x95, 0xB6, 0xD5);
    public SKColor BevelShadowColor { get; init; } = new(0x10, 0x18, 0x23);
    public float CornerRadius { get; init; } = 10f;
    public float BorderWidth { get; init; } = 2f;
    public float BevelSize { get; init; } = 2f;
    public byte DisabledAlpha { get; init; } = 110;

    public static UiButtonAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiButton button)
    {
        DrawDirect(canvas, button.LocalRect, button.Label,
            button.IsPressed, button.IsEnabled, button.FontSize, button.Alpha);
    }

    /// <summary>
    /// Draws a button directly without requiring a <see cref="UiButton"/> entity.
    /// Used by game screens that render touch-control overlays.
    /// </summary>
    public void DrawDirect(
        SKCanvas canvas,
        SKRect rect,
        string label,
        bool pressed = false,
        bool enabled = true,
        float fontSize = 18f,
        float alpha = 1f)
    {
        byte a = enabled ? (byte)(255 * Math.Clamp(alpha, 0f, 1f)) : DisabledAlpha;
        if (a == 0) return;

        var font = UiLabelAppearance.GetFont(fontSize);

        FillPaint.Color = (pressed ? PressedFillColor : FillColor).WithAlpha(a);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, FillPaint);

        if (BevelSize > 0f)
        {
            StrokePaint.StrokeWidth = BevelSize;
            StrokePaint.Color = (pressed ? BevelShadowColor : BevelLightColor).WithAlpha(a);
            var inset = SKRect.Inflate(rect, -BevelSize * 0.5f, -BevelSize * 0.5f);
            canvas.DrawLine(inset.Left, inset.Top, inset.Right, inset.Top, StrokePaint);
            canvas.DrawLine(inset.Left, inset.Top, inset.Left, inset.Bottom, StrokePaint);

            StrokePaint.Color = (pressed ? BevelLightColor : BevelShadowColor).WithAlpha(a);
            canvas.DrawLine(inset.Left, inset.Bottom, inset.Right, inset.Bottom, StrokePaint);
            canvas.DrawLine(inset.Right, inset.Top, inset.Right, inset.Bottom, StrokePaint);
        }

        StrokePaint.StrokeWidth = BorderWidth;
        StrokePaint.Color = BorderColor.WithAlpha(a);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, StrokePaint);

        TextPaint.Color = TextColor.WithAlpha(a);
        float baselineY = rect.MidY + fontSize * 0.35f;
        canvas.DrawText(label, rect.MidX, baselineY, SKTextAlign.Center, font, TextPaint);
    }
}
