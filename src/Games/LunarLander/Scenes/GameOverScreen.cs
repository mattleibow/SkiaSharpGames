using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>
/// Stage-over scene drawn on top of the frozen play scene.
/// Shows "LANDED SAFELY!" or "CRASHED!" based on the game state.
/// </summary>
internal sealed class GameOverScreen(LunarLanderGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha(186) };

    private readonly HudLabel _titleText = new() { FontSize = 56f, Align = TextAlign.Center };
    private readonly HudLabel _detailText = new() { FontSize = 24f, Color = DimColor, Align = TextAlign.Center };
    private readonly HudLabel _promptText = new() { Text = "Click or Tap to Play Again", FontSize = 24f, Color = AccentColor, Align = TextAlign.Center };

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);

        if (state.Landed)
        {
            _titleText.Text = "LANDED SAFELY!";
            _titleText.Color = SuccessColor;
            _detailText.Text = $"Fuel remaining: {state.Fuel:F0}";
        }
        else
        {
            _titleText.Text = "CRASHED!";
            _titleText.Color = DangerColor;
            _detailText.Text = "Too fast or wrong angle!";
        }

        canvas.Save(); canvas.Translate(GameWidth / 2f, 260f); _titleText.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 310f); _detailText.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 380f); _promptText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
