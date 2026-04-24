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
    };

    public int Damage { get; set; } = 1;

    public PlayerBullet(float x, float y, float vx, float vy, int damage)
    {
        X = x;
        Y = y;
        Damage = damage;
        Collider = new CircleCollider(3f);
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(vx, vy);
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (Y < -10f || Y > GameHeight + 10f || X < -10f || X > GameWidth + 10f)
            Active = false;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        // Glow
        _glowPaint.Color = CyanAccent.WithAlpha(80);
        canvas.DrawCircle(0, 0, 5f, _glowPaint);

        // Core
        canvas.DrawRoundRect(-1.5f, -5f, 3f, 10f, 1.5f, 1.5f, _bulletPaint);
    }
}
