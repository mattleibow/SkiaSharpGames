using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>Draws a flickering thruster flame at origin pointing downward in local space.</summary>
internal sealed class FlameSprite : Sprite
{
    private readonly SKPaint _outerPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _innerPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    private float _flicker;

    /// <summary>Controls the size of the flame (0–1).</summary>
    public float Intensity { get; set; } = 1f;

    public override void Update(float deltaTime)
    {
        _flicker = 0.8f + 0.2f * MathF.Sin(Environment.TickCount * 0.03f + GetHashCode());
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible || Alpha <= 0f || Intensity <= 0f)
            return;

        byte alpha = (byte)(220 * Alpha);
        float length = 8f + 10f * Intensity * _flicker;
        float halfWidth = 4f * Intensity * _flicker;

        // Outer flame (orange)
        _outerPaint.Color = FlameColor.WithAlpha(alpha);
        using var outerPath = new SKPath();
        outerPath.MoveTo(-halfWidth, 0);
        outerPath.LineTo(halfWidth, 0);
        outerPath.LineTo(0, length);
        outerPath.Close();
        canvas.DrawPath(outerPath, _outerPaint);

        // Inner flame (yellow, smaller)
        float innerHalfW = halfWidth * 0.5f;
        float innerLen = length * 0.6f;
        _innerPaint.Color = FlameInnerColor.WithAlpha(alpha);
        using var innerPath = new SKPath();
        innerPath.MoveTo(-innerHalfW, 0);
        innerPath.LineTo(innerHalfW, 0);
        innerPath.LineTo(0, innerLen);
        innerPath.Close();
        canvas.DrawPath(innerPath, _innerPaint);
    }
}
