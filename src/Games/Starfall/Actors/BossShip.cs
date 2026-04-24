using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

internal enum BossType { Colossus, Marauder, Dreadnought }

/// <summary>
/// Boss enemy with HP bar, multiple attack phases, and movement patterns.
/// </summary>
internal sealed class BossShip : EnemyBase
{
    private readonly BossType _type;
    private float _moveTimer;
    private float _moveTargetX;
    private float _attackTimer;
    private int _phase;
    private float _entryY;
    private bool _entered;
    private float _time;

    /// <summary>Callbacks for boss attacks.</summary>
    public Action<EnemyBullet>? OnFireBullet { get; set; }
    public Action<float, float>? OnShockwave { get; set; }

    public float HPRatio => MaxHP > 0 ? HP / MaxHP : 0f;

    public BossShip(BossType type) : base(
        type switch
        {
            BossType.Colossus => ColossusHP,
            BossType.Marauder => MarauderHP,
            BossType.Dreadnought => DreadnoughtHP,
            _ => 40
        },
        type switch
        {
            BossType.Colossus => ColossusScore,
            BossType.Marauder => MarauderScore,
            BossType.Dreadnought => DreadnoughtScore,
            _ => 1000
        },
        type switch
        {
            BossType.Colossus => ColossusRadius,
            BossType.Marauder => MarauderWidth / 2f,
            BossType.Dreadnought => DreadnoughtWidth / 2f,
            _ => 40
        })
    {
        _type = type;
        X = GameWidth / 2f;
        Y = -80f;
        _entryY = 80f;
        _moveTargetX = GameWidth / 2f;
        _attackTimer = 2f;
        _phase = 1;
    }

    private float _playerX, _playerY;

    public void SetPlayerPosition(float px, float py)
    {
        _playerX = px;
        _playerY = py;
    }

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        _time += deltaTime;

        // Entry animation
        if (!_entered)
        {
            Y += 40f * deltaTime;
            if (Y >= _entryY)
            {
                Y = _entryY;
                _entered = true;
            }
            return;
        }

        // Phase transitions based on HP ratio
        float hpRatio = HPRatio;
        if (_type == BossType.Dreadnought)
        {
            if (hpRatio < 0.33f && _phase < 3) _phase = 3;
            else if (hpRatio < 0.66f && _phase < 2) _phase = 2;
        }
        else if (_type == BossType.Marauder)
        {
            if (hpRatio < 0.5f && _phase < 2) _phase = 2;
        }

        // Movement: sweep side to side
        float speed = _type switch
        {
            BossType.Colossus => ColossusSpeed,
            BossType.Marauder => MarauderSpeed,
            BossType.Dreadnought => DreadnoughtSpeed * (1f + (3 - _phase) * 0.2f),
            _ => 60f,
        };

        _moveTimer -= deltaTime;
        if (_moveTimer <= 0f)
        {
            _moveTimer = 1.5f + Random.Shared.NextSingle() * 1.5f;
            _moveTargetX = 100f + Random.Shared.NextSingle() * (GameWidth - 200f);
        }

        float dx = _moveTargetX - X;
        if (MathF.Abs(dx) > 2f)
            X += MathF.Sign(dx) * speed * deltaTime;

        // Attack patterns
        _attackTimer -= deltaTime;
        if (_attackTimer <= 0f)
        {
            Attack();
            _attackTimer = _type switch
            {
                BossType.Colossus => 1.2f - _phase * 0.1f,
                BossType.Marauder => _phase == 1 ? 0.8f : 0.5f,
                BossType.Dreadnought => _phase switch
                {
                    1 => 0.7f,
                    2 => 0.5f,
                    _ => 0.3f,
                },
                _ => 1f,
            };
        }
    }

    private void Attack()
    {
        switch (_type)
        {
            case BossType.Colossus:
                // Spread shot downward
                for (int i = -2; i <= 2; i++)
                {
                    float angle = MathF.PI / 2f + i * 0.25f;
                    float vx = MathF.Cos(angle) * EnemyBulletSpeed * 0.8f;
                    float vy = MathF.Sin(angle) * EnemyBulletSpeed * 0.8f;
                    OnFireBullet?.Invoke(new EnemyBullet(X + i * 10f, Y + Radius, vx, vy));
                }
                break;

            case BossType.Marauder:
                if (_phase == 1)
                {
                    // Aimed double shot
                    float dx = _playerX - X;
                    float dy = _playerY - Y;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    if (dist > 1f)
                    {
                        float speed = EnemyBulletSpeed;
                        OnFireBullet?.Invoke(new EnemyBullet(X - 20f, Y + Radius, dx / dist * speed, dy / dist * speed));
                        OnFireBullet?.Invoke(new EnemyBullet(X + 20f, Y + Radius, dx / dist * speed, dy / dist * speed));
                    }
                }
                else
                {
                    // Ring of bullets
                    int count = 8;
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathF.PI * 2f / count * i + _time * 0.5f;
                        float vx = MathF.Cos(angle) * EnemyBulletSpeed * 0.7f;
                        float vy = MathF.Sin(angle) * EnemyBulletSpeed * 0.7f;
                        OnFireBullet?.Invoke(new EnemyBullet(X, Y + Radius * 0.5f, vx, vy));
                    }
                }
                break;

            case BossType.Dreadnought:
                if (_phase == 1)
                {
                    // Triple turret spread
                    for (int t = -1; t <= 1; t++)
                    {
                        float bx = X + t * 40f;
                        float dx = _playerX - bx;
                        float dy = _playerY - Y;
                        float dist = MathF.Sqrt(dx * dx + dy * dy);
                        if (dist > 1f)
                            OnFireBullet?.Invoke(new EnemyBullet(bx, Y + Radius, dx / dist * EnemyBulletSpeed, dy / dist * EnemyBulletSpeed));
                    }
                }
                else if (_phase == 2)
                {
                    // Beam sweep (many bullets in a line)
                    float sweepAngle = MathF.PI / 2f + MathF.Sin(_time * 2f) * 0.6f;
                    for (int i = 0; i < 5; i++)
                    {
                        float a = sweepAngle + (i - 2) * 0.1f;
                        OnFireBullet?.Invoke(new EnemyBullet(X, Y + Radius,
                            MathF.Cos(a) * EnemyBulletSpeed,
                            MathF.Sin(a) * EnemyBulletSpeed));
                    }
                }
                else
                {
                    // Rapid spiral
                    int count = 12;
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathF.PI * 2f / count * i + _time * 3f;
                        float vx = MathF.Cos(angle) * EnemyBulletSpeed * 0.6f;
                        float vy = MathF.Sin(angle) * EnemyBulletSpeed * 0.6f;
                        OnFireBullet?.Invoke(new EnemyBullet(X, Y, vx, vy));
                    }
                    // Plus shockwave
                    OnShockwave?.Invoke(X, Y + Radius);
                }
                break;
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        var color = FlashColor(BossColor);
        using var paint = new SKPaint { Color = color, IsAntialias = true };

        switch (_type)
        {
            case BossType.Colossus:
                DrawColossus(canvas, paint);
                break;
            case BossType.Marauder:
                DrawMarauder(canvas, paint);
                break;
            case BossType.Dreadnought:
                DrawDreadnought(canvas, paint);
                break;
        }

        // HP bar
        DrawHPBar(canvas);
    }

    private void DrawColossus(SKCanvas canvas, SKPaint paint)
    {
        // Giant rocky asteroid
        float r = ColossusRadius;
        using var path = new SKPath();
        var rng = new Random(12345);
        int verts = 12;
        for (int i = 0; i < verts; i++)
        {
            float angle = MathF.PI * 2f * i / verts;
            float variation = 0.75f + rng.NextSingle() * 0.5f;
            float px = MathF.Cos(angle) * r * variation;
            float py = MathF.Sin(angle) * r * variation;
            if (i == 0) path.MoveTo(px, py);
            else path.LineTo(px, py);
        }
        path.Close();

        paint.Color = AsteroidColor;
        canvas.DrawPath(path, paint);

        // Glowing cracks
        paint.Color = BossColor.WithAlpha(150);
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 2f;
        canvas.DrawLine(-r * 0.3f, -r * 0.2f, r * 0.2f, r * 0.3f, paint);
        canvas.DrawLine(r * 0.1f, -r * 0.4f, -r * 0.1f, r * 0.1f, paint);
        paint.Style = SKPaintStyle.Fill;

        // Core glow
        using var glowPaint = new SKPaint { IsAntialias = true };
        glowPaint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(0, 0), r * 0.4f,
            [BossColor.WithAlpha(80), SKColors.Transparent],
            SKShaderTileMode.Clamp);
        canvas.DrawCircle(0, 0, r * 0.4f, glowPaint);
    }

    private void DrawMarauder(SKCanvas canvas, SKPaint paint)
    {
        float w = MarauderWidth / 2f;
        float h = MarauderHeight / 2f;

        // Main hull
        using var path = new SKPath();
        path.MoveTo(0, -h);
        path.LineTo(-w, h * 0.3f);
        path.LineTo(-w * 0.7f, h);
        path.LineTo(w * 0.7f, h);
        path.LineTo(w, h * 0.3f);
        path.Close();
        canvas.DrawPath(path, paint);

        // Wing details
        paint.Color = BossColor.WithAlpha(200);
        canvas.DrawRect(-w + 5f, -h * 0.1f, 8f, h * 0.6f, paint);
        canvas.DrawRect(w - 13f, -h * 0.1f, 8f, h * 0.6f, paint);

        // Bridge
        paint.Color = SKColors.White.WithAlpha(180);
        canvas.DrawRoundRect(-12f, -h * 0.6f, 24f, 14f, 3f, 3f, paint);

        // Phase 2 indicator — glowing engines
        if (_phase >= 2)
        {
            using var glow = new SKPaint { IsAntialias = true };
            glow.Color = RedAccent.WithAlpha(150);
            glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f);
            canvas.DrawCircle(-w * 0.5f, h + 5f, 8f, glow);
            canvas.DrawCircle(w * 0.5f, h + 5f, 8f, glow);
        }
    }

    private void DrawDreadnought(SKCanvas canvas, SKPaint paint)
    {
        float w = DreadnoughtWidth / 2f;
        float h = DreadnoughtHeight / 2f;

        // Massive hull
        using var path = new SKPath();
        path.MoveTo(0, -h);
        path.LineTo(-w * 0.6f, -h * 0.5f);
        path.LineTo(-w, 0);
        path.LineTo(-w, h * 0.7f);
        path.LineTo(-w * 0.3f, h);
        path.LineTo(w * 0.3f, h);
        path.LineTo(w, h * 0.7f);
        path.LineTo(w, 0);
        path.LineTo(w * 0.6f, -h * 0.5f);
        path.Close();
        canvas.DrawPath(path, paint);

        // Turret positions
        paint.Color = SKColors.White.WithAlpha(200);
        canvas.DrawCircle(-40f, 0, 6f, paint);
        canvas.DrawCircle(0, -h * 0.3f, 6f, paint);
        canvas.DrawCircle(40f, 0, 6f, paint);

        // Phase indicators
        SKColor phaseColor = _phase switch
        {
            1 => BossColor,
            2 => OrangeAccent,
            _ => RedAccent,
        };
        paint.Color = phaseColor.WithAlpha(100);
        float pulseR = 10f + 3f * MathF.Sin(_time * 4f);
        canvas.DrawCircle(0, 0, pulseR, paint);

        // Engine glow scales with phase
        using var glow = new SKPaint { IsAntialias = true };
        glow.Color = phaseColor.WithAlpha((byte)(60 + _phase * 30));
        glow.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f);
        canvas.DrawRect(-w * 0.6f, h, w * 1.2f, 10f, glow);
    }

    private void DrawHPBar(SKCanvas canvas)
    {
        float barW = _type switch
        {
            BossType.Colossus => 80f,
            BossType.Marauder => 100f,
            BossType.Dreadnought => 140f,
            _ => 80f,
        };
        float barH = 4f;
        float barY = -Radius - 15f;

        using var bgPaint = new SKPaint { Color = new SKColor(0x33, 0x33, 0x33) };
        using var fgPaint = new SKPaint
        {
            Color = HPRatio > 0.5f ? GreenAccent
                  : HPRatio > 0.25f ? YellowAccent
                  : RedAccent,
        };

        canvas.DrawRoundRect(-barW / 2f, barY, barW, barH, 2f, 2f, bgPaint);
        canvas.DrawRoundRect(-barW / 2f, barY, barW * HPRatio, barH, 2f, 2f, fgPaint);
    }
}
