using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Start/title scene for Castle Attack. Shows the background and instructions.</summary>
internal sealed class StartScreen(IDirector director) : Scene
{
    // ── Cached paints ─────────────────────────────────────────────────────
    private static readonly SKPaint OverlayPaint = new();
    private static readonly SKPaint SkyPaint = new();
    private static readonly SKPaint GroundPaint = new() { Color = ColGround };
    private static readonly SKPaint GroundEdgePaint = new() { Color = ColGroundEdge };
    private static readonly SKPaint HillPaint = new()
    {
        Color = new SKColor(0x2A, 0x20, 0x12),
        IsAntialias = true,
    };
    private static readonly SKPath HillPath;

    // ── Text sprites ──────────────────────────────────────────────────────
    private readonly HudLabel _title = new()
    {
        Text = "CASTLE ATTACK",
        FontSize = 68f,
        Color = ColGold,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 190f,
    };
    private readonly HudLabel _subtitle = new()
    {
        Text = "Defend the castle until the keep is complete!",
        FontSize = 22f,
        Color = ColHud,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 258f,
    };
    private readonly HudLabel _tapLine = new()
    {
        Text = "Tap the battlefield to aim & fire",
        FontSize = 17f,
        Color = ColAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 308f,
    };
    private readonly HudLabel _btnLine = new()
    {
        Text = "Use the on-screen buttons at the bottom for all actions",
        FontSize = 16f,
        Color = ColDim,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 334f,
    };
    private readonly HudLabel _kbLine = new()
    {
        Text = "Keyboard: LEFT RIGHT aim  |  SPACE fire  |  UP DN convert  |  Z X C weapons",
        FontSize = 14f,
        Color = ColDim,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 360f,
    };
    private readonly HudLabel _startLine = new()
    {
        Text = "Tap or Click to Start",
        FontSize = 24f,
        Color = ColAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 420f,
    };

    static StartScreen()
    {
        var skyShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, GroundY),
            [ColSky, ColHorizon],
            [0f, 1f],
            SKShaderTileMode.Clamp
        );
        SkyPaint.Shader = skyShader;

        HillPath = new SKPath();
        HillPath.MoveTo(0, GroundY);
        HillPath.CubicTo(200, GroundY - 60, 400, GroundY - 80, 600, GroundY - 40);
        HillPath.CubicTo(800, GroundY, 900, GroundY - 50, 1100, GroundY - 30);
        HillPath.LineTo(GameWidth, GroundY);
        HillPath.LineTo(0, GroundY);
        HillPath.Close();
    }

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_title);
            Children.Add(_subtitle);
            Children.Add(_tapLine);
            Children.Add(_btnLine);
            Children.Add(_kbLine);
            Children.Add(_startLine);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(ColSky);

        DrawBackground(canvas);

        OverlayPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.6f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), OverlayPaint);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }

    private static void DrawBackground(SKCanvas canvas)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GroundY), SkyPaint);
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, GameHeight - GroundY), GroundPaint);
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, 4f), GroundEdgePaint);
        canvas.DrawPath(HillPath, HillPaint);
    }
}