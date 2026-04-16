using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class AsteroidSprite : Sprite
{
    private static readonly SKPaint _strokePaint = new()
    {
        Color = AsteroidColor,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.5f,
    };

    private readonly SKPath _shapePath;

    public float Radius { get; }

    public AsteroidSprite(float radius, int seed)
    {
        Radius = radius;
        _shapePath = GenerateShape(radius, seed);
    }

    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawPath(_shapePath, _strokePaint);
    }

    private static SKPath GenerateShape(float radius, int seed)
    {
        var random = new Random(seed);
        int vertices = AsteroidVertices;
        var path = new SKPath();

        for (int i = 0; i < vertices; i++)
        {
            float angle = MathF.PI * 2f * i / vertices;
            float variation = 0.7f + random.NextSingle() * 0.6f;
            float r = radius * variation;
            float px = MathF.Cos(angle) * r;
            float py = MathF.Sin(angle) * r;

            if (i == 0)
                path.MoveTo(px, py);
            else
                path.LineTo(px, py);
        }

        path.Close();
        return path;
    }
}
