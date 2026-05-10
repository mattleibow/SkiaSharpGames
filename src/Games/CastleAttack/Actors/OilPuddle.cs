using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>A burning oil puddle that persists on the ground and damages enemies walking through it.</summary>
internal sealed class OilPuddle : Actor
{
    public float Life = OilPuddleDuration;

    private static readonly SKColor CoreColor = new(0xFF, 0x6B, 0x00);
    private static readonly SKColor GlowColor = new(0xFF, 0x44, 0x00, 80);

    public OilPuddle(float x)
    {
        X = x;
        Y = GroundY - OilPuddleHeight / 2f;
        Collider = new RectCollider { Width = OilPuddleWidth, Height = OilPuddleHeight };
    }

    public new RectCollider Collider
    {
        get => (RectCollider)base.Collider!;
        init => base.Collider = value;
    }

    protected override void OnUpdate(float deltaTime)
    {
        Life -= deltaTime;
        if (Life <= 0f)
            Active = false;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        byte alpha = (byte)(255 * Math.Clamp(Life / OilPuddleDuration, 0f, 1f));
        using var corePaint = new SKPaint
        {
            Color = CoreColor.WithAlpha(alpha),
            IsAntialias = true,
        };
        using var glowPaint = new SKPaint
        {
            Color = GlowColor.WithAlpha((byte)(alpha / 3)),
            IsAntialias = true,
        };

        float hw = OilPuddleWidth / 2f;
        float hh = OilPuddleHeight / 2f;

        // Glow underneath
        canvas.DrawRoundRect(
            -hw - 4f,
            -hh - 2f,
            OilPuddleWidth + 8f,
            OilPuddleHeight + 4f,
            4f,
            2f,
            glowPaint
        );
        // Core puddle
        canvas.DrawRoundRect(-hw, -hh, OilPuddleWidth, OilPuddleHeight, 3f, 2f, corePaint);
    }
}
