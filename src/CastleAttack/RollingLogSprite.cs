using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws a rolling log as a brown rounded rectangle with wood grain lines.</summary>
internal sealed class RollingLogSprite : Sprite
{
    public float RollAngle;

    private static readonly SKPaint WoodPaint = new() { Color = ColLog, IsAntialias = true };
    private static readonly SKPaint GrainPaint = new() { Color = new SKColor(0x6B, 0x44, 0x1E), StrokeWidth = 1f, IsAntialias = true };
    private static readonly SKPaint EndPaint = new() { Color = new SKColor(0xA0, 0x72, 0x3A), IsAntialias = true };

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible) return;

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

    public override void Update(float deltaTime)
    {
        RollAngle += LogSpeed * deltaTime * 0.1f;
    }
}
