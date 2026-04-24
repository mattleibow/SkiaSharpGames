using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Enemy that moves downward in a sine-wave pattern.
/// </summary>
internal sealed class Zigzagger : EnemyBase
{
    private readonly float _startX;
    private float _time;

    public Zigzagger(float x, float y)
        : base(ZigzaggerHP, ZigzaggerScore, ZigzaggerRadius)
    {
        X = x;
        Y = y;
        _startX = x;
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(0, ZigzaggerSpeed);
    }

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        _time += deltaTime;
        // Override X for sine wave
        X = _startX + MathF.Sin(_time * ZigzaggerFrequency) * ZigzaggerAmplitude;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        var color = FlashColor(ZigzaggerColor);
        using var paint = new SKPaint { Color = color, IsAntialias = true };

        // Hexagonal shape
        using var path = new SKPath();
        float r = ZigzaggerRadius;
        for (int i = 0; i < 6; i++)
        {
            float angle = MathF.PI / 3f * i - MathF.PI / 6f;
            float px = MathF.Cos(angle) * r;
            float py = MathF.Sin(angle) * r;
            if (i == 0) path.MoveTo(px, py);
            else path.LineTo(px, py);
        }
        path.Close();
        canvas.DrawPath(path, paint);

        // Inner glow
        paint.Color = SKColors.White.WithAlpha(120);
        canvas.DrawCircle(0, 0, 4f, paint);
    }
}
