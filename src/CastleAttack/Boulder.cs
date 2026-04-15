using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

internal sealed class Boulder : Entity
{
    public int TargetWallIdx;

    public Boulder()
    {
        Collider = new CircleCollider { Radius = 7f };
        Rigidbody = new Rigidbody2D();
        Sprite = new BoulderSprite();
    }

    public new CircleCollider Collider { get => (CircleCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }
}
