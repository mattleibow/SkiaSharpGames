using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>Start/title scene: decorative brick grid + instructions. Click to start the game.</summary>
internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new() { Text = "BREAKOUT", FontSize = 72f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly HudLabel _startPrompt = new() { Text = "Click or Tap to Start", FontSize = 28f, Color = AccentColor, Align = TextAlign.Center };
    private readonly HudLabel _instructions = new() { Text = "Move mouse / finger / arrow keys to control the paddle", FontSize = 18f, Color = DimColor, Align = TextAlign.Center };

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
                brick.Color = BrickColors[r];
                brick.Shimmer.Start(Random.Shared.NextSingle() * brick.Shimmer.Period);
                _bricks.Add(brick);
            }
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        foreach (var brick in _bricks)
            brick.Update(deltaTime);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        foreach (var brick in _bricks)
        {
            brick.Alpha = 0.3f;
            brick.Draw(canvas);
        }

        canvas.Save(); canvas.Translate(GameWidth / 2f, 290f); _title.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 360f); _startPrompt.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 415f); _instructions.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
