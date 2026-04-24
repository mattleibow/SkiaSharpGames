using SkiaSharp.Theatre;

using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class CountdownTimerTests
{
    // ── Default state ──────────────────────────────────────────────────────

    [Fact]
    public void Default_IsNotActive()
    {
        CountdownTimer t = default;
        Assert.False(t.Active);
    }

    [Fact]
    public void Default_RemainingIsZero()
    {
        CountdownTimer t = default;
        Assert.Equal(0f, t.Remaining);
    }

    // ── Set ────────────────────────────────────────────────────────────────

    [Fact]
    public void Set_WithPositiveDuration_MakesActive()
    {
        CountdownTimer t = default;
        t.Set(2f);
        Assert.True(t.Active);
        Assert.Equal(2f, t.Remaining);
    }

    [Fact]
    public void Set_WithZero_RemainsInactive()
    {
        CountdownTimer t = default;
        t.Set(0f);
        Assert.False(t.Active);
    }

    [Fact]
    public void Set_WithNegativeDuration_ClampedToZero()
    {
        CountdownTimer t = default;
        t.Set(-5f);
        Assert.False(t.Active);
        Assert.Equal(0f, t.Remaining);
    }

    // ── Reset ──────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_CancelsActiveTimer()
    {
        CountdownTimer t = default;
        t.Set(3f);
        t.Reset();
        Assert.False(t.Active);
        Assert.Equal(0f, t.Remaining);
    }

    // ── Tick ──────────────────────────────────────────────────────────────

    [Fact]
    public void Tick_WhenNotActive_ReturnsFalse()
    {
        CountdownTimer t = default;
        bool expired = t.Tick(1f);
        Assert.False(expired);
    }

    [Fact]
    public void Tick_DecrementsRemaining()
    {
        CountdownTimer t = default;
        t.Set(2f);
        t.Tick(0.5f);
        Assert.True(t.Active);
        Assert.Equal(1.5f, t.Remaining, precision: 4);
    }

    [Fact]
    public void Tick_ReturnsTrueExactlyOnExpiryTick()
    {
        CountdownTimer t = default;
        t.Set(1f);

        bool mid = t.Tick(0.5f);
        Assert.False(mid, "should not expire at 0.5 s");
        Assert.True(t.Active);

        bool expired = t.Tick(0.5f);
        Assert.True(expired, "should expire at 1.0 s total");
        Assert.False(t.Active);
        Assert.Equal(0f, t.Remaining);
    }

    [Fact]
    public void Tick_WithLargeDeltaTime_ExpiresInOneTick()
    {
        CountdownTimer t = default;
        t.Set(0.1f);

        bool expired = t.Tick(10f);
        Assert.True(expired);
        Assert.False(t.Active);
        Assert.Equal(0f, t.Remaining);
    }

    [Fact]
    public void Tick_AfterExpiry_ReturnsFalse()
    {
        CountdownTimer t = default;
        t.Set(0.5f);
        t.Tick(1f); // expires

        bool secondTick = t.Tick(1f);
        Assert.False(secondTick, "should not fire again after expiry");
    }

    [Fact]
    public void Tick_DoesNotGoBelowZero()
    {
        CountdownTimer t = default;
        t.Set(0.5f);
        t.Tick(100f);
        Assert.Equal(0f, t.Remaining);
    }

    // ── Re-set after expiry ────────────────────────────────────────────────

    [Fact]
    public void Set_AfterExpiry_RestartsTimer()
    {
        CountdownTimer t = default;
        t.Set(0.5f);
        t.Tick(1f); // expires

        t.Set(2f);
        Assert.True(t.Active);
        Assert.Equal(2f, t.Remaining);

        bool expired = t.Tick(2f);
        Assert.True(expired);
    }
}
