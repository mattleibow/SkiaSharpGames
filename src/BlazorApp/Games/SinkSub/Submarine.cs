using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.BlazorApp.Games.SinkSub;

internal sealed class Submarine : Entity
{
    public readonly RectCollider Collider = new() { Width = SubWidth, Height = SubHeight };
    public readonly Rigidbody2D Rigidbody = new();
    public readonly RectSprite Hull = new()
    {
        Width = SubWidth,
        Height = SubHeight,
        Color = new SKColor(0x3F, 0x6B, 0x7A),
        CornerRadius = 12f,
        ShowShine = true,
    };

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
        Hull.Draw(canvas, X, Y);
        DrawHelper.FillRect(canvas, X - 10f, Y - 18f, 20f, 8f, new SKColor(0x2D, 0x4F, 0x5A));
        DrawHelper.FillRect(canvas, X + 24f, Y - 2f, 14f, 4f, SKColors.White.WithAlpha(110));
    }
}
