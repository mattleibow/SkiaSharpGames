using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>Title screen with atmospheric fog preview. Click or press Space to start.</summary>
internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new()
    {
        Text = "GHOST LIGHT",
        FontSize = 64f,
        Color = new SKColor(0x88, 0xCC, 0xFF),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 260f,
    };
    private readonly HudLabel _subtitle = new()
    {
        Text = "Navigate the darkness. Survive the shadows.",
        FontSize = 22f,
        Color = new SKColor(0x66, 0x88, 0xAA),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 310f,
    };
    private readonly HudLabel _startPrompt = new()
    {
        Text = "Click or Tap to Start",
        FontSize = 28f,
        Color = new SKColor(0x66, 0xBB, 0xFF),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 380f,
    };
    private readonly HudLabel _instructions = new()
    {
        Text = "Arrow keys / WASD to move",
        FontSize = 18f,
        Color = new SKColor(0x55, 0x66, 0x77),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 430f,
    };

    // Decorative fog layers for atmosphere
    private readonly Actor _darkness = new() { Name = "darkness", Alpha = 0.5f };
    private readonly FogLayer _fog = new(0.3f) { Name = "titleFog" };

    // Decorative spirit preview
    private readonly Spirit _previewSpirit = new()
    {
        X = GameWidth / 2f,
        Y = 160f,
        Alpha = 0.8f,
    };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            _fog.Children.Add(_previewSpirit);
            _darkness.Children.Add(_fog);
            Children.Add(_darkness);
            Children.Add(_title);
            Children.Add(_subtitle);
            Children.Add(_startPrompt);
            Children.Add(_instructions);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(new SKColor(0x08, 0x06, 0x12));
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new FadeCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new FadeCurtain());
    }
}
