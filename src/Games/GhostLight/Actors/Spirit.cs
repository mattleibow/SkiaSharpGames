using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>The player — a glowing orb that moves through darkness.</summary>
internal sealed class Spirit : Actor
{
    private readonly SKPaint _corePaint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true };

    public Spirit()
    {
        Name = "spirit";
        Collider = new CircleCollider { Radius = PlayerRadius };
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        // Glow: radial gradient
        using var shader = SKShader.CreateRadialGradient(
            new SKPoint(0, 0),
            PlayerGlowRadius,
            new[] { PlayerCoreColor.WithAlpha((byte)(0.9f * 255)), PlayerCoreColor.WithAlpha(0) },
            new float[] { 0f, 1f },
            SKShaderTileMode.Clamp
        );
        _glowPaint.Shader = shader;
        canvas.DrawCircle(0, 0, PlayerGlowRadius, _glowPaint);
        _glowPaint.Shader = null;

        // Core circle
        _corePaint.Color = PlayerCoreColor;
        canvas.DrawCircle(0, 0, PlayerRadius, _corePaint);

        // Inner bright spot
        _corePaint.Color = SKColors.White;
        canvas.DrawCircle(0, 0, PlayerRadius * 0.4f, _corePaint);
    }
}
