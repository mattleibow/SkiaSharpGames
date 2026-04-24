using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>An ambient fog particle that drifts slowly for parallax effect.</summary>
internal sealed class FogParticle : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly float _radius;

    public SKColor Color { get; set; } = new SKColor(0x20, 0x18, 0x35);

    public FogParticle(float radius, float vx, float vy)
    {
        _radius = radius;
        Rigidbody = new Rigidbody2D { VelocityX = vx, VelocityY = vy };
        Alpha = 0.5f;
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (X < -_radius * 2)
            X = GameWidth + _radius;
        if (X > GameWidth + _radius * 2)
            X = -_radius;
        if (Y < -_radius * 2)
            Y = GameHeight + _radius;
        if (Y > GameHeight + _radius * 2)
            Y = -_radius;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        using var shader = SKShader.CreateRadialGradient(
            new SKPoint(0, 0),
            _radius,
            [Color.WithAlpha(60), Color.WithAlpha(0)],
            SKShaderTileMode.Clamp
        );
        _paint.Shader = shader;
        canvas.DrawCircle(0, 0, _radius, _paint);
        _paint.Shader = null;
    }
}
