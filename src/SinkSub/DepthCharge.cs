using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class DepthCharge : Entity
{
    public DepthCharge()
    {
        Collider = new CircleCollider { Radius = ChargeRadius };
        Rigidbody = new Rigidbody2D();
        Sprite = new DepthChargeSprite();
    }

    public new DepthChargeSprite Sprite { get => (DepthChargeSprite)base.Sprite!; init => base.Sprite = value; }
    public new CircleCollider Collider { get => (CircleCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }
}
