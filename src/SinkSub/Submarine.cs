using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class Submarine : Entity
{
    public readonly RectCollider Collider = new() { Width = SubWidth, Height = SubHeight };
    public readonly Rigidbody2D Rigidbody = new();
    public readonly SubmarineSprite Sprite = new();

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

    public void Draw(SKCanvas canvas)
    {
        canvas.Save();
        canvas.Translate(X, Y);
        Sprite.Draw(canvas);
        canvas.Restore();
    }
}
