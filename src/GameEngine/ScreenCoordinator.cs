using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Default implementation of <see cref="IScreenCoordinator"/> and <see cref="IScreenDrawable"/>.
/// Manages the active screen, overlay stack, and running transitions.
/// Registered as a singleton in the game's own DI container by <see cref="GameBuilder.Build"/>.
/// </summary>
internal sealed class ScreenCoordinator : IScreenCoordinator, IScreenDrawable
{
    private readonly IServiceProvider _services;
    private readonly Type _initialScreenType;
    private GameScreen? _current;
    private readonly List<GameScreen> _overlays = [];
    private ActiveTransition? _activeTransition;

    private sealed class ActiveTransition(
        GameScreen outgoing,
        GameScreen incoming,
        IScreenTransition transition)
    {
        public GameScreen Outgoing { get; } = outgoing;
        public GameScreen Incoming { get; } = incoming;
        public IScreenTransition Transition { get; } = transition;
        public float Progress { get; set; }
    }

    internal ScreenCoordinator(IServiceProvider services, IOptions<GameOptions> options)
    {
        _services = services;
        _initialScreenType = options.Value.InitialScreenType!;
    }

    private void EnsureInitialized()
    {
        if (_current is not null)
            return;

        _current = (GameScreen)_services.GetRequiredService(_initialScreenType);
        _current.OnActivated();
    }

    // ── IScreenCoordinator ────────────────────────────────────────────────

    /// <inheritdoc/>
    public void TransitionTo<TScreen>(IScreenTransition? transition = null)
        where TScreen : GameScreen
    {
        EnsureInitialized();

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
            _current!.OnDeactivated();
            _current = incoming;
            _current.IsPaused = false;
            _current.OnActivated();
        }
        else
        {
            _current!.IsPaused = true;
            incoming.OnActivated();
            _activeTransition = new ActiveTransition(_current, incoming, transition);
        }
    }

    /// <inheritdoc/>
    public void PushOverlay<TOverlay>() where TOverlay : GameScreen
    {
        EnsureInitialized();

        if (_overlays.Count == 0)
        {
            _current!.IsPaused = true;
            _current.OnPaused();
        }

        var overlay = ResolveScreen<TOverlay>();
        overlay.OnActivated();
        _overlays.Add(overlay);
    }

    /// <inheritdoc/>
    public void PopOverlay()
    {
        if (_overlays.Count == 0)
            return;

        _overlays[^1].OnDeactivated();
        _overlays.RemoveAt(_overlays.Count - 1);

        if (_overlays.Count == 0)
        {
            _current!.IsPaused = false;
            _current.OnResumed();
        }
    }

    /// <inheritdoc/>
    public GameScreen ActiveInputScreen
    {
        get
        {
            EnsureInitialized();
            return _activeTransition is not null ? _activeTransition.Incoming :
                   _overlays.Count > 0 ? _overlays[^1] :
                                         _current!;
        }
    }

    // ── IScreenDrawable ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Update(float deltaTime)
    {
        EnsureInitialized();

        if (_activeTransition is not null)
        {
            _activeTransition.Progress +=
                deltaTime / _activeTransition.Transition.Duration;

            if (_activeTransition.Progress >= 1f)
            {
                var incoming = _activeTransition.Incoming;
                _activeTransition.Outgoing.OnDeactivated();
                _activeTransition = null;
                _current = incoming;
                _current.IsPaused = false;
            }
            return;
        }

        if (_overlays.Count > 0)
            _overlays[^1].Update(deltaTime);
        else
            _current!.Update(deltaTime);
    }

    /// <inheritdoc/>
    public void DrawScreens(SKCanvas canvas, int width, int height)
    {
        EnsureInitialized();

        if (_activeTransition is not null)
        {
            float progress = Math.Clamp(_activeTransition.Progress, 0f, 1f);
            _activeTransition.Transition.Draw(
                canvas, progress,
                c => _activeTransition.Outgoing.Draw(c, width, height),
                c => _activeTransition.Incoming.Draw(c, width, height),
                width, height);
        }
        else
        {
            _current!.Draw(canvas, width, height);
            foreach (var overlay in _overlays)
                overlay.Draw(canvas, width, height);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private TScreen ResolveScreen<TScreen>() where TScreen : GameScreen =>
        _services.GetRequiredService<TScreen>();
}
