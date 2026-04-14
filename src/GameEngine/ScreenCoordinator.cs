using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Default implementation of <see cref="IScreenCoordinator"/>.
/// Manages the active screen, overlay stack, and running transitions.
/// Registered as a singleton in the game's own DI container by <see cref="GameBuilder.Build"/>.
/// </summary>
internal sealed class ScreenCoordinator : IScreenCoordinator
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

    internal ScreenCoordinator(IServiceProvider services, IOptions<GameOptions> options)
    {
        _services = services;
        _current  = (GameScreen)services.GetRequiredService(options.Value.InitialScreenType!);
        _current.SetCoordinator(this);
        _current.OnActivated();
    }

    // ── IScreenCoordinator ────────────────────────────────────────────────

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    // ── Internal API (used by Game) ───────────────────────────────────────

    /// <summary>Advances the screen stack by <paramref name="deltaTime"/> seconds.</summary>
    internal void Update(float deltaTime)
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

    /// <summary>
    /// Draws the current screen, overlays, or active transition to
    /// <paramref name="canvas"/>. The canvas is assumed to already be in game-space
    /// coordinates (the fit-and-centre transform is applied by the caller).
    /// </summary>
    internal void DrawScreens(SKCanvas canvas, int width, int height)
    {
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
            _current.Draw(canvas, width, height);
            foreach (var overlay in _overlays)
                overlay.Draw(canvas, width, height);
        }
    }

    /// <summary>The screen that should receive input events.</summary>
    internal GameScreen ActiveInputScreen =>
        _activeTransition is not null ? _activeTransition.Incoming :
        _overlays.Count   > 0         ? _overlays[^1]              :
                                        _current;

    // ── Private helpers ───────────────────────────────────────────────────

    private TScreen ResolveScreen<TScreen>() where TScreen : GameScreen
    {
        var screen = _services.GetRequiredService<TScreen>();
        screen.SetCoordinator(this);
        return screen;
    }
}
