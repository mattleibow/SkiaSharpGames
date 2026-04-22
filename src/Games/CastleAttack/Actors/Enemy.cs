using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

internal sealed class Enemy : Actor
{
    public EnemyType Type;
    public float HP, MaxHP;
    public float Speed;
    public EnemyState State = EnemyState.Walking;
    public float AttackTimer;
    public float AttackInterval;
    public float AttackDamage;
    public float AttackRange;
    public int TargetWallIdx = -1;
    // Crossbowman
    public float FireCooldown;
    public float FireInterval = 2.5f;

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

    private readonly HudLabel _mooText = new() { Text = "MOO", FontSize = 10f, Color = new SKColor(0x55, 0x44, 0x22), Align = TextAlign.Center };

    public Enemy()
    {
        Collider = new RectCollider();
        Rigidbody = new Rigidbody2D();
    }

    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }

    protected override void OnDraw(SKCanvas canvas)
    {
        SKColor col = CastleAttackConstants.EnemyCol(Type);
        switch (Type)
        {
            case EnemyType.Catapult: DrawCatapult(canvas, col); break;
            case EnemyType.Ram: DrawRam(canvas, col); break;
            case EnemyType.Cow: DrawCow(canvas, col); break;
            default: DrawHumanoid(canvas, col); break;
        }

        if (MaxHP > 1f)
        {
            float barW = Collider.Width + 4f;
            float ratio = HP / MaxHP;
            float barX = -barW / 2f;
            float barY = -Collider.Height / 2f - 10f;
            canvas.DrawRect(SKRect.Create(barX, barY, barW, 4f), HpBarBg);
            HpBarFg.Color = new SKColor(0xFF, 0x44, 0x00);
            canvas.DrawRect(SKRect.Create(barX, barY, barW * ratio, 4f), HpBarFg);
        }
    }

    private void DrawHumanoid(SKCanvas canvas, SKColor col)
    {
        float hw = Collider.Width / 2f;
        float hh = Collider.Height / 2f;

        BodyPaint.Color = col;
        canvas.DrawRect(SKRect.Create(-hw, -hh, Collider.Width, Collider.Height - 8f), BodyPaint);
        canvas.DrawCircle(0f, -hh - 5f, 6f, BodyPaint);

        if (Type == EnemyType.Crossbowman)
        {
            canvas.DrawLine(-hw - 4f, -hh + Collider.Height / 3f, -hw - 12f, -hh + Collider.Height / 3f, CrossbowPaint);
        }
        else if (Type is EnemyType.Spearman or EnemyType.Berserker)
        {
            canvas.DrawLine(-hw - 2f, -hh, -hw - 14f, -hh - 10f, SpearPaint);
        }
        else
        {
            canvas.DrawLine(-hw - 2f, -hh + 2f, -hw - 14f, -hh + 8f, SwordPaint);
        }
    }

    private void DrawCatapult(SKCanvas canvas, SKColor col)
    {
        float hw = Collider.Width / 2f;
        float hh = Collider.Height / 2f;

        BodyPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(-hw, -hh + 8f, Collider.Width, Collider.Height - 8f), 3f, 3f, BodyPaint);
        canvas.DrawLine(0f, -hh + 14f, -14f, -hh, ArmPaint);
        canvas.DrawCircle(-hw + 6f, hh - 5f, 5f, WheelPaint);
        canvas.DrawCircle(hw - 6f, hh - 5f, 5f, WheelPaint);
    }

    private void DrawRam(SKCanvas canvas, SKColor col)
    {
        float hw = Collider.Width / 2f;
        float hh = Collider.Height / 2f;

        BodyPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(-hw, -hh, Collider.Width, Collider.Height - 6f), 4f, 4f, BodyPaint);
        canvas.DrawRect(SKRect.Create(-hw - 10f, -hh + 6f, 14f, 8f), TipPaint);
        canvas.DrawCircle(-hw + 6f, hh - 4f, 5f, WheelPaint);
        canvas.DrawCircle(hw - 6f, hh - 4f, 5f, WheelPaint);
    }

    private void DrawCow(SKCanvas canvas, SKColor col)
    {
        float hw = Collider.Width / 2f;
        float hh = Collider.Height / 2f;

        BodyPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(-hw, -hh + 4f, Collider.Width, Collider.Height - 8f), 5f, 5f, BodyPaint);
        HeadPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(-hw - 8f, -hh + 6f, 14f, 10f), 4f, 4f, HeadPaint);
        canvas.DrawLine(-hw + 4f, hh - 4f, -hw + 4f, hh, LegPaint);
        canvas.DrawLine(hw - 4f, hh - 4f, hw - 4f, hh, LegPaint);
        canvas.DrawCircle(0f, -hh + 8f, 3f, SpotPaint);
        canvas.Save(); canvas.Translate(0f, -hh); _mooText.Draw(canvas); canvas.Restore();
    }
}
