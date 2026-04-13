using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

// ── Observable state shared between screens via DI ────────────────────────

file sealed class ScreenTracker
{
    public bool AActivated          { get; set; }
    public bool ADeactivated        { get; set; }
    public bool AUpdateCalled       { get; set; }
    public bool APaused             { get; set; }
    public bool AResumed            { get; set; }
    public bool AIsPausedWhenPaused { get; set; }

    public bool BActivated  { get; set; }
    public bool BDeactivated{ get; set; }
}

// ── Minimal concrete screen implementations ───────────────────────────────

file sealed class ScreenA(ScreenTracker t) : GameScreenBase
{
    public override void Update(float dt) => t.AUpdateCalled = true;
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnActivated()   => t.AActivated   = true;
    public override void OnDeactivated() => t.ADeactivated = true;
    public override void OnPaused()      { t.APaused = true; t.AIsPausedWhenPaused = IsPaused; }
    public override void OnResumed()     => t.AResumed = true;
}

file sealed class ScreenB(ScreenTracker t) : GameScreenBase
{
    public override void Update(float dt) { }
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnActivated()   => t.BActivated   = true;
    public override void OnDeactivated() => t.BDeactivated = true;
}

file sealed class OverlayScreen : GameScreenBase
{
    public override void Update(float dt) { }
    public override void Draw(SKCanvas c, int w, int h) { }
}

// ── Concrete test Game ────────────────────────────────────────────────────

file sealed class TestGame : Game
{
    // Fully initialized before Configure() is ever called (lazy init)
    public readonly ScreenTracker Tracker = new();

    protected override void Configure(GameBuilder builder)
    {
        builder.Services.AddSingleton(Tracker);
        builder.AddScreen<ScreenA>()
               .AddScreen<ScreenB>()
               .AddScreen<OverlayScreen>();
    }
}

file sealed class ConfigureCallTrackingGame : Game
{
    public bool ConfigureCalled { get; private set; }

    protected override void Configure(GameBuilder builder)
    {
        ConfigureCalled = true;
        builder.AddScreen<OverlayScreen>();
    }
}

// ── Game tests ────────────────────────────────────────────────────────────

public class GameTests
{
    // ── Initial state ──────────────────────────────────────────────────────

    [Fact]
    public void InitialScreen_IsActivated()
    {
        var game = new TestGame();
        game.Update(0f); // trigger lazy init
        Assert.True(game.Tracker.AActivated);
    }

    [Fact]
    public void InitialScreen_Update_IsCalled()
    {
        var game = new TestGame();
        game.Update(0.016f);
        Assert.True(game.Tracker.AUpdateCalled);
    }

    [Fact]
    public void InitialScreen_Draw_DoesNotThrow()
    {
        var game = new TestGame();
        game.Update(0f);
        using var bmp    = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => game.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void GameDimensions_DelegatesToCurrentScreen()
    {
        var game = new TestGame();
        Assert.Equal((800, 600), game.GameDimensions); // also triggers init
    }

    // ── TransitionTo (immediate) ───────────────────────────────────────────

    [Fact]
    public void TransitionTo_NoTransition_DeactivatesOldScreen()
    {
        var game = new TestGame();
        game.Update(0f);
        game.TransitionTo<ScreenB>();
        Assert.True(game.Tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_ActivatesNewScreen()
    {
        var game = new TestGame();
        game.Update(0f);
        game.TransitionTo<ScreenB>();
        Assert.True(game.Tracker.BActivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_NewScreenReceivesUpdate()
    {
        var game = new TestGame();
        game.Update(0f);
        game.TransitionTo<ScreenB>();
        var ex = Record.Exception(() => game.Update(0.016f));
        Assert.Null(ex);
    }

    // ── TransitionTo (with transition) ────────────────────────────────────

    [Fact]
    public void TransitionTo_WithTransition_OldScreenNotDeactivatedYet()
    {
        var game = new TestGame();
        game.Update(0f);
        game.TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        Assert.False(game.Tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_CompletesAfterDuration()
    {
        var game = new TestGame();
        game.Update(0f);
        game.TransitionTo<ScreenB>(new DissolveTransition { Duration = 0.5f });
        game.Update(0.5f);
        Assert.True(game.Tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_DrawDoesNotThrow()
    {
        var game = new TestGame();
        game.Update(0f);
        game.TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        using var bmp    = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bmp);
        game.Update(0.25f);
        var ex = Record.Exception(() => game.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }

    // ── PushOverlay / PopOverlay ───────────────────────────────────────────

    [Fact]
    public void PushOverlay_PausesBaseScreen()
    {
        var game = new TestGame();
        game.Update(0f);
        game.PushOverlay<OverlayScreen>();
        Assert.True(game.Tracker.APaused);
        Assert.True(game.Tracker.AIsPausedWhenPaused);
    }

    [Fact]
    public void PushOverlay_BaseScreenNotUpdated()
    {
        var game = new TestGame();
        game.Update(0f);
        game.PushOverlay<OverlayScreen>();
        game.Tracker.AUpdateCalled = false; // reset after init tick
        game.Update(0.016f);
        Assert.False(game.Tracker.AUpdateCalled);
    }

    [Fact]
    public void PopOverlay_ResumesBaseScreen()
    {
        var game = new TestGame();
        game.Update(0f);
        game.PushOverlay<OverlayScreen>();
        game.PopOverlay();
        Assert.True(game.Tracker.AResumed);
    }

    [Fact]
    public void PopOverlay_WhenEmpty_DoesNothing()
    {
        var game = new TestGame();
        game.Update(0f);
        var ex = Record.Exception(() => game.PopOverlay());
        Assert.Null(ex);
    }

    // ── TransitionTo clears overlay stack ─────────────────────────────────

    [Fact]
    public void TransitionTo_ClearsOverlayStack_DeactivatesBase()
    {
        var game = new TestGame();
        game.Update(0f);
        game.PushOverlay<OverlayScreen>();
        game.TransitionTo<ScreenB>();
        Assert.True(game.Tracker.ADeactivated);
    }
}

// ── GameBuilder / lazy-init tests ─────────────────────────────────────────

public class GameBuilderTests
{
    [Fact]
    public void Configure_IsNotCalledOnConstruction()
    {
        var game = new ConfigureCallTrackingGame();
        Assert.False(game.ConfigureCalled);
    }

    [Fact]
    public void Configure_IsCalledOnFirstUpdate()
    {
        var game = new ConfigureCallTrackingGame();
        game.Update(0f);
        Assert.True(game.ConfigureCalled);
    }

    [Fact]
    public void Configure_IsCalledOnFirstGameDimensionsAccess()
    {
        var game = new ConfigureCallTrackingGame();
        _ = game.GameDimensions;
        Assert.True(game.ConfigureCalled);
    }
}

// ── DissolveTransition tests ───────────────────────────────────────────────

public class DissolveTransitionTests
{
    private static SKCanvas MakeCanvas() => new(new SKBitmap(800, 600));

    [Fact]
    public void Draw_AtProgress0_DoesNotThrow()
    {
        var t  = new DissolveTransition();
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), 0f, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_AtProgress1_DoesNotThrow()
    {
        var t  = new DissolveTransition();
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), 1f, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_AtProgress05_DoesNotThrow()
    {
        var t  = new DissolveTransition();
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), 0.5f, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultDuration_Is04()
        => Assert.Equal(0.4f, new DissolveTransition().Duration);
}

// ── FadeToBlackTransition tests ────────────────────────────────────────────

public class FadeToBlackTransitionTests
{
    private static SKCanvas MakeCanvas() => new(new SKBitmap(800, 600));

    [Theory]
    [InlineData(0f)]
    [InlineData(0.25f)]
    [InlineData(0.5f)]
    [InlineData(0.75f)]
    [InlineData(1f)]
    public void Draw_DoesNotThrow_AtVariousProgress(float progress)
    {
        var t  = new FadeToBlackTransition();
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), progress, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_FirstHalf_CallsOutgoingCallback()
    {
        bool outCalled = false;
        var t = new FadeToBlackTransition();
        t.Draw(MakeCanvas(), 0.2f, _ => outCalled = true, _ => { }, 800, 600);
        Assert.True(outCalled);
    }

    [Fact]
    public void Draw_SecondHalf_CallsIncomingCallback()
    {
        bool inCalled = false;
        var t = new FadeToBlackTransition();
        t.Draw(MakeCanvas(), 0.8f, _ => { }, _ => inCalled = true, 800, 600);
        Assert.True(inCalled);
    }
}

// ── SlideTransition tests ──────────────────────────────────────────────────

public class SlideTransitionTests
{
    private static SKCanvas MakeCanvas() => new(new SKBitmap(800, 600));

    [Theory]
    [InlineData(SlideDirection.Up)]
    [InlineData(SlideDirection.Down)]
    [InlineData(SlideDirection.Left)]
    [InlineData(SlideDirection.Right)]
    public void Draw_AllDirections_DoNotThrow(SlideDirection dir)
    {
        var t  = new SlideTransition { Direction = dir };
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), 0.5f, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_CallsBothScreenCallbacks()
    {
        bool outCalled = false, inCalled = false;
        var t = new SlideTransition();
        t.Draw(MakeCanvas(), 0.5f, _ => outCalled = true, _ => inCalled = true, 800, 600);
        Assert.True(outCalled);
        Assert.True(inCalled);
    }
}
