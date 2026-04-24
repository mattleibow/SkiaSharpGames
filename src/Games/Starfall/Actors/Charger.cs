using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Enemy that stalks toward the player, then rushes forward at high speed.
/// </summary>
internal sealed class Charger : EnemyBase
{
    private enum ChargerState { Stalking, Locking, Rushing }

    private ChargerState _state = ChargerState.Stalking;
    private float _lockTimer;
    private float _rushDirX;
    private float _rushDirY;
    private float _playerX;
    private float _playerY;
    private float _pulseTime;

    public Charger(float x, float y)
        : base(ChargerHP, ChargerScore, ChargerRadius)
    {
        X = x;
        Y = y;
        Rigidbody = new Rigidbody2D();
    }

    public void SetPlayerPosition(float px, float py)
    {
        _playerX = px;
        _playerY = py;
    }

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        _pulseTime += deltaTime;

        switch (_state)
        {
            case ChargerState.Stalking:
                // Move slowly toward player
                float dx = _playerX - X;
                float dy = _playerY - Y;
                float dist = MathF.Sqrt(dx * dx + dy * dy);
                if (dist > 1f)
                {
                    Rigidbody!.SetVelocity(
                        dx / dist * ChargerStalkSpeed,
                        dy / dist * ChargerStalkSpeed);
                }

                // When close enough or after some time in the game area, start locking
                if (Y > 100f && (dist < 200f || Y > GameHeight * 0.4f))
                {
                    _state = ChargerState.Locking;
                    _lockTimer = ChargerLockOnTime;
                    Rigidbody!.Stop();
                }
                break;

            case ChargerState.Locking:
                _lockTimer -= deltaTime;
                if (_lockTimer <= 0f)
                {
                    _state = ChargerState.Rushing;
                    dx = _playerX - X;
                    dy = _playerY - Y;
                    dist = MathF.Sqrt(dx * dx + dy * dy);
                    if (dist > 1f)
                    {
                        _rushDirX = dx / dist;
                        _rushDirY = dy / dist;
                    }
                    else
                    {
                        _rushDirX = 0;
                        _rushDirY = 1;
                    }
                    Rigidbody!.SetVelocity(
                        _rushDirX * ChargerRushSpeed,
                        _rushDirY * ChargerRushSpeed);
                }
                break;

            case ChargerState.Rushing:
                // Just keep going in rush direction (rigidbody handles it)
                break;
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        var baseColor = _state == ChargerState.Locking
            ? SKColors.White // Flash white while locking
            : ChargerColor;
        var color = FlashColor(baseColor);
        using var paint = new SKPaint { Color = color, IsAntialias = true };

        // Arrow/chevron shape pointing toward rush direction
        float r = ChargerRadius;

        // Pulsing effect while locking
        if (_state == ChargerState.Locking)
        {
            float pulse = 0.7f + 0.3f * MathF.Sin(_pulseTime * 12f);
            paint.Color = ChargerColor.WithAlpha((byte)(255 * pulse));
        }

        using var path = new SKPath();
        path.MoveTo(0, -r);
        path.LineTo(-r * 0.8f, r * 0.3f);
        path.LineTo(-r * 0.3f, r * 0.1f);
        path.LineTo(0, r);
        path.LineTo(r * 0.3f, r * 0.1f);
        path.LineTo(r * 0.8f, r * 0.3f);
        path.Close();
        canvas.DrawPath(path, paint);

        // Eye
        paint.Color = _state == ChargerState.Locking
            ? RedAccent
            : SKColors.White.WithAlpha(200);
        canvas.DrawCircle(0, -r * 0.2f, 2.5f, paint);
    }
}
