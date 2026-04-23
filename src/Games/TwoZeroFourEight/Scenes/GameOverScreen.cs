using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.TwoZeroFourEight.TwoZeroFourEightConstants;

namespace SkiaSharpGames.TwoZeroFourEight;

internal sealed class GameOverScreen(TwoZeroFourEightGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha((byte)(255 * 0.65f)), IsAntialias = true };

    private readonly HudLabel _titleText = new() { Text = "GAME OVER", FontSize = 72f, Color = LightTextColor, Align = TextAlign.Center };
    private readonly HudLabel _scoreText = new() { FontSize = 30f, Color = LightTextColor, Align = TextAlign.Center };
    private readonly HudLabel _bestText = new() { FontSize = 28f, Color = LightTextColor, Align = TextAlign.Center };
    private readonly HudLabel _restartText = new() { Text = "Click, tap, Enter, or Space to restart", FontSize = 22f, Color = LightTextColor, Align = TextAlign.Center };

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 250f); _titleText.Draw(canvas); canvas.Restore();

        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 320f); _scoreText.Draw(canvas); canvas.Restore();

        _bestText.Text = $"Best: {state.BestScore}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 360f); _bestText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, 425f); _restartText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
