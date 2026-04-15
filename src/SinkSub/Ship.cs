using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class Ship : Entity
{
    public readonly RectCollider Collider = new() { Width = ShipWidth, Height = ShipHeight };

    public readonly ShipSprite Sprite = new();

    public void Draw(SKCanvas canvas)
    {
        canvas.Save();
        canvas.Translate(X, Y);
        Sprite.Draw(canvas);
        canvas.Restore();
    }
}
