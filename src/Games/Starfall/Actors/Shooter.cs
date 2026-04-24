using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Enemy that flies downward and periodically fires bullets toward the player.
/// </summary>
internal sealed class Shooter : EnemyBase
{
    private float _fireTimer;
    private float _playerX;
    private float _playerY;

    /// <summary>Callback to spawn a bullet into the scene.</summary>
    public Action<EnemyBullet>? OnFireBullet { get; set; }

    public Shooter(float x, float y)
        : base(ShooterHP, ShooterScore, ShooterRadius)
    {
        X = x;
        Y = y;
        _fireTimer = ShooterFireRate * 0.5f; // stagger first shot
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(0, ShooterSpeed);
    }

    public void SetPlayerPosition(float px, float py)
    {
        _playerX = px;
        _playerY = py;
    }

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        _fireTimer -= deltaTime;
        if (_fireTimer <= 0f && Y > 40f && Y < GameHeight - 60f)
        {
            _fireTimer = ShooterFireRate;
            Fire();
        }
    }

    private void Fire()
    {
        float dx = _playerX - X;
        float dy = _playerY - Y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist < 1f) return;

        float vx = dx / dist * EnemyBulletSpeed;
        float vy = dy / dist * EnemyBulletSpeed;
        OnFireBullet?.Invoke(new EnemyBullet(X, Y + Radius, vx, vy));
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        var color = FlashColor(ShooterColor);
        using var paint = new SKPaint { Color = color, IsAntialias = true };

        // Inverted triangle with wings
        float r = ShooterRadius;
        using var path = new SKPath();
        path.MoveTo(0, r);
        path.LineTo(-r, -r * 0.5f);
        path.LineTo(-r * 0.4f, -r * 0.3f);
        path.LineTo(0, -r * 0.7f);
        path.LineTo(r * 0.4f, -r * 0.3f);
        path.LineTo(r, -r * 0.5f);
        path.Close();
        canvas.DrawPath(path, paint);

        // Turret core
        paint.Color = SKColors.White.WithAlpha(200);
        canvas.DrawCircle(0, r * 0.3f, 3f, paint);
    }
}
