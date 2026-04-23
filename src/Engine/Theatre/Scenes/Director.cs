using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SkiaSharp.Theatre;

/// <summary>
/// Default implementation of <see cref="IDirector"/> and <see cref="IRenderer"/>.
/// Manages the active scene, scene stack, and running transitions.
/// Registered as a singleton in the game's own DI container by <see cref="StageBuilder.Open"/>.
/// </summary>
internal sealed class Director : IDirector, IRenderer
{
    private readonly IServiceProvider _services;
    private readonly Type _initialSceneType;
    private Stage? _stage;
    private Scene? _current;
    private readonly List<Scene> _sceneStack = [];
    private ActiveCurtain? _activeCurtain;

    private sealed class ActiveCurtain(
        Scene outgoing,
        Scene incoming,
        ICurtain curtain)
    {
        public Scene Outgoing { get; } = outgoing;
        public Scene Incoming { get; } = incoming;
        public ICurtain Curtain { get; } = curtain;
        public float Progress { get; set; }
    }

    internal Director(IServiceProvider services, IOptions<StageOptions> options)
    {
        _services = services;
        _initialSceneType = options.Value.OpeningSceneType!;
    }

    internal void Initialize(Stage stage)
    {
        _stage = stage;
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (_current is not null)
            return;

        _current = (Scene)_services.GetRequiredService(_initialSceneType);
        _current.Stage = _stage;
        _current.OnActivating();
        _current.OnActivated();
    }

    // ── IDirector ────────────────────────────────────────────────

    /// <inheritdoc/>
    public void TransitionTo<TScene>(ICurtain? transition = null)
        where TScene : Scene
    {
        EnsureInitialized();

        // Cancel any running transition: both the outgoing (already deactivating) and the
        // incoming (was activating but never completed) are now fully removed.
        if (_activeCurtain != null)
        {
            _activeCurtain.Outgoing.OnDeactivated();
            _activeCurtain.Incoming.OnDeactivating();
            _activeCurtain.Incoming.OnDeactivated();
            _activeCurtain = null;
        }

        // Clear scene stack
        foreach (var layer in _sceneStack)
        {
            layer.OnDeactivating();
            layer.OnDeactivated();
        }
        _sceneStack.Clear();

        var incoming = _services.GetRequiredService<TScene>();

        if (transition is null || transition.Duration <= 0f)
        {
            _current!.OnDeactivating();
            _current.OnDeactivated();
            _current.Stage = null;
            _current = incoming;
            _current.Stage = _stage;
            _current.IsPaused = false;
            _current.OnActivating();
            _current.OnActivated();
        }
        else
        {
            _current!.IsPaused = true;
            _current.OnDeactivating();
            incoming.Stage = _stage;
            incoming.OnActivating();
            _activeCurtain = new ActiveCurtain(_current, incoming, transition);
        }
    }

    /// <inheritdoc/>
    public void PushScene<TScene>()
        where TScene : Scene
    {
        EnsureInitialized();

        if (_sceneStack.Count == 0)
        {
            _current!.IsPaused = true;
            _current.OnPaused();
        }

        var layer = _services.GetRequiredService<TScene>();
        layer.Stage = _stage;
        layer.OnActivating();
        layer.OnActivated();
        _sceneStack.Add(layer);
    }

    /// <inheritdoc/>
    public void PopScene()
    {
        if (_sceneStack.Count == 0)
            return;

        _sceneStack[^1].OnDeactivating();
        _sceneStack[^1].OnDeactivated();
        _sceneStack.RemoveAt(_sceneStack.Count - 1);

        if (_sceneStack.Count == 0)
        {
            _current!.IsPaused = false;
            _current.OnResumed();
        }
    }

    /// <inheritdoc/>
    public Scene ActiveInputScene
    {
        get
        {
            EnsureInitialized();
            return _activeCurtain is not null ? _activeCurtain.Incoming :
                   _sceneStack.Count > 0 ? _sceneStack[^1] :
                                         _current!;
        }
    }

    // ── IRenderer ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Update(float deltaTime)
    {
        EnsureInitialized();

        if (_activeCurtain is not null)
        {
            _activeCurtain.Progress +=
                deltaTime / _activeCurtain.Curtain.Duration;

            if (_activeCurtain.Progress >= 1f)
            {
                var outgoing = _activeCurtain.Outgoing;
                var incoming = _activeCurtain.Incoming;
                _activeCurtain = null;
                _current = incoming;
                _current.IsPaused = false;
                outgoing.Stage = null;
                outgoing.OnDeactivated();
                incoming.OnActivated();
                // Fall through so the new scene gets its first update on this frame
            }
            else
            {
                return;
            }
        }

        if (_sceneStack.Count > 0)
            _sceneStack[^1].Update(deltaTime);
        else
            _current!.Update(deltaTime);
    }

    /// <inheritdoc/>
    public void Draw(SKCanvas canvas)
    {
        EnsureInitialized();

        if (_activeCurtain is not null)
        {
            float progress = Math.Clamp(_activeCurtain.Progress, 0f, 1f);
            _activeCurtain.Curtain.Draw(
                canvas, progress,
                c => _activeCurtain.Outgoing.Draw(c),
                c => _activeCurtain.Incoming.Draw(c),
                (int)(_stage!.StageSize.Width), (int)(_stage!.StageSize.Height));
        }
        else
        {
            _current!.Draw(canvas);
            foreach (var layer in _sceneStack)
                layer.Draw(canvas);
        }
    }
}
