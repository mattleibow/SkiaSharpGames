using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal enum AsteroidSize
{
    Large,
    Medium,
    Small
}

internal sealed class Asteroid : Entity
{
    public AsteroidSize Size { get; }
    public float RotationSpeed { get; }

    public Asteroid(AsteroidSize size, float x, float y, float vx, float vy, int seed)
    {
        Size = size;
        X = x;
        Y = y;

        float radius = size switch
        {
            AsteroidSize.Large => AsteroidLargeRadius,
            AsteroidSize.Medium => AsteroidMediumRadius,
            _ => AsteroidSmallRadius,
        };

        Collider = new CircleCollider { Radius = radius * 0.8f };
        Rigidbody = new Rigidbody2D { VelocityX = vx, VelocityY = vy };
        Sprite = new AsteroidSprite(radius, seed);
        RotationSpeed = (Random.Shared.NextSingle() - 0.5f) * 3f;
    }

    public int ScoreValue => Size switch
    {
        AsteroidSize.Large => AsteroidLargeScore,
        AsteroidSize.Medium => AsteroidMediumScore,
        _ => AsteroidSmallScore,
    };

    protected override void OnUpdate(float deltaTime)
    {
        Rotation += RotationSpeed * deltaTime;

        // Screen wrap
        float radius = Size switch
        {
            AsteroidSize.Large => AsteroidLargeRadius,
            AsteroidSize.Medium => AsteroidMediumRadius,
            _ => AsteroidSmallRadius,
        };

        if (X < -radius) X = GameWidth + radius;
        else if (X > GameWidth + radius) X = -radius;
        if (Y < -radius) Y = GameHeight + radius;
        else if (Y > GameHeight + radius) Y = -radius;
    }
}
