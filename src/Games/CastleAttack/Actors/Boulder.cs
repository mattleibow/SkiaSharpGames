using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

internal sealed class Boulder : Actor
{
    public int TargetWallIdx;

    private static readonly SKPaint Paint = new() { Color = ColBoulder, IsAntialias = true };

    public Boulder()
    {
        Collider = new CircleCollider { Radius = 7f };
        Rigidbody = new Rigidbody2D();
    }

    public new CircleCollider Collider { get => (CircleCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawCircle(0f, 0f, 7f, Paint);
    }
}
