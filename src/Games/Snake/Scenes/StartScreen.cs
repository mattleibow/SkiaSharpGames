using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new() { Text = "SNAKE", FontSize = 78f, Align = TextAlign.Center };
    private readonly HudLabel _subtitle = new() { Text = "Eat, grow, survive", FontSize = 30f, Color = AccentColor, Align = TextAlign.Center };
    private readonly HudLabel _instruction1 = new() { Text = "Arrow keys or WASD to steer", FontSize = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly HudLabel _instruction2 = new() { Text = "Eat the red food to grow", FontSize = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly HudLabel _instruction3 = new() { Text = "Don't hit the walls or yourself", FontSize = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly HudLabel _startPrompt = new() { Text = "Click, tap, or press Space to start", FontSize = 24f, Align = TextAlign.Center };

    // Decorative snake preview
    private static readonly SKPaint _previewPaint = new() { IsAntialias = true };

    protected override void OnDraw(SKCanvas canvas)
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
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
