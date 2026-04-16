using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class BulletSprite : Sprite
{
    private static readonly SKPaint _playerBulletPaint = new() { Color = PlayerBulletColor, IsAntialias = true };
    private static readonly SKPaint _enemyBulletPaint = new() { Color = EnemyBulletColor, IsAntialias = true };

    public bool FromEnemy { get; set; }

    public override void Draw(SKCanvas canvas)
    {
        var paint = FromEnemy ? _enemyBulletPaint : _playerBulletPaint;
        canvas.DrawRect(-BulletWidth / 2f, -BulletHeight / 2f, BulletWidth, BulletHeight, paint);
    }
}
