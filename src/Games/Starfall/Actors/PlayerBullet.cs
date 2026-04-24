using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Bullet fired by the player. Moves upward and deactivates off-screen.
/// </summary>
internal sealed class PlayerBullet : Actor
{
    private static readonly SKPaint _bulletPaint = new()
    {
        Color = PlayerBulletColor,
        IsAntialias = true,
    };
    private static readonly SKPaint _glowPaint = new()
    {
        IsAntialias = true,
        MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f),
        Color = CyanAccent.WithAlpha(80),
    };
    private static readonly SKPaint _trailPaint = new()
    {
        IsAntialias = true,
        StrokeWidth = 2f,
        Style = SKPaintStyle.Stroke,
    };

    public int Damage { get; set; } = 1;
    private float _prevX, _prevY;

    public PlayerBullet(float x, float y, float vx, float vy, int damage)
    {
        X = x;
        Y = y;
        _prevX = x;
        _prevY = y;
        Damage = damage;
        Collider = new CircleCollider(3f);
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(vx, vy);
    }

    protected override void OnUpdate(float deltaTime)
    {
        _prevX = X;
        _prevY = Y;

        // Let rigidbody step first (Actor.Update calls Rigidbody.Step)
        if (Y < -10f || Y > GameHeight + 10f || X < -10f || X > GameWidth + 10f)
            Active = false;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        // Trail (draw line from prev position to current in local space)
        float trailDx = _prevX - X; // in world space, but we're in local space at (0,0)
        float trailDy = _prevY - Y;
        if (MathF.Abs(trailDx) + MathF.Abs(trailDy) > 1f)
        {
            _trailPaint.Color = CyanAccent.WithAlpha(50);
            canvas.DrawLine(0, 0, trailDx, trailDy, _trailPaint);
        }

        // Glow
        canvas.DrawCircle(0, 0, 5f, _glowPaint);

        // Core
        canvas.DrawRoundRect(-1.5f, -5f, 3f, 10f, 1.5f, 1.5f, _bulletPaint);
    }
}
