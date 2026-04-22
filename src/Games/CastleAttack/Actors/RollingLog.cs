using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>A flaming log that rolls rightward from the walls, damaging enemies in its path.</summary>
internal sealed class RollingLog : Actor
{
    public float RollAngle;

    private static readonly SKPaint WoodPaint = new() { Color = ColLog, IsAntialias = true };
    private static readonly SKPaint GrainPaint = new() { Color = new SKColor(0x6B, 0x44, 0x1E), StrokeWidth = 1f, IsAntialias = true };
    private static readonly SKPaint EndPaint = new() { Color = new SKColor(0xA0, 0x72, 0x3A), IsAntialias = true };

    public RollingLog(float x)
    {
        X = x;
        Y = GroundY - LogHeight / 2f;
        Collider = new RectCollider { Width = LogWidth, Height = LogHeight };
        Rigidbody = new Rigidbody2D { VelocityX = LogSpeed };
    }

    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }

    protected override void OnUpdate(float deltaTime)
    {
        RollAngle += LogSpeed * deltaTime * 0.1f;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        float hw = LogWidth / 2f;
        float hh = LogHeight / 2f;

        // Main body
        canvas.DrawRoundRect(-hw, -hh, LogWidth, LogHeight, 5f, 5f, WoodPaint);

        // Wood grain
        canvas.DrawLine(-hw + 4f, -hh + 2f, -hw + 4f, hh - 2f, GrainPaint);
        canvas.DrawLine(0f, -hh + 2f, 0f, hh - 2f, GrainPaint);
        canvas.DrawLine(hw - 4f, -hh + 2f, hw - 4f, hh - 2f, GrainPaint);

        // End circles for rolling effect
        canvas.DrawCircle(-hw + 2f, 0f, hh - 1f, EndPaint);
        canvas.DrawCircle(hw - 2f, 0f, hh - 1f, EndPaint);
    }
}
