using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Basic enemy that flies straight down.
/// </summary>
internal sealed class Drone : EnemyBase
{
    private readonly float _speed;

    public Drone(float x, float y, float speed = DroneSpeed)
        : base(DroneHP, DroneScore, DroneRadius)
    {
        X = x;
        Y = y;
        _speed = speed;
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(0, _speed);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        var color = FlashColor(DroneColor);
        using var paint = new SKPaint { Color = color, IsAntialias = true };

        // Diamond shape
        using var path = new SKPath();
        path.MoveTo(0, -DroneRadius);
        path.LineTo(-DroneRadius * 0.7f, 0);
        path.LineTo(0, DroneRadius * 0.6f);
        path.LineTo(DroneRadius * 0.7f, 0);
        path.Close();
        canvas.DrawPath(path, paint);

        // Core
        paint.Color = SKColors.White.WithAlpha(180);
        canvas.DrawCircle(0, 0, 3f, paint);
    }
}
