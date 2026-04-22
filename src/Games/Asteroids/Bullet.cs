using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class Bullet : Actor
{
    private static readonly SKPaint _paint = new()
    {
        Color = BulletColor,
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };

    public float Lifetime { get; set; } = BulletLifetime;

    public Bullet(float x, float y, float vx, float vy)
    {
        X = x;
        Y = y;
        Collider = new CircleCollider { Radius = BulletRadius };
        Rigidbody = new Rigidbody2D { VelocityX = vx, VelocityY = vy };
    }

    protected override void OnUpdate(float deltaTime)
    {
        Lifetime -= deltaTime;
        if (Lifetime <= 0f)
        {
            Active = false;
            return;
        }

        // Screen wrap
        if (X < 0) X += GameWidth;
        else if (X > GameWidth) X -= GameWidth;
        if (Y < 0) Y += GameHeight;
        else if (Y > GameHeight) Y -= GameHeight;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawCircle(0, 0, BulletRadius, _paint);
    }
}
