using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using SkiaSharp.Theatre.Diagnostics;

namespace SkiaSharp.Theatre;

/// <summary>
/// The root of a running game. Produced by <see cref="StageBuilder.Open"/>; cannot be
/// subclassed or instantiated directly.
/// </summary>
/// <remarks>
/// <para>
/// Assign the instance returned by <see cref="StageBuilder.Open"/> to a view component
/// (e.g. <c>GameView.razor</c>). The game owns its own isolated DI container, scene stack,
/// and transition engine.
/// </para>
/// <example>
/// <code>
/// var builder = StageBuilder.Create();
/// builder.Services.AddSingleton&lt;BreakoutGameState&gt;();
/// builder.Scenes
///        .Add&lt;BreakoutStartScreen&gt;()
///        .Add&lt;BreakoutPlayScreen&gt;()
///        .Add&lt;BreakoutGameOverScreen&gt;()
///        .Add&lt;BreakoutVictoryScreen&gt;();
/// builder.SetOpeningScene&lt;BreakoutStartScreen&gt;();
/// builder.SetStageSize(new SKSize(800, 600));
/// var stage = builder.Open();
/// </code>
/// </example>
/// </remarks>
public sealed class Stage
{
    private readonly IDirector _director;
    private readonly IRenderer _renderer;

    internal Stage(
        IServiceProvider services,
        IOptions<StageOptions> options,
        IDirector director,
        IRenderer renderer
    )
    {
        Services = services;
        StageSize = options.Value.Dimensions;
        _director = director;
        _renderer = renderer;
    }

    // ── Host API (called by GameView or any rendering host) ───────────────

    /// <summary>
    /// The game's isolated DI container. Use this to resolve any service registered via
    /// <see cref="StageBuilder.Services"/>, including <see cref="IDirector"/>.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Logical (virtual) dimensions of the game canvas in game-space units.
    /// Set via <see cref="StageBuilder.SetStageSize"/> before calling
    /// <see cref="StageBuilder.Open"/>. All scenes in the game share this value.
    /// </summary>
    public SKSize StageSize { get; }

    /// <summary>
    /// Game-wide HUD theme. Acts as the final fallback when resolving themes for scenes
    /// and nodes. Can be changed at runtime to swap the entire game's theme.
    /// </summary>
    public HudTheme? HudTheme { get; set; }

    /// <summary>
    /// Controls whether the pointer is drawn for scenes that don't override
    /// <see cref="Scene.ShowPointer"/>. Defaults to <c>true</c>.
    /// </summary>
    public bool ShowPointer { get; set; } = true;

    /// <summary>Advances the game by <paramref name="deltaTime"/> seconds.</summary>
    public void Update(float deltaTime) => _renderer.Update(deltaTime);

    /// <summary>Draws the current frame to <paramref name="canvas"/>.</summary>
    /// <remarks>
    /// Computes the fit-and-centre transform from pixel space to game space once, then
    /// delegates to <see cref="IRenderer.Draw"/> with a pre-transformed canvas.
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

        var viewport = SKMatrix44
            .CreateTranslation(offsetX, offsetY, 0f)
            .PostConcat(SKMatrix44.CreateScale(scale, scale, 1f));

        canvas.Save();
        canvas.Concat(viewport);

        _renderer.Draw(canvas);

        // Inspector overlay (drawn in game-space on top of the scene)
        var inspector = Services.GetService<IStageInspector>();
        if (inspector?.IsEnabled == true)
            inspector.DrawOverlay(canvas, _director.ActiveInputScene);

        canvas.Restore();
    }

    /// <summary>Called when the pointer/mouse moves over the canvas.</summary>
    public void OnPointerMove(float x, float y)
    {
        var scene = _director.ActiveInputScene;
        var p = scene.Pointer;
        p.X = x;
        p.Y = y;
        p.Visible = true;
        scene.OnPointerMove(x, y);
    }

    /// <summary>Called when the user clicks or taps the canvas.</summary>
    public void OnPointerDown(float x, float y)
    {
        var scene = _director.ActiveInputScene;
        var p = scene.Pointer;
        p.IsDown = true;
        p.X = x;
        p.Y = y;
        p.Visible = true;
        scene.OnPointerDown(x, y);
    }

    /// <summary>Called when the user releases a click or touch on the canvas.</summary>
    public void OnPointerUp(float x, float y)
    {
        var scene = _director.ActiveInputScene;
        var p = scene.Pointer;
        p.X = x;
        p.Y = y;
        p.IsDown = false;
        scene.OnPointerUp(x, y);
    }

    /// <summary>Called when a key is pressed while the game canvas has focus.</summary>
    public void OnKeyDown(string key) => _director.ActiveInputScene.OnKeyDown(key);

    /// <summary>Called when a key is released while the game canvas has focus.</summary>
    public void OnKeyUp(string key) => _director.ActiveInputScene.OnKeyUp(key);
}
