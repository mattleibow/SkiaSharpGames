using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Heavy, slow enemy that periodically emits expanding shockwave rings.
/// </summary>
internal sealed class Bomber : EnemyBase
{
    private float _shockwaveTimer;

    /// <summary>Callback to spawn a shockwave ring.</summary>
    public Action<float, float>? OnShockwave { get; set; }

    public Bomber(float x, float y)
        : base(BomberHP, BomberScore, BomberRadius)
    {
        X = x;
        Y = y;
        _shockwaveTimer = BomberShockwaveRate;
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(0, BomberSpeed);
    }

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        _shockwaveTimer -= deltaTime;
        if (_shockwaveTimer <= 0f && Y > 40f && Y < GameHeight - 60f)
        {
            _shockwaveTimer = BomberShockwaveRate;
            OnShockwave?.Invoke(X, Y);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        var color = FlashColor(BomberColor);
        using var paint = new SKPaint { Color = color, IsAntialias = true };

        // Chunky octagon
        float r = BomberRadius;
        using var path = new SKPath();
        for (int i = 0; i < 8; i++)
        {
            float angle = MathF.PI / 4f * i - MathF.PI / 8f;
            float px = MathF.Cos(angle) * r;
            float py = MathF.Sin(angle) * r;
            if (i == 0) path.MoveTo(px, py);
            else path.LineTo(px, py);
        }
        path.Close();
        canvas.DrawPath(path, paint);

        // Inner danger symbol
        paint.Color = SKColors.White.WithAlpha(150);
        float ir = r * 0.4f;
        using var inner = new SKPath();
        inner.MoveTo(0, -ir);
        inner.LineTo(-ir * 0.87f, ir * 0.5f);
        inner.LineTo(ir * 0.87f, ir * 0.5f);
        inner.Close();
        canvas.DrawPath(inner, paint);
    }
}
