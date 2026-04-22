using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// An invisible wall used for boundary collisions.
/// </summary>
internal sealed class Wall : Actor
{
    public Wall(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Collider = new RectCollider { Width = width, Height = height };
    }
}
