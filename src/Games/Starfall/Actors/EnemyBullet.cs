using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Bullet fired by enemies toward the player.
/// </summary>
internal sealed class EnemyBullet : Actor
{
    private static readonly SKPaint _paint = new()
    {
        Color = EnemyBulletColor,
        IsAntialias = true,
    };
    private static readonly SKPaint _glowPaint = new()
    {
        IsAntialias = true,
        MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2f),
    };

    public EnemyBullet(float x, float y, float vx, float vy)
    {
        X = x;
        Y = y;
        Collider = new CircleCollider(EnemyBulletRadius);
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(vx, vy);
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (Y < -20f || Y > GameHeight + 20f || X < -20f || X > GameWidth + 20f)
            Active = false;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        _glowPaint.Color = EnemyBulletColor.WithAlpha(60);
        canvas.DrawCircle(0, 0, EnemyBulletRadius + 3f, _glowPaint);
        canvas.DrawCircle(0, 0, EnemyBulletRadius, _paint);
    }
}
