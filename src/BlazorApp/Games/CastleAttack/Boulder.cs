using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal sealed class Boulder : Entity
{
    public readonly Rigidbody2D Rigidbody = new();
    public readonly CircleCollider Collider = new() { Radius = 7f };
    public int TargetWallIdx;
}
