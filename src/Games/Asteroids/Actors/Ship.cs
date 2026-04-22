using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class Ship : Actor
{
    private static readonly SKPaint _strokePaint = new()
    {
        Color = ShipColor,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2f,
    };

    private static readonly SKPaint _thrustPaint = new()
    {
        Color = ThrustColor,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2f,
    };

    private float _flicker;

    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public bool Thrusting { get; set; }
    public float InvincibleTimer { get; set; }
    public bool IsInvincible => InvincibleTimer > 0f;

    public Ship()
    {
        Collider = new CircleCollider { Radius = ShipRadius * 0.65f };
    }

    protected override void OnUpdate(float deltaTime)
    {
        // Apply drag
        VelocityX *= ShipDrag;
        VelocityY *= ShipDrag;

        // Clamp speed
        float speed = MathF.Sqrt(VelocityX * VelocityX + VelocityY * VelocityY);
        if (speed > ShipMaxSpeed)
        {
            float scale = ShipMaxSpeed / speed;
            VelocityX *= scale;
            VelocityY *= scale;
        }

        // Move
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;

        // Screen wrap
        WrapPosition();

        // Update invincibility
        if (InvincibleTimer > 0f)
        {
            InvincibleTimer -= deltaTime;
            float blink = MathF.Sin(InvincibleTimer * 15f);
            Alpha = blink > 0f ? 1f : 0.2f;
        }
        else
        {
            Alpha = 1f;
        }

        // Advance thrust flicker animation
        if (Thrusting)
            _flicker += deltaTime * 30f;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        byte alpha = (byte)(255 * Math.Clamp(Alpha, 0f, 1f));

        // Draw thrust flame behind ship
        if (Thrusting)
        {
            float r = ShipRadius;
            float flickerScale = 0.8f + 0.4f * MathF.Sin(_flicker);

            using var thrustPath = new SKPath();
            thrustPath.MoveTo(-r * 0.35f, r * 0.45f);
            thrustPath.LineTo(0, r * (0.7f + 0.5f * flickerScale));
            thrustPath.LineTo(r * 0.35f, r * 0.45f);

            canvas.DrawPath(thrustPath, _thrustPaint);
        }

        // Draw ship hull
        _strokePaint.Color = ShipColor.WithAlpha(alpha);

        float sr = ShipRadius;
        using var path = new SKPath();
        path.MoveTo(0, -sr);
        path.LineTo(-sr * 0.7f, sr * 0.7f);
        path.LineTo(0, sr * 0.35f);
        path.LineTo(sr * 0.7f, sr * 0.7f);
        path.Close();

        canvas.DrawPath(path, _strokePaint);
    }

    private void WrapPosition()
    {
        if (X < -ShipRadius) X = GameWidth + ShipRadius;
        else if (X > GameWidth + ShipRadius) X = -ShipRadius;
        if (Y < -ShipRadius) Y = GameHeight + ShipRadius;
        else if (Y > GameHeight + ShipRadius) Y = -ShipRadius;
    }
}
