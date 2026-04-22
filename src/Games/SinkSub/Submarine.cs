using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class Submarine : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public Submarine()
    {
        Collider = new RectCollider { Width = SubWidth, Height = SubHeight };
        Rigidbody = new Rigidbody2D();
    }

    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }

    public float CruiseSpeed { get; private set; }

    public int Direction { get; private set; } = 1;

    private CountdownTimer _mineTimer;

    public void Reset(float x, float y, float speed, int direction, float firstMineDelay)
    {
        X = x;
        Y = y;
        Active = true;
        CruiseSpeed = speed;
        Direction = direction >= 0 ? 1 : -1;
        Rigidbody.SetVelocity(CruiseSpeed * Direction, 0f);
        _mineTimer.Set(firstMineDelay);
    }

    public bool TickMineTimer(float deltaTime)
    {
        if (!_mineTimer.Tick(deltaTime))
            return false;

        _mineTimer.Set(1.8f + Random.Shared.NextSingle() * 1.8f);
        return true;
    }

    public void Reverse()
    {
        Direction = -Direction;
        Rigidbody.SetVelocity(CruiseSpeed * Direction, 0f);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        byte a = (byte)(255 * Alpha);

        _paint.Color = new SKColor(0x3F, 0x6B, 0x7A).WithAlpha(a);
        canvas.DrawRoundRect(SKRect.Create(0f - SubWidth / 2f, 0f - SubHeight / 2f, SubWidth, SubHeight), 12f, 12f, _paint);

        _paint.Color = new SKColor(0x2D, 0x4F, 0x5A).WithAlpha(a);
        canvas.DrawRect(SKRect.Create(0f - 10f, 0f - 18f, 20f, 8f), _paint);

        _paint.Color = SKColors.White.WithAlpha(110).WithAlpha(a);
        canvas.DrawRect(SKRect.Create(0f + 24f, 0f - 2f, 14f, 4f), _paint);
    }
}
