using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

internal sealed class Arrow : Actor
{
    public bool IsEnemy;
    public int EnemyTargetWall;

    private static readonly SKPaint FriendlyPaint = new()
    {
        Color = ColArrow,
        StrokeWidth = 2f,
        IsAntialias = true,
    };
    private static readonly SKPaint EnemyPaint = new()
    {
        Color = ColFire,
        StrokeWidth = 2.5f,
        IsAntialias = true,
    };

    public Arrow()
    {
        Collider = new CircleCollider { Radius = 2f };
        Rigidbody = new Rigidbody2D();
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
        float angle = MathF.Atan2(Rigidbody.VelocityY, Rigidbody.VelocityX);
        const float len = 14f;
        float dx = MathF.Cos(angle) * len;
        float dy = MathF.Sin(angle) * len;
        var paint = IsEnemy ? EnemyPaint : FriendlyPaint;
        canvas.DrawLine(-dx / 2f, -dy / 2f, dx / 2f, dy / 2f, paint);
    }
}