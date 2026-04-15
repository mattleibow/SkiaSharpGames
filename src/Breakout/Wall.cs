using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// An invisible wall used for boundary collisions. The ball bounces off these
/// using the same <see cref="CollisionResolver.TryGetHit"/> path as bricks and paddle.
/// </summary>
internal sealed class Wall : Entity
{
    public readonly RectCollider Collider;

    public Wall(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Collider = new RectCollider { Width = width, Height = height };
    }
}
