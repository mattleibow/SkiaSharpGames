using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>Start/title screen: decorative brick grid + instructions. Click to start the game.</summary>
internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    private readonly UiLabel _title = new() { Text = "BREAKOUT", FontSize = 72f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly UiLabel _startPrompt = new() { Text = "Click or Tap to Start", FontSize = 28f, Color = AccentColor, Align = TextAlign.Center };
    private readonly UiLabel _instructions = new() { Text = "Move mouse / finger / arrow keys to control the paddle", FontSize = 18f, Color = DimColor, Align = TextAlign.Center };

    private readonly List<Brick> _bricks = [];

    public override void OnActivating()
    {
        _bricks.Clear();
        for (int r = 0; r < BrickRows; r++)
        {
            for (int c = 0; c < BrickCols; c++)
            {
                float cx = BricksStartX + c * (BrickWidth + BrickGap) + BrickWidth / 2f;
                float cy = BricksStartY + r * (BrickHeight + BrickGap) + BrickHeight / 2f;
                var brick = new Brick(r, c, cx, cy);
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
            canvas.Save(); canvas.Translate(brick.X, brick.Y); brick.Sprite.Draw(canvas); canvas.Restore();
        }

        canvas.Save(); canvas.Translate(GameWidth / 2f, 290f); _title.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 360f); _startPrompt.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 415f); _instructions.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
