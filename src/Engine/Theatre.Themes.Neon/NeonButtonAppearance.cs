using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.Neon;

/// <summary>
/// Neon/cyberpunk button appearance — dark fill with glowing neon border and text.
/// </summary>
public record NeonButtonAppearance : HudAppearance<HudButton>
{
    private static readonly SKMaskFilter GlowFilter = SKMaskFilter.CreateBlur(
        SKBlurStyle.Normal,
        4f
    );

    public SKColor FillColor { get; init; } = new(0x0A, 0x0A, 0x14);
    public SKColor PressedFillColor { get; init; } = new(0x12, 0x0A, 0x1E);
    public SKColor BorderColor { get; init; } = new(0x00, 0xFF, 0xFF);
    public SKColor PressedBorderColor { get; init; } = new(0xFF, 0x00, 0xFF);
    public SKColor TextColor { get; init; } = new(0x00, 0xFF, 0xFF);
    public SKColor PressedTextColor { get; init; } = new(0xFF, 0x00, 0xFF);
    public float CornerRadius { get; init; } = 6f;
    public float BorderWidth { get; init; } = 1.5f;
    public byte GlowAlpha { get; init; } = 100;
    public byte DisabledAlpha { get; init; } = 60;

    public static NeonButtonAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudButton button)
    {
        var rect = button.LocalRect;
        bool pressed = button.IsPressed;
        float alpha = button.Alpha;
        byte a = button.IsEnabled ? (byte)(255 * Math.Clamp(alpha, 0f, 1f)) : DisabledAlpha;

        var neon = pressed ? PressedBorderColor : BorderColor;
        var text = pressed ? PressedTextColor : TextColor;
        float cr = CornerRadius;

        using var fillPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = (pressed ? PressedFillColor : FillColor).WithAlpha(a),
        };
        canvas.DrawRoundRect(rect, cr, cr, fillPaint);

        // Glow pass
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = BorderWidth + 2f,
            Color = neon.WithAlpha((byte)(GlowAlpha * a / 255)),
            MaskFilter = GlowFilter,
        };
        canvas.DrawRoundRect(rect, cr, cr, glowPaint);

        // Solid neon stroke
        glowPaint.MaskFilter = null;
        glowPaint.StrokeWidth = BorderWidth;
        glowPaint.Color = neon.WithAlpha(a);
        canvas.DrawRoundRect(rect, cr, cr, glowPaint);

        // Label
        if (!string.IsNullOrEmpty(button.Label))
        {
            var font = FontProvider.Default.GetFont(button.FontSize);
            using var textPaint = new SKPaint { IsAntialias = true, Color = text.WithAlpha(a) };
            float baselineY = rect.MidY + button.FontSize * 0.35f;
            canvas.DrawText(
                button.Label,
                rect.MidX,
                baselineY,
                SKTextAlign.Center,
                font,
                textPaint
            );
        }
    }
}
