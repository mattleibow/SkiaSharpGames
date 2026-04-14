namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Standard easing functions for use with <see cref="AnimatedFloat"/> and other animation systems.
/// Each function maps a normalised time <c>t ∈ [0, 1]</c> to a transformed value in approximately [0, 1].
/// </summary>
public static class Easing
{
    /// <summary>Constant rate — no easing.</summary>
    public static readonly Func<float, float> Linear = t => t;

    /// <summary>Accelerates from rest (quadratic).</summary>
    public static readonly Func<float, float> EaseIn = t => t * t;

    /// <summary>Decelerates to rest (quadratic).</summary>
    public static readonly Func<float, float> EaseOut = t => 1f - (1f - t) * (1f - t);

    /// <summary>Accelerates then decelerates (smooth-step).</summary>
    public static readonly Func<float, float> EaseInOut =
        t => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

    /// <summary>Bounces at the end like a dropped ball.</summary>
    public static readonly Func<float, float> BounceOut = t =>
    {
        const float n = 7.5625f, d = 2.75f;
        if (t < 1f / d) return n * t * t;
        if (t < 2f / d) { t -= 1.5f / d; return n * t * t + 0.75f; }
        if (t < 2.5f / d) { t -= 2.25f / d; return n * t * t + 0.9375f; }
        /* t < 2.625/d */
        t -= 2.625f / d; return n * t * t + 0.984375f;
    };

    /// <summary>Overshoots slightly then settles back (back-ease-out).</summary>
    public static readonly Func<float, float> BackOut = t =>
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
    };

    /// <summary>Elastic snap at the end — oscillates past the target before settling.</summary>
    public static readonly Func<float, float> ElasticOut = t =>
    {
        if (t is 0f or 1f) return t;
        const float c4 = 2f * MathF.PI / 3f;
        return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * c4) + 1f;
    };
}
