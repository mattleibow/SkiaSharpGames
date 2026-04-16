using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class ThrustSprite : Sprite
{
    private static readonly SKPaint _thrustPaint = new()
    {
        Color = ThrustColor,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2f,
    };

    private float _flicker;

    public override void Update(float deltaTime)
    {
        _flicker += deltaTime * 30f;
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible) return;

        float r = ShipRadius;
        float flickerScale = 0.8f + 0.4f * MathF.Sin(_flicker);

        using var path = new SKPath();
        path.MoveTo(-r * 0.35f, r * 0.45f);
        path.LineTo(0, r * (0.7f + 0.5f * flickerScale));
        path.LineTo(r * 0.35f, r * 0.45f);

        canvas.DrawPath(path, _thrustPaint);
    }
}
