using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// The root of a running game. Produced by <see cref="Theatre.Open"/>; cannot be
/// subclassed or instantiated directly.
/// </summary>
/// <remarks>
/// <para>
/// Assign the instance returned by <see cref="Theatre.Open"/> to a view component
/// (e.g. <c>GameView.razor</c>). The game owns its own isolated DI container, screen stack,
/// and transition engine.
/// </para>
/// <example>
/// <code>
/// var builder = Theatre.Create();
/// builder.Services.AddSingleton&lt;BreakoutGameState&gt;();
/// builder.Scenes
///        .Add&lt;BreakoutStartScreen&gt;()
///        .Add&lt;BreakoutPlayScreen&gt;()
///        .Add&lt;BreakoutGameOverScreen&gt;()
///        .Add&lt;BreakoutVictoryScreen&gt;();
/// builder.SetOpeningScene&lt;BreakoutStartScreen&gt;();
/// builder.SetStageSize(new SKSize(800, 600));
/// var game = builder.Open();
/// </code>
/// </example>
/// </remarks>
public sealed class Stage
{
    private readonly IDirector _coordinator;
    private readonly IStageRenderer _drawable;

    internal Stage(IServiceProvider services, IOptions<StageOptions> options, IStageRenderer drawable)
    {
        Services = services;
        StageSize = options.Value.Dimensions;
        _coordinator = services.GetRequiredService<IDirector>();
        _drawable = drawable;
    }

    // ── Host API (called by GameView or any rendering host) ───────────────

    /// <summary>
    /// The game's isolated DI container. Use this to resolve any service registered via
    /// <see cref="Theatre.Services"/>, including <see cref="IDirector"/>.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Logical (virtual) dimensions of the game canvas in game-space units.
    /// Set via <see cref="Theatre.SetStageSize"/> before calling
    /// <see cref="Theatre.Open"/>. All screens in the game share this value.
    /// </summary>
    public SKSize StageSize { get; }

    /// <summary>Advances the game by <paramref name="deltaTime"/> seconds.</summary>
    public void Update(float deltaTime) => _drawable.Update(deltaTime);

    /// <summary>Draws the current frame to <paramref name="canvas"/>.</summary>
    /// <remarks>
    /// Computes the fit-and-centre transform from pixel space to game space once, then
    /// delegates to <see cref="IStageRenderer.Draw"/> with a pre-transformed canvas.
    /// Screens always receive a canvas in game-space coordinates — they never need to
    /// compute or apply their own scale/offset.
    /// </remarks>
    public void Draw(SKCanvas canvas, int width, int height)
    {
        float gw = StageSize.Width;
        float gh = StageSize.Height;
        float scale = MathF.Min(width / gw, height / gh);
        float offsetX = (width - gw * scale) / 2f;
        float offsetY = (height - gh * scale) / 2f;

        var viewport = SKMatrix44.CreateTranslation(offsetX, offsetY, 0f)
            .PostConcat(SKMatrix44.CreateScale(scale, scale, 1f));

        canvas.Save();
        canvas.Concat(viewport);

        _drawable.Draw(canvas, (int)gw, (int)gh);

        canvas.Restore();
    }

    /// <summary>Called when the pointer/mouse moves over the canvas.</summary>
    public void OnPointerMove(float x, float y)
    {
        var screen = _coordinator.ActiveInputScene;
        if (screen.Spotlight is { } p) { p.X = x; p.Y = y; p.Visible = true; }
        screen.OnPointerMove(x, y);
    }

    /// <summary>Called when the user clicks or taps the canvas.</summary>
    public void OnPointerDown(float x, float y)
    {
        var screen = _coordinator.ActiveInputScene;
        if (screen.Spotlight is { } p) { p.IsDown = true; p.X = x; p.Y = y; p.Visible = true; }
        screen.OnPointerDown(x, y);
    }

    /// <summary>Called when the user releases a click or touch on the canvas.</summary>
    public void OnPointerUp(float x, float y)
    {
        var screen = _coordinator.ActiveInputScene;
        if (screen.Spotlight is { } p) { p.X = x; p.Y = y; p.IsDown = false; }
        screen.OnPointerUp(x, y);
    }

    /// <summary>Called when a key is pressed while the game canvas has focus.</summary>
    public void OnKeyDown(string key) => _coordinator.ActiveInputScene.OnKeyDown(key);

    /// <summary>Called when a key is released while the game canvas has focus.</summary>
    public void OnKeyUp(string key) => _coordinator.ActiveInputScene.OnKeyUp(key);
}

