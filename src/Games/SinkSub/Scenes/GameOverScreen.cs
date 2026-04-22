using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class GameOverScreen(SinkSubGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _fillPaint = new() { IsAntialias = true };

    private readonly HudLabel _titleText = new() { Text = "SHIP SUNK", FontSize = 62f, Color = new SKColor(0xFF, 0x73, 0x5A), Align = TextAlign.Center };
    private readonly HudLabel _scoreText = new() { FontSize = 30f, Align = TextAlign.Center };
    private readonly HudLabel _waveText = new() { FontSize = 24f, Color = AccentColor, Align = TextAlign.Center };
    private readonly HudLabel _restartText = new() { Text = "Click, tap, or press Space to sail again", FontSize = 22f, Color = DimColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        _fillPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.78f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _fillPaint);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 250f); _titleText.Draw(canvas); canvas.Restore();

        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 320f); _scoreText.Draw(canvas); canvas.Restore();

        _waveText.Text = $"Wave reached: {state.Wave}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 360f); _waveText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, 420f); _restartText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
