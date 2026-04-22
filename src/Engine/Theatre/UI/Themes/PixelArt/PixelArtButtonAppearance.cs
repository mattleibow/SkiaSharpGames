using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Pixel-art button appearance — flat filled rect with 3D bevel effect using
/// highlight (top-left) and shadow (bottom-right) inset lines.
/// No anti-aliasing, no rounded corners.
/// </summary>
public record PixelArtButtonAppearance : HudAppearance<HudButton>
{
    private static readonly SKPaint FillPaint = new() { IsAntialias = false, Style = SKPaintStyle.Fill };
    private static readonly SKPaint StrokePaint = new() { IsAntialias = false, Style = SKPaintStyle.Stroke };
    private static readonly SKPaint TextPaint = new() { IsAntialias = false };

    public SKColor FillColor { get; init; } = new(0x4A, 0x6B, 0x3A);
    public SKColor PressedFillColor { get; init; } = new(0x35, 0x4F, 0x28);
    public SKColor TextColor { get; init; } = new(0xF5, 0xE6, 0xB8);
    public SKColor BorderColor { get; init; } = new(0x1A, 0x1A, 0x0E);
    public SKColor BevelLightColor { get; init; } = new(0x7A, 0xA8, 0x5C);
    public SKColor BevelShadowColor { get; init; } = new(0x26, 0x3B, 0x1A);
    public float BorderWidth { get; init; } = 3f;
    public float BevelSize { get; init; } = 2f;
    public byte DisabledAlpha { get; init; } = 110;

    public static PixelArtButtonAppearance Default { get; } = new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, HudButton button)
    {
        DrawDirect(canvas, button.LocalRect, button.Label,
            button.IsPressed, button.IsEnabled, button.FontSize, button.Alpha);
    }

    /// <summary>
    /// Draws a button directly without requiring a <see cref="HudButton"/> actor.
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

        var font = DefaultLabelAppearance.GetFont(fontSize);

        // Solid fill
        FillPaint.Color = (pressed ? PressedFillColor : FillColor).WithAlpha(a);
        canvas.DrawRect(rect, FillPaint);

        // 3D bevel — highlight on top-left, shadow on bottom-right (swap when pressed)
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

        // Border
        StrokePaint.StrokeWidth = BorderWidth;
        StrokePaint.Color = BorderColor.WithAlpha(a);
        canvas.DrawRect(rect, StrokePaint);

        // Label text
        TextPaint.Color = TextColor.WithAlpha(a);
        float baselineY = rect.MidY + fontSize * 0.35f;
        canvas.DrawText(label, rect.MidX, baselineY, SKTextAlign.Center, font, TextPaint);
    }
}
