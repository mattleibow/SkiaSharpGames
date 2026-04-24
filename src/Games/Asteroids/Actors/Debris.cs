using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

/// <summary>
/// A short-lived particle spawned when an asteroid or ship is destroyed.
/// </summary>
internal sealed class Debris : Actor
{
    private static readonly SKPaint _paint = new()
    {
        Color = DebrisColor,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.5f,
    };

    public float Lifetime { get; set; } = DebrisLifetime;
    private readonly float _initialLifetime = DebrisLifetime;

    public Debris(float x, float y, float vx, float vy)
    {
        X = x;
        Y = y;
        Rigidbody = new Rigidbody2D { VelocityX = vx, VelocityY = vy };
    }

    protected override void OnUpdate(float deltaTime)
    {
        Lifetime -= deltaTime;
        if (Lifetime <= 0f)
        {
            Active = false;
            return;
        }

        Alpha = Lifetime / _initialLifetime;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        byte alpha = (byte)(255 * Math.Clamp(Alpha, 0f, 1f));
        _paint.Color = DebrisColor.WithAlpha(alpha);
        canvas.DrawLine(-3f, 0f, 3f, 0f, _paint);
    }
}