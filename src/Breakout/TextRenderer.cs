using SkiaSharp;

namespace SkiaSharpGames.Breakout;

/// <summary>Shared text and overlay drawing helpers with cached paints/fonts.</summary>
internal static class TextRenderer
{
    private static readonly SKPaint Paint = new() { IsAntialias = true };
    private static readonly Dictionary<float, SKFont> Fonts = [];

    public static void DrawText(SKCanvas canvas, string text, float size, SKColor color, float x, float y, float alpha = 1f)
    {
        var font = GetFont(size);
        Paint.Color = color.WithAlpha((byte)(255 * Math.Clamp(alpha, 0f, 1f)));
        canvas.DrawText(text, x, y, font, Paint);
    }

    public static void DrawCenteredText(SKCanvas canvas, string text, float size, SKColor color, float cx, float y, float alpha = 1f)
    {
        var font = GetFont(size);
        Paint.Color = color.WithAlpha((byte)(255 * Math.Clamp(alpha, 0f, 1f)));
        float w = font.MeasureText(text);
        canvas.DrawText(text, cx - w / 2f, y, font, Paint);
    }

    public static float MeasureText(string text, float size) => GetFont(size).MeasureText(text);

    public static void DrawOverlay(SKCanvas canvas, float width, float height, float alpha = 0.73f, SKColor? color = null)
    {
        var c = color ?? SKColors.Black;
        Paint.Color = c.WithAlpha((byte)(255 * Math.Clamp(alpha, 0f, 1f)));
        canvas.DrawRect(SKRect.Create(0, 0, width, height), Paint);
    }

    private static SKFont GetFont(float size)
    {
        float key = MathF.Round(size, 2);
        if (!Fonts.TryGetValue(key, out var font))
        {
            font = new SKFont(SKTypeface.Default, key) { Edging = SKFontEdging.Antialias };
            Fonts[key] = font;
        }
        return font;
    }
}
