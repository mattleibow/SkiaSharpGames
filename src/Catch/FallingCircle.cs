using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class FallingCircle : Entity
{
    public readonly CircleCollider Collider = new() { Radius = CircleRadius };

    public readonly Rigidbody2D Rigidbody = new();

    public readonly CircleSprite Sprite = new();
}
