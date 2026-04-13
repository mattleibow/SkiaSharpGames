using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Manages the active screen and optional overlay stack for a game.
/// This class itself extends <see cref="GameScreenBase"/> so it can be passed directly
/// to the host (e.g. <c>GameView.razor</c>) without any host changes.
/// </summary>
/// <remarks>
/// <para>
/// Build a <see cref="ScreenCoordinator"/> via <see cref="ScreenCoordinator(IServiceProvider, GameScreenBase)"/>.
/// The <paramref name="services"/> container is used to resolve new screens when
/// <see cref="TransitionTo{TScreen}"/> or <see cref="PushOverlay{TOverlay}"/> are called.
/// </para>
/// <para>
/// Typical DI setup (in a game factory method):
/// <code>
/// var services = new ServiceCollection();
/// services.AddTransient&lt;StartScreen&gt;();
/// services.AddTransient&lt;PlayScreen&gt;();
/// var provider = services.BuildServiceProvider();
/// var initialScreen = provider.GetRequiredService&lt;StartScreen&gt;();
/// return new ScreenCoordinator(provider, initialScreen);
/// </code>
/// </para>
/// </remarks>
public sealed class ScreenCoordinator : GameScreenBase, IScreenCoordinator
{
    private readonly IServiceProvider _services;
    private GameScreenBase _current;
    private readonly List<GameScreenBase> _overlays = [];
    private ActiveTransition? _activeTransition;

    private sealed class ActiveTransition(
        GameScreenBase outgoing,
        GameScreenBase incoming,
        IScreenTransition transition)
    {
        public GameScreenBase Outgoing { get; } = outgoing;
        public GameScreenBase Incoming { get; } = incoming;
        public IScreenTransition Transition { get; } = transition;
        public float Progress { get; set; }
    }

    /// <summary>
    /// Creates a coordinator with <paramref name="initialScreen"/> as the first active screen.
    /// </summary>
    /// <param name="services">
    /// DI container used to resolve screen types when transitioning.
    /// </param>
    /// <param name="initialScreen">The screen to show immediately (already resolved from DI or constructed manually).</param>
    public ScreenCoordinator(IServiceProvider services, GameScreenBase initialScreen)
    {
        _services = services;
        _current  = initialScreen;
        _current.SetCoordinator(this);
        _current.OnActivated();
    }

    /// <inheritdoc />
    /// <remarks>Delegates to the incoming screen during a transition, otherwise to the current screen.</remarks>
    public override (int width, int height) GameDimensions =>
        _activeTransition?.Incoming.GameDimensions ?? _current.GameDimensions;

    // ── IScreenCoordinator ────────────────────────────────────────────────

    /// <inheritdoc />
    public void TransitionTo<TScreen>(IScreenTransition? transition = null)
        where TScreen : GameScreenBase
    {
        // Cancel any running transition immediately
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
            _current         = incoming;
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

    /// <inheritdoc />
    public void PushOverlay<TOverlay>() where TOverlay : GameScreenBase
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

    /// <inheritdoc />
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

    // ── GameScreenBase ────────────────────────────────────────────────────

    /// <inheritdoc />
    public override void Update(float deltaTime)
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

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, int width, int height)
    {
        if (_activeTransition is not null)
        {
            float progress = Math.Clamp(_activeTransition.Progress, 0f, 1f);
            _activeTransition.Transition.Draw(
                canvas, progress,
                c => _activeTransition.Outgoing.Draw(c, width, height),
                c => _activeTransition.Incoming.Draw(c, width, height),
                width, height);
            return;
        }

        _current.Draw(canvas, width, height);
        foreach (var overlay in _overlays)
            overlay.Draw(canvas, width, height);
    }

    /// <inheritdoc />
    public override void OnPointerMove(float x, float y) => ActiveInputScreen.OnPointerMove(x, y);

    /// <inheritdoc />
    public override void OnPointerDown(float x, float y) => ActiveInputScreen.OnPointerDown(x, y);

    /// <inheritdoc />
    public override void OnPointerUp(float x, float y) => ActiveInputScreen.OnPointerUp(x, y);

    /// <inheritdoc />
    public override void OnKeyDown(string key) => ActiveInputScreen.OnKeyDown(key);

    /// <inheritdoc />
    public override void OnKeyUp(string key) => ActiveInputScreen.OnKeyUp(key);

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// The screen that should receive input events.
    /// During a transition the incoming screen receives input.
    /// Otherwise, the topmost overlay (or the base screen if none) receives input.
    /// </summary>
    private GameScreenBase ActiveInputScreen =>
        _activeTransition is not null ? _activeTransition.Incoming :
        _overlays.Count   > 0         ? _overlays[^1]               :
                                        _current;

    private T ResolveScreen<T>() where T : GameScreenBase
    {
        var screen = _services.GetRequiredService<T>();
        screen.SetCoordinator(this);
        return screen;
    }
}
