using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class Bullet(bool fromEnemy) : Entity
{
    private static readonly SKPaint _playerBulletPaint = new() { Color = PlayerBulletColor, IsAntialias = true };
    private static readonly SKPaint _enemyBulletPaint = new() { Color = EnemyBulletColor, IsAntialias = true };

    public bool FromEnemy { get; } = fromEnemy;
    public float SpeedY { get; set; }
    public readonly RectCollider Collider = new() { Width = BulletWidth, Height = BulletHeight };

    public void Draw(SKCanvas canvas)
    {
        var paint = FromEnemy ? _enemyBulletPaint : _playerBulletPaint;
        canvas.DrawRect(X - BulletWidth / 2f, Y - BulletHeight / 2f, BulletWidth, BulletHeight, paint);
    }
}
