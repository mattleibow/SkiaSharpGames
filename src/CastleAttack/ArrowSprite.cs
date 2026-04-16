using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws an angled arrow or crossbow bolt line.</summary>
internal sealed class ArrowSprite : Sprite
{
    public bool IsEnemy;
    public float VelocityX;
    public float VelocityY;

    private static readonly SKPaint FriendlyPaint = new() { Color = ColArrow, StrokeWidth = 2f, IsAntialias = true };
    private static readonly SKPaint EnemyPaint = new() { Color = ColFire, StrokeWidth = 2.5f, IsAntialias = true };

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible) return;

        float angle = MathF.Atan2(VelocityY, VelocityX);
        const float len = 14f;
        float dx = MathF.Cos(angle) * len;
        float dy = MathF.Sin(angle) * len;
        var paint = IsEnemy ? EnemyPaint : FriendlyPaint;
        canvas.DrawLine(0f - dx / 2f, 0f - dy / 2f, 0f + dx / 2f, 0f + dy / 2f, paint);
    }
}
