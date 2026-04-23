using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// Stage-over scene drawn on top of the frozen play scene.
/// Does not clear the canvas — relies on the base play scene being drawn first.
/// </summary>
internal sealed class GameOverScreen(BreakoutGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha(186) };

    private readonly HudLabel _titleText = new() { Text = "GAME OVER", FontSize = 64f, Color = new SKColor(0xFF, 0x2D, 0x55), Align = TextAlign.Center, X = GameWidth / 2f, Y = 270f };
    private readonly HudLabel _scoreText = new() { FontSize = 32f, Color = SKColors.White, Align = TextAlign.Center, X = GameWidth / 2f, Y = 335f };
    private readonly HudLabel _promptText = new() { Text = "Click or Tap to Play Again", FontSize = 24f, Color = AccentColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 395f };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_titleText);
            Children.Add(_scoreText);
            Children.Add(_promptText);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);
        _scoreText.Text = $"Score: {state.Score}";
    }

    public override void OnPointerDown(float x, float y)
        => director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
