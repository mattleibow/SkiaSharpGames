using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Static utility methods for common SkiaSharp drawing patterns used across games.
/// These helpers reduce boilerplate in <see cref="GameScreenBase"/> subclasses.
/// </summary>
public static class DrawHelper
{
    /// <summary>Fills a rectangle with a solid colour.</summary>
    public static void FillRect(SKCanvas canvas, float x, float y, float width, float height, SKColor color, float alpha = 1f)
    {
        byte a = (byte)(255 * Math.Clamp(alpha, 0f, 1f));
        using var paint = new SKPaint { Color = color.WithAlpha(a) };
        canvas.DrawRect(SKRect.Create(x, y, width, height), paint);
    }

    /// <summary>Draws a semi-transparent black overlay over the full game area.</summary>
    public static void DrawOverlay(SKCanvas canvas, float width, float height, float alpha = 0.73f, SKColor? color = null)
    {
        byte a = (byte)(255 * Math.Clamp(alpha, 0f, 1f));
        var c = color ?? SKColors.Black;
        using var paint = new SKPaint { Color = c.WithAlpha(a) };
        canvas.DrawRect(SKRect.Create(0, 0, width, height), paint);
    }

    /// <summary>
    /// Draws text horizontally centred at the given <paramref name="cx"/> position.
    /// </summary>
    public static void DrawCenteredText(SKCanvas canvas, string text, float size, SKColor color,
        float cx, float y, float alpha = 1f)
    {
        byte a = (byte)(255 * Math.Clamp(alpha, 0f, 1f));
        using var font = new SKFont(SKTypeface.Default, size) { Edging = SKFontEdging.Antialias };
        using var paint = new SKPaint { Color = color.WithAlpha(a), IsAntialias = true };
        float w = font.MeasureText(text);
        canvas.DrawText(text, cx - w / 2f, y, font, paint);
    }

    /// <summary>Draws left-aligned text at an explicit position.</summary>
    public static void DrawText(SKCanvas canvas, string text, float size, SKColor color,
        float x, float y, float alpha = 1f)
    {
        byte a = (byte)(255 * Math.Clamp(alpha, 0f, 1f));
        using var font = new SKFont(SKTypeface.Default, size) { Edging = SKFontEdging.Antialias };
        using var paint = new SKPaint { Color = color.WithAlpha(a), IsAntialias = true };
        canvas.DrawText(text, x, y, font, paint);
    }

    /// <summary>Returns the rendered width of <paramref name="text"/> at the given font size.</summary>
    public static float MeasureText(string text, float size)
    {
        using var font = new SKFont(SKTypeface.Default, size);
        return font.MeasureText(text);
    }
}
