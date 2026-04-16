using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

/// <summary>
/// A short-lived particle spawned when an asteroid or ship is destroyed.
/// </summary>
internal sealed class Debris : Entity
{
    public float Lifetime { get; set; } = DebrisLifetime;
    private readonly float _initialLifetime = DebrisLifetime;

    public Debris(float x, float y, float vx, float vy)
    {
        X = x;
        Y = y;
        Rigidbody = new Rigidbody2D { VelocityX = vx, VelocityY = vy };
        Sprite = new DebrisSprite();
    }

    protected override void OnUpdate(float deltaTime)
    {
        Lifetime -= deltaTime;
        if (Lifetime <= 0f)
        {
            Active = false;
            return;
        }

        if (Sprite is not null)
            Sprite.Alpha = Lifetime / _initialLifetime;
    }
}
