using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

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

    /// <summary>
    /// Hitbox. Width and Height are also used for drawing — they define the visual size of the enemy.
    /// </summary>
    public RectCollider Collider = new();
    public readonly Rigidbody2D Rigidbody = new();
}
