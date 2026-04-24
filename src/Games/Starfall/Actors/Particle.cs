using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Short-lived visual particle for explosions, hits, and engine effects.
/// </summary>
internal sealed class Particle : Actor
{
    private readonly float _lifetime;
    private float _remaining;
    private readonly SKColor _color;
    private readonly float _size;

    public Particle(float x, float y, float vx, float vy, SKColor color, float lifetime = 0.6f, float size = 3f)
    {
        X = x;
        Y = y;
        _color = color;
        _lifetime = lifetime;
        _remaining = lifetime;
        _size = size;
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(vx, vy);
    }

    protected override void OnUpdate(float deltaTime)
    {
        _remaining -= deltaTime;
        if (_remaining <= 0f)
        {
            Active = false;
            return;
        }
        Alpha = _remaining / _lifetime;

        // Drag
        if (Rigidbody is { } rb)
        {
            rb.VelocityX *= 0.97f;
            rb.VelocityY *= 0.97f;
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        float t = 1f - (_remaining / _lifetime);
        float currentSize = _size * (1f - t * 0.5f);

        using var paint = new SKPaint
        {
            Color = _color,
            IsAntialias = true,
        };
        canvas.DrawCircle(0, 0, currentSize, paint);
    }
}
