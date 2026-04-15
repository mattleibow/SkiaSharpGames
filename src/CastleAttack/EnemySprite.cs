using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws all enemy types (humanoid, catapult, ram, cow) and their HP bar.</summary>
internal sealed class EnemySprite : Sprite
{
    public EnemyType Type;
    public RectCollider Collider = new();
    public float HP;
    public float MaxHP;

    // ── Cached paints ─────────────────────────────────────────────────
    private static readonly SKPaint BodyPaint = new() { IsAntialias = true };
    private static readonly SKPaint HeadPaint = new() { IsAntialias = true };
    private static readonly SKPaint WeaponPaint = new() { IsAntialias = true, StrokeWidth = 2f };
    private static readonly SKPaint CrossbowPaint = new() { Color = ColArrow, StrokeWidth = 1.5f, IsAntialias = true };
    private static readonly SKPaint SwordPaint = new() { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 3f, IsAntialias = true };
    private static readonly SKPaint SpearPaint = new() { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 2f, IsAntialias = true };
    private static readonly SKPaint ArmPaint = new() { Color = new SKColor(0x8B, 0x5E, 0x2E), StrokeWidth = 3f, IsAntialias = true };
    private static readonly SKPaint WheelPaint = new() { Color = new SKColor(0x55, 0x44, 0x22), IsAntialias = true };
    private static readonly SKPaint TipPaint = new() { Color = new SKColor(0xCC, 0xCC, 0xCC), IsAntialias = true };
    private static readonly SKPaint LegPaint = new() { Color = new SKColor(0xDD, 0xDD, 0xCC), StrokeWidth = 3f, IsAntialias = true };
    private static readonly SKPaint SpotPaint = new() { Color = new SKColor(0x88, 0x88, 0x77), IsAntialias = true };
    private static readonly SKPaint HpBarBg = new() { Color = new SKColor(0x40, 0x00, 0x00) };
    private static readonly SKPaint HpBarFg = new() { Color = new SKColor(0xFF, 0x44, 0x00) };

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible) return;

        SKColor col = CastleAttackConstants.EnemyCol(Type);
        switch (Type)
        {
            case EnemyType.Catapult: DrawCatapult(canvas, x, y, col); break;
            case EnemyType.Ram: DrawRam(canvas, x, y, col); break;
            case EnemyType.Cow: DrawCow(canvas, x, y, col); break;
            default: DrawHumanoid(canvas, x, y, col); break;
        }

        if (MaxHP > 1f)
        {
            float ex = x - Collider.Width / 2f;
            float ey = GroundY - Collider.Height;
            float barW = Collider.Width + 4f;
            float ratio = HP / MaxHP;
            canvas.DrawRect(SKRect.Create(ex - 2f, ey - 8f, barW, 4f), HpBarBg);
            HpBarFg.Color = new SKColor(0xFF, 0x44, 0x00);
            canvas.DrawRect(SKRect.Create(ex - 2f, ey - 8f, barW * ratio, 4f), HpBarFg);
        }
    }

    private void DrawHumanoid(SKCanvas canvas, float cx, float cy, SKColor col)
    {
        float ex = cx - Collider.Width / 2f;
        float ey = GroundY - Collider.Height;

        BodyPaint.Color = col;
        canvas.DrawRect(SKRect.Create(ex, ey, Collider.Width, Collider.Height - 8f), BodyPaint);
        canvas.DrawCircle(cx, ey - 5f, 6f, BodyPaint);

        if (Type == EnemyType.Crossbowman)
        {
            canvas.DrawLine(ex - 4f, ey + Collider.Height / 3f, ex - 12f, ey + Collider.Height / 3f, CrossbowPaint);
        }
        else if (Type is EnemyType.Spearman or EnemyType.Berserker)
        {
            canvas.DrawLine(ex - 2f, ey, ex - 14f, ey - 10f, SpearPaint);
        }
        else
        {
            canvas.DrawLine(ex - 2f, ey + 2f, ex - 14f, ey + 8f, SwordPaint);
        }
    }

    private void DrawCatapult(SKCanvas canvas, float cx, float cy, SKColor col)
    {
        float ex = cx - Collider.Width / 2f;
        float ey = GroundY - Collider.Height;

        BodyPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(ex, ey + 8f, Collider.Width, Collider.Height - 8f), 3f, 3f, BodyPaint);
        canvas.DrawLine(cx, ey + 14f, cx - 14f, ey, ArmPaint);
        canvas.DrawCircle(ex + 6f, GroundY - 5f, 5f, WheelPaint);
        canvas.DrawCircle(ex + Collider.Width - 6f, GroundY - 5f, 5f, WheelPaint);
    }

    private void DrawRam(SKCanvas canvas, float cx, float cy, SKColor col)
    {
        float ex = cx - Collider.Width / 2f;
        float ey = GroundY - Collider.Height;

        BodyPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(ex, ey, Collider.Width, Collider.Height - 6f), 4f, 4f, BodyPaint);
        canvas.DrawRect(SKRect.Create(ex - 10f, ey + 6f, 14f, 8f), TipPaint);
        canvas.DrawCircle(ex + 6f, GroundY - 4f, 5f, WheelPaint);
        canvas.DrawCircle(ex + Collider.Width - 6f, GroundY - 4f, 5f, WheelPaint);
    }

    private void DrawCow(SKCanvas canvas, float cx, float cy, SKColor col)
    {
        float ex = cx - Collider.Width / 2f;
        float ey = GroundY - Collider.Height;

        BodyPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(ex, ey + 4f, Collider.Width, Collider.Height - 8f), 5f, 5f, BodyPaint);
        HeadPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(ex - 8f, ey + 6f, 14f, 10f), 4f, 4f, HeadPaint);
        canvas.DrawLine(ex + 4f, ey + Collider.Height - 4f, ex + 4f, GroundY, LegPaint);
        canvas.DrawLine(ex + Collider.Width - 4f, ey + Collider.Height - 4f, ex + Collider.Width - 4f, GroundY, LegPaint);
        canvas.DrawCircle(cx, ey + 8f, 3f, SpotPaint);
        TextRenderer.DrawCenteredText(canvas, "MOO", 10f, new SKColor(0x55, 0x44, 0x22), cx, ey);
    }
}
