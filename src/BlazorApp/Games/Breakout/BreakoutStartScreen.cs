using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.Breakout.BreakoutConstants;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>Start/title screen: decorative brick grid + instructions. Click to start the game.</summary>
internal sealed class BreakoutStartScreen : GameScreen
{
    private readonly List<Brick> _bricks = [];

    public override void OnActivated()
    {
        _bricks.Clear();
        for (int r = 0; r < BrickRows; r++)
        {
            for (int c = 0; c < BrickCols; c++)
            {
                float bx = BricksStartX + c * (BrickWidth + BrickGap);
                float by = BricksStartY + r * (BrickHeight + BrickGap);
                var brick = new Brick(r, c, bx, by);
                brick.Sprite.Color = BrickColors[r];
                brick.Sprite.Shimmer.Start(Random.Shared.NextSingle() * brick.Sprite.Shimmer.Period);
                _bricks.Add(brick);
            }
        }
    }

    public override void Update(float deltaTime)
    {
        foreach (var brick in _bricks)
            brick.Sprite.Update(deltaTime);
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        foreach (var brick in _bricks)
        {
            brick.Sprite.Alpha = 0.3f;
            brick.Sprite.Draw(canvas);
        }

        DrawHelper.DrawCenteredText(canvas, "BREAKOUT", 72f, SKColors.White, GameWidth / 2f, 290f);
        DrawHelper.DrawCenteredText(canvas, "Click or Tap to Start", 28f, AccentColor, GameWidth / 2f, 360f);
        DrawHelper.DrawCenteredText(canvas, "Move mouse / finger to control the paddle", 18f, DimColor, GameWidth / 2f, 415f);
    }

    public override void OnPointerDown(float x, float y)
        => Game?.TransitionTo<BreakoutPlayScreen>(new DissolveTransition());
}
