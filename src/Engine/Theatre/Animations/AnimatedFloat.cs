namespace SkiaSharp.Theatre;

/// <summary>
/// A <see langword="float"/> value that smoothly animates from its current value
/// to a target over a given duration using a configurable <see cref="Easing"/> function.
/// </summary>
/// <remarks>
/// Call <see cref="Update"/> every game tick to advance the animation.
/// The <see cref="Value"/> property always reflects the current interpolated value.
/// </remarks>
/// <example>
/// <code>
/// // Create with an initial value
/// var paddleWidth = new AnimatedFloat(100f);
///
/// // Animate to a new value over 0.3 s with a spring-like overshoot
/// paddleWidth.AnimateTo(180f, 0.3f, Easing.BackOut);
///
/// // In Update():
/// paddleWidth.Update(deltaTime);
///
/// // Use the value:
/// float w = paddleWidth.Value;
/// </code>
/// </example>
public sealed class AnimatedFloat(float initialValue = 0f)
{
    private float _from;
    private float _to;
    private float _duration;
    private float _elapsed;
    private Func<float, float> _easing = Easing.Linear;

    /// <summary>The current interpolated value.</summary>
    public float Value { get; private set; } = initialValue;

    /// <summary>True while the animation is in progress.</summary>
    public bool IsAnimating { get; private set; }

    /// <summary>
    /// Starts animating from the current <see cref="Value"/> to <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The destination value.</param>
    /// <param name="duration">Duration of the animation in seconds.</param>
    /// <param name="easing">Easing function to use. Defaults to <see cref="Easing.Linear"/>.</param>
    public void AnimateTo(float target, float duration, Func<float, float>? easing = null)
    {
        _from = Value;
        _to = target;
        _duration = MathF.Max(duration, 0f);
        _elapsed = 0f;
        _easing = easing ?? Easing.Linear;
        IsAnimating = _duration > 0f;
        if (!IsAnimating)
            Value = _to;
    }

    /// <summary>Sets the value instantly without animating.</summary>
    public void SetImmediate(float value)
    {
        Value = value;
        IsAnimating = false;
    }

    /// <summary>Advances the animation by <paramref name="deltaTime"/> seconds.</summary>
    public void Update(float deltaTime)
    {
        if (!IsAnimating)
            return;
        _elapsed = MathF.Min(_elapsed + deltaTime, _duration);
        float t = _elapsed / _duration;
        Value = _from + (_to - _from) * _easing(t);
        if (_elapsed >= _duration)
            IsAnimating = false;
    }
}
