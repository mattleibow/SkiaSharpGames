using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class Submarine : Entity
{
    public Submarine()
    {
        Collider = new RectCollider { Width = SubWidth, Height = SubHeight };
        Rigidbody = new Rigidbody2D();
        Sprite = new SubmarineSprite();
    }

    public new SubmarineSprite Sprite { get => (SubmarineSprite)base.Sprite!; init => base.Sprite = value; }
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
}
