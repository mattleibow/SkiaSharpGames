using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class Bullet : Entity
{
    public float Lifetime { get; set; } = BulletLifetime;

    public Bullet(float x, float y, float vx, float vy)
    {
        X = x;
        Y = y;
        Collider = new CircleCollider { Radius = BulletRadius };
        Rigidbody = new Rigidbody2D { VelocityX = vx, VelocityY = vy };
        Sprite = new BulletSprite();
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
}
