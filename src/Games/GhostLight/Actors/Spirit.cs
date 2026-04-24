using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>The player — a glowing orb that moves through darkness.</summary>
internal sealed class Spirit : Actor
{
    private readonly SKPaint _corePaint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true };

    public SKColor CoreColor { get; set; } = new SKColor(0xCC, 0xEE, 0xFF);
    public SKColor GlowColor { get; set; } = new SKColor(0x66, 0xBB, 0xFF);
    public float GlowRadius { get; set; } = PlayerGlowRadius;

    public Spirit()
    {
        Name = "spirit";
        Collider = new CircleCollider { Radius = PlayerRadius };
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        // Outer glow (radial gradient)
        using var shader = SKShader.CreateRadialGradient(
            new SKPoint(0, 0),
            GlowRadius,
            [GlowColor.WithAlpha(120), GlowColor.WithAlpha(0)],
            SKShaderTileMode.Clamp
        );
        _glowPaint.Shader = shader;
        canvas.DrawCircle(0, 0, GlowRadius, _glowPaint);
        _glowPaint.Shader = null;

        // Core bright circle
        _corePaint.Color = CoreColor;
        canvas.DrawCircle(0, 0, PlayerRadius, _corePaint);

        // Inner bright spot
        _corePaint.Color = SKColors.White;
        canvas.DrawCircle(0, 0, PlayerRadius * 0.4f, _corePaint);
    }
}
