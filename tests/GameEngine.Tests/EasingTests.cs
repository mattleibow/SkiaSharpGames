using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class EasingTests
{
    // ── Boundary conditions shared by all functions ────────────────────────

    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(1f, 1f)]
    public void Linear_ReturnsIdentity(float t, float expected)
        => Assert.Equal(expected, Easing.Linear(t), precision: 5);

    [Fact]
    public void Linear_Midpoint_IsHalf()
        => Assert.Equal(0.5f, Easing.Linear(0.5f), precision: 5);

    // ── EaseIn ────────────────────────────────────────────────────────────

    [Fact]
    public void EaseIn_AtZero_ReturnsZero()
        => Assert.Equal(0f, Easing.EaseIn(0f), precision: 5);

    [Fact]
    public void EaseIn_AtOne_ReturnsOne()
        => Assert.Equal(1f, Easing.EaseIn(1f), precision: 5);

    [Fact]
    public void EaseIn_Midpoint_LessThanHalf()
        => Assert.True(Easing.EaseIn(0.5f) < 0.5f,
            "EaseIn should start slow so mid-point value < 0.5");

    // ── EaseOut ───────────────────────────────────────────────────────────

    [Fact]
    public void EaseOut_AtZero_ReturnsZero()
        => Assert.Equal(0f, Easing.EaseOut(0f), precision: 5);

    [Fact]
    public void EaseOut_AtOne_ReturnsOne()
        => Assert.Equal(1f, Easing.EaseOut(1f), precision: 5);

    [Fact]
    public void EaseOut_Midpoint_GreaterThanHalf()
        => Assert.True(Easing.EaseOut(0.5f) > 0.5f,
            "EaseOut should start fast so mid-point value > 0.5");

    // ── EaseInOut ─────────────────────────────────────────────────────────

    [Fact]
    public void EaseInOut_AtZero_ReturnsZero()
        => Assert.Equal(0f, Easing.EaseInOut(0f), precision: 5);

    [Fact]
    public void EaseInOut_AtOne_ReturnsOne()
        => Assert.Equal(1f, Easing.EaseInOut(1f), precision: 5);

    [Fact]
    public void EaseInOut_AtHalf_ReturnsHalf()
        => Assert.Equal(0.5f, Easing.EaseInOut(0.5f), precision: 5);

    // Covers the t >= 0.5 branch of EaseInOut
    [Fact]
    public void EaseInOut_AboveHalf_UsesSecondFormula()
    {
        float v1 = Easing.EaseInOut(0.75f);
        Assert.True(v1 > 0.5f && v1 < 1f);
    }

    // ── BounceOut — 4 segments ────────────────────────────────────────────

    [Fact]
    public void BounceOut_Segment1_t0()
        => Assert.Equal(0f, Easing.BounceOut(0f), precision: 5);

    // t=0.2 → segment 1 (t < 1/2.75 ≈ 0.364)
    [Fact]
    public void BounceOut_Segment1_InRange()
    {
        float v = Easing.BounceOut(0.2f);
        Assert.True(v >= 0f && v < 0.75f);
    }

    // t=0.5 → segment 2 (1/2.75 ≤ t < 2/2.75 ≈ 0.727)
    [Fact]
    public void BounceOut_Segment2_InRange()
    {
        float v = Easing.BounceOut(0.5f);
        Assert.True(v >= 0.75f && v < 0.9375f, $"Expected segment 2 output, got {v}");
    }

    // t=0.82 → segment 3 (2/2.75 ≤ t < 2.5/2.75 ≈ 0.909)
    [Fact]
    public void BounceOut_Segment3_InRange()
    {
        float v = Easing.BounceOut(0.82f);
        Assert.True(v >= 0.9375f && v < 0.984375f, $"Expected segment 3 output, got {v}");
    }

    // t=0.95 → segment 4 (t ≥ 2.5/2.75)
    [Fact]
    public void BounceOut_Segment4_InRange()
    {
        float v = Easing.BounceOut(0.95f);
        Assert.True(v >= 0.984375f && v <= 1f, $"Expected segment 4 output, got {v}");
    }

    [Fact]
    public void BounceOut_AtOne_ReturnsOne()
        => Assert.Equal(1f, Easing.BounceOut(1f), precision: 3);

    // ── BackOut ───────────────────────────────────────────────────────────

    [Fact]
    public void BackOut_AtZero_ReturnsZero()
        => Assert.Equal(0f, Easing.BackOut(0f), precision: 5);

    [Fact]
    public void BackOut_AtOne_ReturnsOne()
        => Assert.Equal(1f, Easing.BackOut(1f), precision: 4);

    [Fact]
    public void BackOut_Overshoots_PastOne()
        => Assert.True(Easing.BackOut(0.8f) > 1f, "BackOut should overshoot past 1 before t=1");

    // ── ElasticOut ────────────────────────────────────────────────────────

    // Boundary t=0 branch
    [Fact]
    public void ElasticOut_AtZero_ReturnsZero()
        => Assert.Equal(0f, Easing.ElasticOut(0f), precision: 5);

    // Boundary t=1 branch
    [Fact]
    public void ElasticOut_AtOne_ReturnsOne()
        => Assert.Equal(1f, Easing.ElasticOut(1f), precision: 5);

    // Non-boundary branch
    [Fact]
    public void ElasticOut_Midpoint_CanOvershoots()
    {
        float v = Easing.ElasticOut(0.5f);
        // at t=0.5 elastic usually overshoots past 1
        Assert.NotEqual(0f, v);
        Assert.NotEqual(1f, v);
    }
}
