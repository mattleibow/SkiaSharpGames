using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// The root of a running game. Produced by <see cref="GameBuilder.Build"/>; cannot be
/// subclassed or instantiated directly.
/// </summary>
/// <remarks>
/// <para>
/// Assign the instance returned by <see cref="GameBuilder.Build"/> to a view component
/// (e.g. <c>GameView.razor</c>). The game owns its own isolated DI container, screen stack,
/// and transition engine.
/// </para>
/// <example>
/// <code>
/// var builder = GameBuilder.CreateDefault();
/// builder.Services.AddSingleton&lt;BreakoutGameState&gt;();
/// builder.Screens
///        .Add&lt;BreakoutStartScreen&gt;()
///        .Add&lt;BreakoutPlayScreen&gt;()
///        .Add&lt;BreakoutGameOverScreen&gt;()
///        .Add&lt;BreakoutVictoryScreen&gt;();
/// builder.SetInitialScreen&lt;BreakoutStartScreen&gt;();
/// builder.SetGameDimensions(new SKSize(800, 600));
/// var game = builder.Build();
/// </code>
/// </example>
/// </remarks>
public sealed class Game
{
    private readonly IServiceProvider _services;
    private GameScreen _current;
    private readonly List<GameScreen> _overlays = [];
    private ActiveTransition? _activeTransition;

    private sealed class ActiveTransition(
        GameScreen outgoing,
        GameScreen incoming,
        IScreenTransition transition)
    {
        public GameScreen        Outgoing   { get; } = outgoing;
        public GameScreen        Incoming   { get; } = incoming;
        public IScreenTransition Transition { get; } = transition;
        public float             Progress   { get; set; }
    }

    internal Game(IServiceProvider services, Type initialScreenType, SKSize gameDimensions)
    {
        _services      = services;
        GameDimensions = gameDimensions;
        _current       = (GameScreen)services.GetRequiredService(initialScreenType);
        _current.SetGame(this);
        _current.OnActivated();
    }

    // ── Screen navigation (called by screens via their Game property) ──────

    /// <summary>
    /// Replaces the current screen with a new <typeparamref name="TScreen"/> instance,
    /// optionally playing a cross-screen transition. Clears any open overlays first.
    /// </summary>
    public void TransitionTo<TScreen>(IScreenTransition? transition = null)
        where TScreen : GameScreen
    {
        // Cancel any running transition
        if (_activeTransition != null)
        {
            _activeTransition.Outgoing.OnDeactivated();
            _activeTransition = null;
        }

        // Clear overlay stack
        foreach (var overlay in _overlays)
            overlay.OnDeactivated();
        _overlays.Clear();

        var incoming = ResolveScreen<TScreen>();

        if (transition is null)
        {
            _current.OnDeactivated();
            _current          = incoming;
            _current.IsPaused = false;
            _current.OnActivated();
        }
        else
        {
            _current.IsPaused = true;
            incoming.OnActivated();
            _activeTransition = new ActiveTransition(_current, incoming, transition);
        }
    }

    /// <summary>
    /// Pushes an overlay screen on top of the current screen.
    /// The current screen is paused (still drawn, not updated) while any overlay is active.
    /// </summary>
    public void PushOverlay<TOverlay>() where TOverlay : GameScreen
    {
        if (_overlays.Count == 0)
        {
            _current.IsPaused = true;
            _current.OnPaused();
        }

        var overlay = ResolveScreen<TOverlay>();
        overlay.OnActivated();
        _overlays.Add(overlay);
    }

    /// <summary>
    /// Removes the topmost overlay and resumes the underlying screen.
    /// Does nothing if no overlay is currently active.
    /// </summary>
    public void PopOverlay()
    {
        if (_overlays.Count == 0)
            return;

        _overlays[^1].OnDeactivated();
        _overlays.RemoveAt(_overlays.Count - 1);

        if (_overlays.Count == 0)
        {
            _current.IsPaused = false;
            _current.OnResumed();
        }
    }

    // ── Host API (called by GameView or any rendering host) ───────────────

    /// <summary>
    /// Logical (virtual) dimensions of the game canvas in game-space units.
    /// Set via <see cref="GameBuilder.SetGameDimensions"/> before calling
    /// <see cref="GameBuilder.Build"/>. All screens in the game share this value.
    /// </summary>
    public SKSize GameDimensions { get; }

    /// <summary>Advances the game by <paramref name="deltaTime"/> seconds.</summary>
    public void Update(float deltaTime)
    {
        if (_activeTransition is not null)
        {
            _activeTransition.Progress +=
                deltaTime / _activeTransition.Transition.Duration;

            if (_activeTransition.Progress >= 1f)
            {
                var incoming = _activeTransition.Incoming;
                _activeTransition.Outgoing.OnDeactivated();
                _activeTransition = null;
                _current          = incoming;
                _current.IsPaused = false;
            }
            return;
        }

        if (_overlays.Count > 0)
            _overlays[^1].Update(deltaTime);
        else
            _current.Update(deltaTime);
    }

    /// <summary>Draws the current frame to <paramref name="canvas"/>.</summary>
    /// <remarks>
    /// Computes the fit-and-centre transform from pixel space to game space once, then
    /// calls each screen's <see cref="GameScreen.Draw"/> with a pre-transformed canvas.
    /// Screens always receive a canvas in game-space coordinates — they never need to
    /// compute or apply their own scale/offset.
    /// </remarks>
    public void Draw(SKCanvas canvas, int width, int height)
    {
        float gw    = GameDimensions.Width;
        float gh    = GameDimensions.Height;
        float scale   = MathF.Min(width / gw, height / gh);
        float offsetX = (width  - gw * scale) / 2f;
        float offsetY = (height - gh * scale) / 2f;

        canvas.Save();
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale, scale);

        int igw = (int)gw;
        int igh = (int)gh;

        if (_activeTransition is not null)
        {
            float progress = Math.Clamp(_activeTransition.Progress, 0f, 1f);
            _activeTransition.Transition.Draw(
                canvas, progress,
                c => _activeTransition.Outgoing.Draw(c, igw, igh),
                c => _activeTransition.Incoming.Draw(c, igw, igh),
                igw, igh);
        }
        else
        {
            _current.Draw(canvas, igw, igh);
            foreach (var overlay in _overlays)
                overlay.Draw(canvas, igw, igh);
        }

        canvas.Restore();
    }

    /// <summary>Called when the pointer/mouse moves over the canvas.</summary>
    public void OnPointerMove(float x, float y) => ActiveInputScreen.OnPointerMove(x, y);

    /// <summary>Called when the user clicks or taps the canvas.</summary>
    public void OnPointerDown(float x, float y) => ActiveInputScreen.OnPointerDown(x, y);

    /// <summary>Called when the user releases a click or touch on the canvas.</summary>
    public void OnPointerUp(float x, float y) => ActiveInputScreen.OnPointerUp(x, y);

    /// <summary>Called when a key is pressed while the game canvas has focus.</summary>
    public void OnKeyDown(string key) => ActiveInputScreen.OnKeyDown(key);

    /// <summary>Called when a key is released while the game canvas has focus.</summary>
    public void OnKeyUp(string key) => ActiveInputScreen.OnKeyUp(key);

    // ── Private helpers ───────────────────────────────────────────────────

    private GameScreen ActiveInputScreen =>
        _activeTransition is not null ? _activeTransition.Incoming :
        _overlays.Count   > 0         ? _overlays[^1]              :
                                        _current;

    private TScreen ResolveScreen<TScreen>() where TScreen : GameScreen
    {
        var screen = _services.GetRequiredService<TScreen>();
        screen.SetGame(this);
        return screen;
    }
}
