namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A periodically repeating animation.
/// After an initial countdown of <see cref="Period"/> seconds the animation runs for
/// <see cref="Duration"/> seconds, then waits again.
/// The <see cref="Progress"/> property (0 → 1) describes how far through the current run the
/// animation is; it is zero while waiting.
/// </summary>
/// <remarks>
/// Call <see cref="Start"/> (optionally with an <c>initialDelay</c>) to begin,
/// then call <see cref="Update"/> every game tick.
/// </remarks>
/// <example>
/// <code>
/// // 1-second shimmer that fires every 8 seconds
/// var shimmer = new LoopedAnimation(period: 8f, duration: 1f);
/// shimmer.Start(initialDelay: Random.Shared.NextSingle() * 8f); // stagger instances
///
/// // In Update():
/// shimmer.Update(deltaTime);
///
/// // In Draw():
/// if (shimmer.IsActive)
///     DrawShimmerAtProgress(shimmer.Progress);
/// </code>
/// </example>
public sealed class LoopedAnimation
{
    private float _sinceLastRun;
    private float _runElapsed;
    private int   _completedRuns;

    /// <param name="period">Seconds between the start of successive runs.</param>
    /// <param name="duration">Seconds each run lasts.</param>
    public LoopedAnimation(float period, float duration)
    {
        Period   = period;
        Duration = duration;
    }

    /// <summary>Seconds between the start of successive animation runs.</summary>
    public float Period { get; set; }

    /// <summary>Seconds each animation run lasts.</summary>
    public float Duration { get; set; }

    /// <summary>
    /// Number of times to repeat. Use <c>-1</c> (default) for infinite repetitions.
    /// </summary>
    public int RepeatCount { get; set; } = -1;

    /// <summary>
    /// Normalised progress within the current run (0 = start, 1 = end).
    /// Zero while not active.
    /// </summary>
    public float Progress { get; private set; }

    /// <summary>True while the animation is currently running.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Whether this animation is enabled and will fire.</summary>
    public bool Enabled { get; private set; }

    /// <summary>
    /// Enables the animation.
    /// </summary>
    /// <param name="initialDelay">
    /// How long to wait before the first run, in seconds.
    /// Defaults to 0 (fires after one full <see cref="Period"/>).
    /// Pass a fraction of <see cref="Period"/> to stagger multiple instances.
    /// </param>
    public void Start(float initialDelay = 0f)
    {
        Enabled        = true;
        IsActive       = false;
        Progress       = 0f;
        _completedRuns = 0;
        _runElapsed    = 0f;
        // _sinceLastRun reaches Period to trigger a run; initialDelay controls how long until then.
        _sinceLastRun = Math.Clamp(Period - initialDelay, 0f, Period);
    }

    /// <summary>Pauses and resets the animation.</summary>
    public void Stop()
    {
        Enabled       = false;
        IsActive      = false;
        Progress      = 0f;
        _sinceLastRun = 0f;
        _runElapsed   = 0f;
    }

    /// <summary>Advances the animation by <paramref name="deltaTime"/> seconds.</summary>
    public void Update(float deltaTime)
    {
        if (!Enabled) return;

        if (IsActive)
        {
            _runElapsed += deltaTime;
            if (_runElapsed >= Duration)
            {
                Progress  = 1f;
                IsActive  = false;
                _completedRuns++;
                _sinceLastRun = 0f;
                _runElapsed   = 0f;

                if (RepeatCount >= 0 && _completedRuns >= RepeatCount)
                    Stop();
            }
            else
            {
                Progress = _runElapsed / Duration;
            }
        }
        else
        {
            _sinceLastRun += deltaTime;
            if (_sinceLastRun >= Period)
            {
                _sinceLastRun -= Period;
                _runElapsed   = 0f;
                Progress      = 0f;
                IsActive      = true;
            }
        }
    }
}
