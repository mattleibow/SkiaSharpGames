using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class Mine : Entity
{
    public readonly CircleCollider Collider = new() { Radius = MineRadius };
    public readonly Rigidbody2D Rigidbody = new();
    public readonly MineSprite Sprite = new();
}
