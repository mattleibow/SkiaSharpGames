using SkiaSharp;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

/// <summary>
/// Stage-local text and overlay drawing helpers with cached paints and fonts.
/// </summary>
internal static class TextRenderer
{
    private static readonly SKPaint _textPaint = new() { IsAntialias = true };
    private static readonly SKPaint _overlayPaint = new()
    {
        Color = SKColors.Black.WithAlpha((byte)(255 * 0.8f)),
        IsAntialias = true,
    };
    private static readonly Dictionary<float, SKFont> _fontCache = [];

    public static void DrawOverlay(SKCanvas canvas)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);
    }

    public static void DrawCenteredText(
        SKCanvas canvas,
        string text,
        float size,
        SKColor color,
        float y
    )
    {
        var font = GetFont(size);
        _textPaint.Color = color;
        float width = font.MeasureText(text);
        canvas.DrawText(text, (GameWidth - width) / 2f, y, font, _textPaint);
    }

    public static void DrawText(
        SKCanvas canvas,
        string text,
        float size,
        SKColor color,
        float x,
        float y
    )
    {
        var font = GetFont(size);
        _textPaint.Color = color;
        canvas.DrawText(text, x, y, font, _textPaint);
    }

    public static float MeasureText(string text, float size)
    {
        return GetFont(size).MeasureText(text);
    }

    private static SKFont GetFont(float size)
    {
        float key = MathF.Round(size, 2);
        if (!_fontCache.TryGetValue(key, out var font))
        {
            font = new SKFont(SKTypeface.Default, key) { Edging = SKFontEdging.Antialias };
            _fontCache[key] = font;
        }
        return font;
    }
}
