using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>A boiling oil drop that falls from a wall and creates a puddle on impact.</summary>
internal sealed class OilDrop : Entity
{
    public OilDrop(float x, float wallTopY)
    {
        X = x;
        Y = wallTopY;
        Collider = new CircleCollider { Radius = OilDropRadius };
        Rigidbody = new Rigidbody2D { VelocityY = OilDropSpeed };
        Sprite = new OilDropSprite();
    }

    public new CircleCollider Collider { get => (CircleCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }
}
