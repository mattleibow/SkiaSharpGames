namespace SkiaSharp.Theatre;

/// <summary>
/// A simple countdown timer that ticks down from a set duration and fires once on expiry.
/// </summary>
/// <remarks>
/// Typical usage:
/// <code>
/// CountdownTimer _cooldown;
///
/// // Start it:
/// _cooldown.Set(1.5f);
///
/// // In Update():
/// if (_cooldown.Tick(deltaTime))
///     OnCooldownExpired(); // called exactly once when it hits zero
///
/// // Read remaining time for HUD:
/// float ratio = _cooldown.Remaining / TotalDuration;
/// </code>
/// </remarks>
public struct CountdownTimer
{
    private float _remaining;

    /// <summary>True while the timer is counting down.</summary>
    public readonly bool Active => _remaining > 0f;

    /// <summary>Remaining time in seconds. Zero when not active.</summary>
    public readonly float Remaining => _remaining;

    /// <summary>Starts (or restarts) the countdown from <paramref name="duration"/> seconds.</summary>
    public void Set(float duration) => _remaining = MathF.Max(0f, duration);

    /// <summary>Cancels the timer without triggering an expiry callback.</summary>
    public void Reset() => _remaining = 0f;

    /// <summary>
    /// Advances the timer by <paramref name="deltaTime"/> seconds.
    /// Returns <see langword="true"/> on the single tick the timer transitions from active to zero.
    /// </summary>
    public bool Tick(float deltaTime)
    {
        if (_remaining <= 0f) return false;
        _remaining -= deltaTime;
        if (_remaining <= 0f) { _remaining = 0f; return true; }
        return false;
    }
}
