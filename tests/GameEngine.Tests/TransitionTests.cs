using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

// ── Minimal concrete subclass for testing GameScreenBase ──────────────────

file sealed class TestGame : GameScreenBase
{
    public bool UpdateCalled { get; private set; }
    public bool DrawCalled   { get; private set; }

    public override void Update(float deltaTime)
    {
        UpdateCalled = true;
        UpdateTransition(deltaTime);
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        DrawCalled = true;
        DrawTransitionOverlay(canvas);
    }

    // Expose protected helpers for testing
    public void StartFade(float halfDuration, Action midpoint)
        => BeginTransition(new FadeTransition(), halfDuration, midpoint);

    public void StartSlide(SlideDirection dir, float halfDuration, Action midpoint)
        => BeginTransition(new SlideTransition { Direction = dir }, halfDuration, midpoint);
}

public class GameScreenBaseTests
{
    // ── Default dimensions ─────────────────────────────────────────────────

    [Fact]
    public void DefaultDimensions_Are800x600()
    {
        var g = new TestGame();
        var (w, h) = g.GameDimensions;
        Assert.Equal(800, w);
        Assert.Equal(600, h);
    }

    // ── No transition ──────────────────────────────────────────────────────

    [Fact]
    public void IsTransitioning_Initially_IsFalse()
        => Assert.False(new TestGame().IsTransitioning);

    // ── BeginTransition + IsTransitioning ─────────────────────────────────

    [Fact]
    public void BeginTransition_SetsIsTransitioning()
    {
        var g = new TestGame();
        g.StartFade(0.3f, () => { });
        Assert.True(g.IsTransitioning);
    }

    // ── UpdateTransition — Out phase ──────────────────────────────────────

    [Fact]
    public void UpdateTransition_OutPhase_MidpointNotCalledYet()
    {
        var g = new TestGame();
        bool midpointCalled = false;
        g.StartFade(0.5f, () => midpointCalled = true);

        g.Update(0.4f); // not yet at midpoint (progress 0.8 < 1)
        Assert.False(midpointCalled);
        Assert.True(g.IsTransitioning);
    }

    [Fact]
    public void UpdateTransition_OutPhaseComplete_CallsMidpoint()
    {
        var g = new TestGame();
        bool midpointCalled = false;
        g.StartFade(0.3f, () => midpointCalled = true);

        g.Update(0.3f); // progress = 1 → midpoint fires, switches to In
        Assert.True(midpointCalled);
        Assert.True(g.IsTransitioning); // still transitioning (now In phase)
    }

    // ── UpdateTransition — In phase ────────────────────────────────────────

    [Fact]
    public void UpdateTransition_InPhaseComplete_ClearsTransition()
    {
        var g = new TestGame();
        g.StartFade(0.1f, () => { });

        g.Update(0.1f); // completes Out phase → enters In
        Assert.True(g.IsTransitioning);

        g.Update(0.1f); // completes In phase
        Assert.False(g.IsTransitioning);
    }

    // ── UpdateTransition — no-op when not transitioning ───────────────────

    [Fact]
    public void UpdateTransition_WhenNotTransitioning_DoesNothing()
    {
        var g = new TestGame();
        g.Update(1f); // should not throw or change IsTransitioning
        Assert.False(g.IsTransitioning);
    }

    // ── DrawTransitionOverlay ─────────────────────────────────────────────

    [Fact]
    public void DrawTransitionOverlay_WhenNotTransitioning_DoesNotThrow()
    {
        var g = new TestGame();
        using var bitmap = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => g.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void DrawTransitionOverlay_DuringTransition_DoesNotThrow()
    {
        var g = new TestGame();
        g.StartFade(1f, () => { });

        using var bitmap = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => g.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }
}

// ── FadeTransition tests ──────────────────────────────────────────────────

public class FadeTransitionTests
{
    [Fact]
    public void Draw_ZeroCoverage_DoesNotThrow()
    {
        var t = new FadeTransition();
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => t.Draw(canvas, 0f, (100, 100)));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_FullCoverage_DoesNotThrow()
    {
        var t = new FadeTransition();
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => t.Draw(canvas, 1f, (100, 100)));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_HalfCoverage_DoesNotThrow()
    {
        var t = new FadeTransition();
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => t.Draw(canvas, 0.5f, (100, 100)));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_CustomColor_DoesNotThrow()
    {
        var t = new FadeTransition { Color = SKColors.Red };
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        var ex = Record.Exception(() => t.Draw(canvas, 0.5f, (100, 100)));
        Assert.Null(ex);
    }
}

// ── SlideTransition tests ─────────────────────────────────────────────────

public class SlideTransitionTests
{
    private static SKCanvas MakeCanvas() => new SKCanvas(new SKBitmap(800, 600));

    [Theory]
    [InlineData(SlideDirection.Up)]
    [InlineData(SlideDirection.Down)]
    [InlineData(SlideDirection.Left)]
    [InlineData(SlideDirection.Right)]
    public void Draw_AllDirections_DoNotThrow(SlideDirection dir)
    {
        var t = new SlideTransition { Direction = dir };
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() => t.Draw(canvas, 0.5f, (800, 600)));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_ZeroCoverage_DoesNotThrow()
    {
        var t = new SlideTransition();
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() => t.Draw(canvas, 0f, (800, 600)));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_FullCoverage_DoesNotThrow()
    {
        var t = new SlideTransition { Direction = SlideDirection.Right };
        using var canvas = MakeCanvas();
        var ex = Record.Exception(() => t.Draw(canvas, 1f, (800, 600)));
        Assert.Null(ex);
    }
}
