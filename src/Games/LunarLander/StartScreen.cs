using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>Title screen: game title, instructions, click to start.</summary>
internal sealed class StartScreen(IDirector coordinator) : Scene
{
    private readonly UiLabel _title = new() { Text = "LUNAR LANDER", FontSize = 64f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly UiLabel _subtitle = new() { Text = "Actor Rotation & Child Entities Demo", FontSize = 18f, Color = AccentColor, Align = TextAlign.Center };
    private readonly UiLabel _startPrompt = new() { Text = "Click or Tap to Start", FontSize = 28f, Color = AccentColor, Align = TextAlign.Center };
    private readonly UiLabel _instructions1 = new() { Text = "LEFT RIGHT rotate    UP / SPACE thrust", FontSize = 18f, Color = DimColor, Align = TextAlign.Center };
    private readonly UiLabel _instructions2 = new() { Text = "Land gently on the green pad", FontSize = 18f, Color = DimColor, Align = TextAlign.Center };

    private readonly SKPoint[] _stars = new SKPoint[StarCount];
    private readonly float[] _starBrightness = new float[StarCount];
    private readonly Random _rng = new(42);

    public override void OnActivating()
    {
        for (int i = 0; i < StarCount; i++)
        {
            _stars[i] = new SKPoint(
                _rng.Next(0, GameWidth),
                _rng.Next(0, GameHeight));
            _starBrightness[i] = 0.3f + (float)_rng.NextDouble() * 0.7f;
        }
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        // Stars
        using var starPaint = new SKPaint { IsAntialias = true };
        for (int i = 0; i < StarCount; i++)
        {
            byte a = (byte)(255 * _starBrightness[i]);
            starPaint.Color = SKColors.White.WithAlpha(a);
            canvas.DrawCircle(_stars[i].X, _stars[i].Y, 1.2f, starPaint);
        }

        // Simple lander illustration
        DrawLanderIcon(canvas, GameWidth / 2f, 200f);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 300f); _title.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 335f); _subtitle.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 400f); _startPrompt.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 450f); _instructions1.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 478f); _instructions2.Draw(canvas); canvas.Restore();
    }

    private static void DrawLanderIcon(SKCanvas canvas, float cx, float cy)
    {
        using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = LanderColor };
        using var path = new SKPath();
        float s = 2.5f; // scale
        path.MoveTo(cx, cy - 12f * s);
        path.LineTo(cx + 15f * s, cy + 12f * s);
        path.LineTo(cx - 15f * s, cy + 12f * s);
        path.Close();
        canvas.DrawPath(path, paint);
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
