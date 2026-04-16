using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class BulletSprite : Sprite
{
    private static readonly SKPaint _paint = new()
    {
        Color = BulletColor,
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };

    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawCircle(0, 0, BulletRadius, _paint);
    }
}
