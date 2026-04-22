using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class LoopedAnimationTests
{
    // ── Construction ──────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SetsProperties()
    {
        var la = new LoopedAnimation(10f, 2f);
        Assert.Equal(10f, la.Period);
        Assert.Equal(2f, la.Duration);
        Assert.False(la.Enabled);
        Assert.False(la.IsActive);
        Assert.Equal(0f, la.Progress);
    }

    // ── Start / Stop ──────────────────────────────────────────────────────

    [Fact]
    public void Start_EnablesAnimation()
    {
        var la = new LoopedAnimation(5f, 1f);
        la.Start();
        Assert.True(la.Enabled);
        Assert.False(la.IsActive); // not yet running — period crossed on next Update
    }

    [Fact]
    public void Stop_DisablesAndResets()
    {
        var la = new LoopedAnimation(1f, 0.5f);
        la.Start();
        la.Update(1.5f); // past period → active
        la.Stop();
        Assert.False(la.Enabled);
        Assert.False(la.IsActive);
        Assert.Equal(0f, la.Progress);
    }

    // ── Update — not enabled ──────────────────────────────────────────────

    [Fact]
    public void Update_WhenNotEnabled_DoesNothing()
    {
        var la = new LoopedAnimation(1f, 0.5f);
        la.Update(100f); // not started
        Assert.False(la.IsActive);
        Assert.Equal(0f, la.Progress);
    }

    // ── initialDelay semantics ─────────────────────────────────────────────
    // Start(delay=0)      → _sinceLastRun = Period → fires on very first Update
    // Start(delay=Period) → _sinceLastRun = 0     → waits a full Period before firing

    [Fact]
    public void Start_WithZeroDelay_FiresOnFirstUpdate()
    {
        // delay=0 → _sinceLastRun starts at Period → any update triggers it
        var la = new LoopedAnimation(period: 2f, duration: 1f);
        la.Start(initialDelay: 0f);
        la.Update(0.01f); // tiny tick — crosses period immediately
        Assert.True(la.IsActive);
    }

    [Fact]
    public void Start_WithFullPeriodDelay_WaitsFullPeriodBeforeFiring()
    {
        // delay=Period → _sinceLastRun = 0 → must wait full Period
        var la = new LoopedAnimation(period: 2f, duration: 1f);
        la.Start(initialDelay: 2f);

        la.Update(1.9f); // not yet at period
        Assert.False(la.IsActive);

        la.Update(0.2f); // now crosses threshold
        Assert.True(la.IsActive);
    }

    [Fact]
    public void Start_WithHalfPeriodDelay_WaitsHalfPeriod()
    {
        // delay=Period/2 → _sinceLastRun = Period/2 → fires after Period/2 more seconds
        var la = new LoopedAnimation(period: 2f, duration: 1f);
        la.Start(initialDelay: 1f); // _sinceLastRun = 1

        la.Update(0.9f); // 1+0.9 = 1.9 < 2 → not yet active
        Assert.False(la.IsActive);

        la.Update(0.2f); // 1.9+0.2 = 2.1 >= 2 → fires
        Assert.True(la.IsActive);
    }

    // ── Active phase ──────────────────────────────────────────────────────

    [Fact]
    public void Update_WhenActive_ProgressAdvances()
    {
        var la = new LoopedAnimation(period: 1f, duration: 2f);
        la.Start(); // fires immediately
        la.Update(0.01f); // become active
        la.Update(1f);    // advance 1s through a 2s duration → Progress ≈ 0.5
        Assert.True(la.IsActive);
        Assert.True(la.Progress > 0.4f && la.Progress < 0.6f);
    }

    [Fact]
    public void Update_ActiveCompletes_ProgressReaches1_ThenWaits()
    {
        var la = new LoopedAnimation(period: 1f, duration: 1f);
        la.Start();       // fires immediately
        la.Update(0.01f); // become active
        la.Update(1f);    // complete run
        Assert.False(la.IsActive);
        Assert.Equal(1f, la.Progress);
    }

    [Fact]
    public void Update_AfterCompletion_WaitsForNextPeriod()
    {
        var la = new LoopedAnimation(period: 2f, duration: 0.5f);
        la.Start();       // fires immediately
        la.Update(0.01f); // become active
        la.Update(0.5f);  // complete run → waiting again

        Assert.False(la.IsActive);

        // Should not be active again until another 2 seconds have elapsed
        la.Update(1.0f);
        Assert.False(la.IsActive);

        la.Update(1.1f); // crosses 2s
        Assert.True(la.IsActive);
    }

    // ── RepeatCount ────────────────────────────────────────────────────────

    [Fact]
    public void RepeatCount_DefaultIsInfinite()
    {
        var la = new LoopedAnimation(1f, 0.1f);
        Assert.Equal(-1, la.RepeatCount);
    }

    [Fact]
    public void RepeatCount_Finite_StopsAfterNRuns()
    {
        var la = new LoopedAnimation(period: 0.1f, duration: 0.1f) { RepeatCount = 2 };
        la.Start(); // fires immediately on first update

        // Run 1
        la.Update(0.01f); // trigger → active
        la.Update(0.1f);  // run completes
        Assert.False(la.IsActive);
        Assert.True(la.Enabled, "still enabled after 1st run");

        // Run 2
        la.Update(0.1f); // wait period → active
        la.Update(0.1f); // run completes → 2 runs done → Stop()
        Assert.False(la.IsActive);
        Assert.False(la.Enabled, "disabled after RepeatCount reached");
    }

    [Fact]
    public void RepeatCount_Zero_StopsAfterFirstRun()
    {
        // RepeatCount=0 means: stop as soon as _completedRuns >= 0 (after any run)
        var la = new LoopedAnimation(period: 0.1f, duration: 0.1f) { RepeatCount = 0 };
        la.Start();

        la.Update(0.01f); // trigger → active
        la.Update(0.1f);  // run completes → _completedRuns(1) >= RepeatCount(0) → Stop()
        Assert.False(la.Enabled);
    }
}
