using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class DepthCharge : Entity
{
    public readonly CircleCollider Collider = new() { Radius = ChargeRadius };
    public readonly Rigidbody2D Rigidbody = new();
    public readonly DepthChargeSprite Sprite = new();
}
