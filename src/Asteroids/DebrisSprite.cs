using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class DebrisSprite : Sprite
{
    private static readonly SKPaint _paint = new()
    {
        Color = DebrisColor,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.5f,
    };

    public override void Draw(SKCanvas canvas)
    {
        byte alpha = (byte)(255 * Math.Clamp(Alpha, 0f, 1f));
        _paint.Color = DebrisColor.WithAlpha(alpha);
        canvas.DrawLine(-3f, 0f, 3f, 0f, _paint);
    }
}
