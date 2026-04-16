using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class ShipSprite : Sprite
{
    private static readonly SKPaint _strokePaint = new()
    {
        Color = ShipColor,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2f,
    };

    public override void Draw(SKCanvas canvas)
    {
        byte alpha = (byte)(255 * Math.Clamp(Alpha, 0f, 1f));
        _strokePaint.Color = ShipColor.WithAlpha(alpha);

        float r = ShipRadius;
        using var path = new SKPath();
        // Triangle ship pointing up (nose at top)
        path.MoveTo(0, -r);                          // nose
        path.LineTo(-r * 0.7f, r * 0.7f);            // bottom-left
        path.LineTo(0, r * 0.35f);                    // rear indent
        path.LineTo(r * 0.7f, r * 0.7f);              // bottom-right
        path.Close();

        canvas.DrawPath(path, _strokePaint);
    }
}
