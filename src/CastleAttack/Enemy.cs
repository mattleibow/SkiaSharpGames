using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

internal sealed class Enemy : Entity
{
    public EnemyType Type;
    public float HP, MaxHP;
    public float Speed;
    public EnemyState State = EnemyState.Walking;
    public float AttackTimer;
    public float AttackInterval;
    public float AttackDamage;
    public float AttackRange;
    public int TargetWallIdx = -1;
    // Crossbowman
    public float FireCooldown;
    public float FireInterval = 2.5f;

    public Enemy()
    {
        Collider = new RectCollider();
        Rigidbody = new Rigidbody2D();
        Sprite = new EnemySprite();
    }

    public new EnemySprite Sprite { get => (EnemySprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }

    protected override void OnUpdate(float deltaTime)
    {
        // Sync sprite with current entity state
        Sprite.HP = HP;
        Sprite.MaxHP = MaxHP;
        Sprite.Type = Type;
        Sprite.Collider = Collider;
    }
}
