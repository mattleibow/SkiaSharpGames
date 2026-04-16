using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    private readonly TextSprite _title = new() { Text = "SNAKE", Size = 78f, Align = TextAlign.Center };
    private readonly TextSprite _subtitle = new() { Text = "Eat, grow, survive", Size = 30f, Color = AccentColor, Align = TextAlign.Center };
    private readonly TextSprite _instruction1 = new() { Text = "Arrow keys or WASD to steer", Size = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly TextSprite _instruction2 = new() { Text = "Eat the red food to grow", Size = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly TextSprite _instruction3 = new() { Text = "Don't hit the walls or yourself", Size = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly TextSprite _startPrompt = new() { Text = "Click, tap, or press Space to start", Size = 24f, Align = TextAlign.Center };

    // Decorative snake preview
    private static readonly SKPaint _previewPaint = new() { IsAntialias = true };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        // Draw a small decorative snake icon
        float cx = GameWidth / 2f;
        _previewPaint.Color = SnakeHeadColor;
        canvas.DrawRoundRect(cx - 12f, 140f, CellSize - 2, CellSize - 2, 4f, 4f, _previewPaint);
        _previewPaint.Color = SnakeBodyColor;
        for (int i = 1; i <= 4; i++)
            canvas.DrawRoundRect(cx - 12f + i * CellSize, 140f, CellSize - 2, CellSize - 2, 4f, 4f, _previewPaint);

        canvas.Save(); canvas.Translate(cx, 225f); _title.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(cx, 280f); _subtitle.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(cx, 345f); _instruction1.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(cx, 377f); _instruction2.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(cx, 409f); _instruction3.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(cx, 474f); _startPrompt.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
