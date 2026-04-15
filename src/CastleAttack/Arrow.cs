using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

internal sealed class Arrow : Entity
{
    public readonly Rigidbody2D Rigidbody = new();
    public readonly CircleCollider Collider = new() { Radius = 2f };
    public bool IsEnemy;
    public int EnemyTargetWall;

    public readonly ArrowSprite Sprite = new();
}
