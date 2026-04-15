using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class Ship : Entity
{
    public Ship()
    {
        Collider = new RectCollider { Width = ShipWidth, Height = ShipHeight };
        Sprite = new ShipSprite();
    }

    public new ShipSprite Sprite { get => (ShipSprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }
}
