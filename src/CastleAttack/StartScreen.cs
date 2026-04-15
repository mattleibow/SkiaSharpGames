using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Start/title screen for Castle Attack. Shows the background and instructions.</summary>
internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    // ── Cached paints ─────────────────────────────────────────────────────
    private static readonly SKPaint OverlayPaint = new();
    private static readonly SKPaint SkyPaint = new();
    private static readonly SKPaint GroundPaint = new() { Color = ColGround };
    private static readonly SKPaint GroundEdgePaint = new() { Color = ColGroundEdge };
    private static readonly SKPaint HillPaint = new() { Color = new SKColor(0x2A, 0x20, 0x12), IsAntialias = true };
    private static readonly SKPath HillPath;

    // ── Text sprites ──────────────────────────────────────────────────────
    private readonly TextSprite _title = new() { Text = "CASTLE ATTACK", Size = 68f, Color = ColGold, Align = TextAlign.Center };
    private readonly TextSprite _subtitle = new() { Text = "Defend the castle until the keep is complete!", Size = 22f, Color = ColHud, Align = TextAlign.Center };
    private readonly TextSprite _tapLine = new() { Text = "Tap the battlefield to aim & fire", Size = 17f, Color = ColAccent, Align = TextAlign.Center };
    private readonly TextSprite _btnLine = new() { Text = "Use the on-screen buttons at the bottom for all actions", Size = 16f, Color = ColDim, Align = TextAlign.Center };
    private readonly TextSprite _kbLine = new() { Text = "Keyboard: ← → aim  |  SPACE fire  |  ↑↓ convert  |  Z X C weapons", Size = 14f, Color = ColDim, Align = TextAlign.Center };
    private readonly TextSprite _startLine = new() { Text = "Tap or Click to Start", Size = 24f, Color = ColAccent, Align = TextAlign.Center };

    static StartScreen()
    {
        var skyShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, GroundY),
            [ColSky, ColHorizon], [0f, 1f], SKShaderTileMode.Clamp);
        SkyPaint.Shader = skyShader;

        HillPath = new SKPath();
        HillPath.MoveTo(0, GroundY);
        HillPath.CubicTo(200, GroundY - 60, 400, GroundY - 80, 600, GroundY - 40);
        HillPath.CubicTo(800, GroundY, 900, GroundY - 50, 1100, GroundY - 30);
        HillPath.LineTo(GameWidth, GroundY);
        HillPath.LineTo(0, GroundY);
        HillPath.Close();
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(ColSky);

        DrawBackground(canvas);

        OverlayPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.6f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), OverlayPaint);

        float cx = GameWidth / 2f;
        _title.Draw(canvas, cx, 190f);
        _subtitle.Draw(canvas, cx, 258f);

        float y = 308f;
        _tapLine.Draw(canvas, cx, y);
        y += 26f;
        _btnLine.Draw(canvas, cx, y);
        y += 26f;
        _kbLine.Draw(canvas, cx, y);

        _startLine.Draw(canvas, cx, 420f);
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    private static void DrawBackground(SKCanvas canvas)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GroundY), SkyPaint);
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, GameHeight - GroundY), GroundPaint);
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, 4f), GroundEdgePaint);
        canvas.DrawPath(HillPath, HillPaint);
    }
}
