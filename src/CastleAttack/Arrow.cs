using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

internal sealed class Arrow : Entity
{
    public bool IsEnemy;
    public int EnemyTargetWall;

    public Arrow()
    {
        Collider = new CircleCollider { Radius = 2f };
        Rigidbody = new Rigidbody2D();
        Sprite = new ArrowSprite();
    }

    public new ArrowSprite Sprite { get => (ArrowSprite)base.Sprite!; init => base.Sprite = value; }
    public new CircleCollider Collider { get => (CircleCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }

    protected override void OnUpdate(float deltaTime)
    {
        // Sync sprite with current velocity for angle-based drawing
        Sprite.IsEnemy = IsEnemy;
        Sprite.VelocityX = Rigidbody.VelocityX;
        Sprite.VelocityY = Rigidbody.VelocityY;
    }
}
