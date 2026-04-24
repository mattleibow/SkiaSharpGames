using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>Start/title scene: decorative brick grid + instructions. Click to start the game.</summary>
internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new()
    {
        Text = "BREAKOUT",
        FontSize = 72f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 290f,
    };
    private readonly HudLabel _startPrompt = new()
    {
        Text = "Click or Tap to Start",
        FontSize = 28f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 360f,
    };
    private readonly HudLabel _instructions = new()
    {
        Text = "Move mouse / finger / arrow keys to control the paddle",
        FontSize = 18f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 415f,
    };

    private readonly Actor _brickContainer = new() { Name = "bricks" };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_brickContainer);
            Children.Add(_title);
            Children.Add(_startPrompt);
            Children.Add(_instructions);
        }

        foreach (var child in _brickContainer.Children.ToArray())
            _brickContainer.Children.Remove(child);

        for (int r = 0; r < BrickRows; r++)
        {
            for (int c = 0; c < BrickCols; c++)
            {
                float cx = BricksStartX + c * (BrickWidth + BrickGap) + BrickWidth / 2f;
                float cy = BricksStartY + r * (BrickHeight + BrickGap) + BrickHeight / 2f;
                var brick = new Brick(r, c, cx, cy);
                brick.Color = BrickColors[r];
                brick.Alpha = 0.3f;
                brick.Shimmer.Start(Random.Shared.NextSingle() * brick.Shimmer.Period);
                _brickContainer.Children.Add(brick);
            }
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}