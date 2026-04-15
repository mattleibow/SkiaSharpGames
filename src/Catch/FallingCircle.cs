using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class FallingCircle : Entity
{
    public FallingCircle()
    {
        Collider = new CircleCollider { Radius = CircleRadius };
        Rigidbody = new Rigidbody2D();
        Sprite = new CircleSprite();
    }

    public new CircleSprite Sprite { get => (CircleSprite)base.Sprite!; init => base.Sprite = value; }
    public new CircleCollider Collider { get => (CircleCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }
}
