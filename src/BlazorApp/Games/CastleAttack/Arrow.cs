using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal sealed class Arrow : Entity
{
    public readonly Rigidbody2D Rigidbody = new();
    public readonly CircleCollider Collider = new() { Radius = 2f };
    public bool IsEnemy;
    public int EnemyTargetWall;
}
