using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class Mine : Entity
{
    public Mine()
    {
        Collider = new CircleCollider { Radius = MineRadius };
        Rigidbody = new Rigidbody2D();
        Sprite = new MineSprite();
    }

    public new MineSprite Sprite { get => (MineSprite)base.Sprite!; init => base.Sprite = value; }
    public new CircleCollider Collider { get => (CircleCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }
}
