using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class Bullet : Actor
{
    private static readonly SKPaint _playerBulletPaint = new()
    {
        Color = PlayerBulletColor,
        IsAntialias = true,
    };
    private static readonly SKPaint _enemyBulletPaint = new()
    {
        Color = EnemyBulletColor,
        IsAntialias = true,
    };

    public bool FromEnemy { get; }

    public Bullet(bool fromEnemy, float speedY)
    {
        FromEnemy = fromEnemy;
        Collider = new RectCollider { Width = BulletWidth, Height = BulletHeight };
        Rigidbody = new Rigidbody2D { VelocityY = speedY };
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        var paint = FromEnemy ? _enemyBulletPaint : _playerBulletPaint;
        canvas.DrawRect(-BulletWidth / 2f, -BulletHeight / 2f, BulletWidth, BulletHeight, paint);
    }
}