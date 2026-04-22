using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class AnimatedFloatTests
{
    [Fact]
    public void Constructor_SetsInitialValue()
    {
        var a = new AnimatedFloat(42f);
        Assert.Equal(42f, a.Value);
        Assert.False(a.IsAnimating);
    }

    [Fact]
    public void Constructor_DefaultValue_IsZero()
    {
        var a = new AnimatedFloat();
        Assert.Equal(0f, a.Value);
    }

    // ── AnimateTo ─────────────────────────────────────────────────────────

    [Fact]
    public void AnimateTo_WithPositiveDuration_SetsIsAnimating()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, 1f);
        Assert.True(a.IsAnimating);
    }

    [Fact]
    public void AnimateTo_ZeroDuration_SnapsImmediately()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, 0f);
        Assert.Equal(100f, a.Value);
        Assert.False(a.IsAnimating);
    }

    [Fact]
    public void AnimateTo_NegativeDuration_TreatedAsZero_SnapsImmediately()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, -5f);
        Assert.Equal(100f, a.Value);
        Assert.False(a.IsAnimating);
    }

    [Fact]
    public void AnimateTo_DefaultEasing_IsLinear()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, 1f); // no easing arg → Linear
        a.Update(0.5f);         // halfway through
        Assert.Equal(50f, a.Value, precision: 3);
    }

    [Fact]
    public void AnimateTo_CustomEasing_Applied()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, 1f, Easing.EaseIn); // EaseIn(0.5) = 0.25
        a.Update(0.5f);
        Assert.True(a.Value < 50f, "EaseIn at halfway should be less than 50");
    }

    // ── SetImmediate ──────────────────────────────────────────────────────

    [Fact]
    public void SetImmediate_UpdatesValueAndStopsAnimation()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, 1f);
        a.SetImmediate(77f);
        Assert.Equal(77f, a.Value);
        Assert.False(a.IsAnimating);
    }

    // ── Update ────────────────────────────────────────────────────────────

    [Fact]
    public void Update_WhenNotAnimating_DoesNothing()
    {
        var a = new AnimatedFloat(50f);
        a.Update(100f); // large dt, should have no effect
        Assert.Equal(50f, a.Value);
    }

    [Fact]
    public void Update_InterpolatesValue()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(200f, 2f);
        a.Update(1f); // halfway
        Assert.Equal(100f, a.Value, precision: 3);
    }

    [Fact]
    public void Update_CompletesAnimation_AtExactEnd()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, 0.5f);
        a.Update(0.5f);
        Assert.Equal(100f, a.Value, precision: 3);
        Assert.False(a.IsAnimating);
    }

    [Fact]
    public void Update_ClampsDeltaTimeAtEnd()
    {
        // dt > remaining duration should still end at target without overshooting
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, 0.1f);
        a.Update(10f); // far beyond duration
        Assert.Equal(100f, a.Value, precision: 3);
        Assert.False(a.IsAnimating);
    }

    [Fact]
    public void Update_MultipleSteps_ReachesTarget()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, 1f);
        for (int i = 0; i < 10; i++)
            a.Update(0.1f);
        Assert.Equal(100f, a.Value, precision: 2);
        Assert.False(a.IsAnimating);
    }

    [Fact]
    public void AnimateTo_MidAnimation_RestartsFromCurrentValue()
    {
        var a = new AnimatedFloat(0f);
        a.AnimateTo(100f, 1f);
        a.Update(0.5f);              // at 50f mid-way
        float mid = a.Value;
        a.AnimateTo(200f, 1f);       // re-target from 50 → 200
        Assert.True(a.IsAnimating);
        a.Update(1f);                // complete new animation
        Assert.Equal(200f, a.Value, precision: 2);
        Assert.True(mid > 0f && mid < 100f, "mid-animation value should be between start and target");
    }
}
