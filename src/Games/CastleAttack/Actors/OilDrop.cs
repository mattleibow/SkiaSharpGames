using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>A boiling oil drop that falls from a wall and creates a puddle on impact.</summary>
internal sealed class OilDrop : Actor
{
    private static readonly SKPaint CorePaint = new() { Color = ColOil, IsAntialias = true };
    private static readonly SKPaint GlowPaint = new()
    {
        Color = new SKColor(0xFF, 0x6B, 0x00, 60),
        IsAntialias = true,
    };

    public OilDrop(float x, float wallTopY)
    {
        X = x;
        Y = wallTopY;
        Collider = new CircleCollider { Radius = OilDropRadius };
        Rigidbody = new Rigidbody2D { VelocityY = OilDropSpeed };
    }

    public new CircleCollider Collider
    {
        get => (CircleCollider)base.Collider!;
        init => base.Collider = value;
    }
    public new Rigidbody2D Rigidbody
    {
        get => (Rigidbody2D)base.Rigidbody!;
        init => base.Rigidbody = value;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawCircle(0f, 0f, OilDropRadius + 3f, GlowPaint);
        canvas.DrawCircle(0f, 0f, OilDropRadius, CorePaint);
    }
}
