using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Pong;

internal sealed class PongEdge : Entity
{
    public readonly RectCollider Collider;

    public PongEdge(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Collider = new RectCollider
        {
            Width = width,
            Height = height,
        };
    }
}
