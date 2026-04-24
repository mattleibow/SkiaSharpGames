using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.Neon;

/// <summary>
/// Neon/cyberpunk checkbox — dark square with glowing neon border and check mark.
/// </summary>
public record NeonCheckboxAppearance : HudAppearance<HudCheckbox>
{
    private static readonly SKMaskFilter GlowFilter = SKMaskFilter.CreateBlur(
        SKBlurStyle.Normal,
        3f
    );

    public SKColor FillColor { get; init; } = new(0x08, 0x08, 0x12);
    public SKColor BorderColor { get; init; } = new(0x00, 0xFF, 0xFF);
    public SKColor CheckColor { get; init; } = new(0x39, 0xFF, 0x14);
    public float CornerRadius { get; init; } = 4f;
    public float BorderWidth { get; init; } = 1.5f;
    public byte GlowAlpha { get; init; } = 100;

    public static NeonCheckboxAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudCheckbox checkbox)
    {
        var rect = checkbox.LocalRect;
        float cr = CornerRadius;

        using var fillPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = FillColor,
        };
        canvas.DrawRoundRect(rect, cr, cr, fillPaint);

        // Glow border
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = BorderWidth + 2f,
            Color = BorderColor.WithAlpha(GlowAlpha),
            MaskFilter = GlowFilter,
        };
        canvas.DrawRoundRect(rect, cr, cr, glowPaint);

        // Solid border
        glowPaint.MaskFilter = null;
        glowPaint.StrokeWidth = BorderWidth;
        glowPaint.Color = BorderColor;
        canvas.DrawRoundRect(rect, cr, cr, glowPaint);

        if (!checkbox.IsChecked)
            return;

        // Glowing check mark
        using var checkPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = MathF.Max(2f, rect.Width * 0.12f) + 2f,
            Color = CheckColor.WithAlpha(GlowAlpha),
            MaskFilter = GlowFilter,
        };

        float x1 = rect.Left + rect.Width * 0.2f;
        float y1 = rect.MidY;
        float x2 = rect.Left + rect.Width * 0.45f;
        float y2 = rect.Bottom - rect.Height * 0.2f;
        float x3 = rect.Right - rect.Width * 0.2f;
        float y3 = rect.Top + rect.Height * 0.2f;

        canvas.DrawLine(x1, y1, x2, y2, checkPaint);
        canvas.DrawLine(x2, y2, x3, y3, checkPaint);

        // Solid check on top
        checkPaint.MaskFilter = null;
        checkPaint.StrokeWidth = MathF.Max(2f, rect.Width * 0.12f);
        checkPaint.Color = CheckColor;
        canvas.DrawLine(x1, y1, x2, y2, checkPaint);
        canvas.DrawLine(x2, y2, x3, y3, checkPaint);
    }
}
