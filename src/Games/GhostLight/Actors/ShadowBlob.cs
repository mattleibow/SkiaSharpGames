using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>An enemy shadow that chases the player with proximity-based alpha.</summary>
internal sealed class ShadowBlob : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public float Radius { get; set; }
    public Actor? Target { get; set; }

    public ShadowBlob(float radius)
    {
        Radius = radius;
        Collider = new CircleCollider { Radius = radius * 0.7f };
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (Target is not null)
        {
            float dx = Target.X - X;
            float dy = Target.Y - Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist > 1f)
            {
                X += dx / dist * EnemySpeed * deltaTime;
                Y += dy / dist * EnemySpeed * deltaTime;
            }

            // Alpha based on proximity: closer = more visible
            float maxDist = 300f;
            float proximity = 1f - Math.Clamp(dist / maxDist, 0f, 1f);
            Alpha = 0.3f + 0.4f * proximity;
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        _paint.Color = SKColors.Black;
        canvas.DrawCircle(0, 0, Radius, _paint);
    }
}
