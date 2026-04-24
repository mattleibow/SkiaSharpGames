using SkiaSharp.Theatre;

namespace SkiaSharpGames.Pong;

internal sealed class PongEdge : Actor
{
    public PongEdge(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Collider = new RectCollider { Width = width, Height = height };
    }
}
