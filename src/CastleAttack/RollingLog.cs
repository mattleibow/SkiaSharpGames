using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>A flaming log that rolls rightward from the walls, damaging enemies in its path.</summary>
internal sealed class RollingLog : Entity
{
    public RollingLog(float x)
    {
        X = x;
        Y = GroundY - LogHeight / 2f;
        Collider = new RectCollider { Width = LogWidth, Height = LogHeight };
        Rigidbody = new Rigidbody2D { VelocityX = LogSpeed };
        Sprite = new RollingLogSprite();
    }

    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }
}
