using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>An enemy shadow that drifts through the fog and fades in over time.</summary>
internal sealed class ShadowBlob : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public float Radius { get; set; }
    public SKColor Color { get; set; } = new SKColor(0x20, 0x10, 0x30);

    public ShadowBlob(float radius, float vx, float vy)
    {
        Radius = radius;
        Collider = new CircleCollider { Radius = radius * 0.7f };
        Rigidbody = new Rigidbody2D { VelocityX = vx, VelocityY = vy };
        Alpha = 0f;
    }

    protected override void OnUpdate(float deltaTime)
    {
        // Fade in
        if (Alpha < 0.85f)
            Alpha = Math.Min(Alpha + deltaTime * 0.5f, 0.85f);

        // Screen wrapping
        if (X < -Radius * 2)
            X = GameWidth + Radius;
        if (X > GameWidth + Radius * 2)
            X = -Radius;
        if (Y < -Radius * 2)
            Y = GameHeight + Radius;
        if (Y > GameHeight + Radius * 2)
            Y = -Radius;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        using var shader = SKShader.CreateRadialGradient(
            new SKPoint(0, 0),
            Radius,
            [Color, Color.WithAlpha(0)],
            SKShaderTileMode.Clamp
        );
        _paint.Shader = shader;
        canvas.DrawCircle(0, 0, Radius, _paint);
        _paint.Shader = null;
    }
}
