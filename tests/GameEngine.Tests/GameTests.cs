using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

// ── Observable state shared between screens via DI ────────────────────────

file sealed class ScreenTracker
{
    public bool AActivated { get; set; }
    public bool ADeactivated { get; set; }
    public bool AUpdateCalled { get; set; }
    public bool APaused { get; set; }
    public bool AResumed { get; set; }
    public bool AIsPausedWhenPaused { get; set; }

    public bool BActivated { get; set; }
    public bool BDeactivated { get; set; }
}

// ── Minimal concrete screen implementations ───────────────────────────────

file sealed class ScreenA(ScreenTracker t) : GameScreen
{
    public override void Update(float dt) => t.AUpdateCalled = true;
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnActivated() => t.AActivated = true;
    public override void OnDeactivated() => t.ADeactivated = true;
    public override void OnPaused() { t.APaused = true; t.AIsPausedWhenPaused = IsPaused; }
    public override void OnResumed() => t.AResumed = true;
}

file sealed class ScreenB(ScreenTracker t) : GameScreen
{
    public override void Update(float dt) { }
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnActivated() => t.BActivated = true;
    public override void OnDeactivated() => t.BDeactivated = true;
}

file sealed class OverlayScreen : GameScreen
{
    public override void Update(float dt) { }
    public override void Draw(SKCanvas c, int w, int h) { }
}

// ── Builder helpers ───────────────────────────────────────────────────────

file static class TestGameFactory
{
    /// <summary>Builds a game with ScreenA (initial), ScreenB, and OverlayScreen.</summary>
    public static (Game game, ScreenTracker tracker) Create()
    {
        var tracker = new ScreenTracker();

        var builder = GameBuilder.CreateDefault();
        builder.Services.AddSingleton(tracker);
        builder.Screens
               .Add<ScreenA>()
               .Add<ScreenB>()
               .Add<OverlayScreen>();
        builder.SetInitialScreen<ScreenA>();

        return (builder.Build(), tracker);
    }
}

// ── Game tests ────────────────────────────────────────────────────────────

public class GameTests
{
    // ── Initial state ──────────────────────────────────────────────────────

    [Fact]
    public void InitialScreen_IsActivated()
    {
        var (_, tracker) = TestGameFactory.Create();
        Assert.True(tracker.AActivated);
    }

    [Fact]
    public void InitialScreen_Update_IsCalled()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Update(0.016f);
        Assert.True(tracker.AUpdateCalled);
    }

    [Fact]
    public void InitialScreen_Draw_DoesNotThrow()
    {
        var (game, _) = TestGameFactory.Create();
        using var bmp = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => game.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void GameDimensions_ReturnsBuilderValue()
    {
        var (game, _) = TestGameFactory.Create();
        Assert.Equal(new SKSize(800, 600), game.GameDimensions);
    }

    [Fact]
    public void Services_ExposesIScreenCoordinator()
    {
        var (game, _) = TestGameFactory.Create();
        var coordinator = game.Services.GetRequiredService<IScreenCoordinator>();
        Assert.NotNull(coordinator);
    }

    // ── TransitionTo (immediate) ───────────────────────────────────────────

    [Fact]
    public void TransitionTo_NoTransition_DeactivatesOldScreen()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>();
        Assert.True(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_ActivatesNewScreen()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>();
        Assert.True(tracker.BActivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_NewScreenReceivesUpdate()
    {
        var (game, _) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>();
        var ex = Record.Exception(() => game.Update(0.016f));
        Assert.Null(ex);
    }

    // ── TransitionTo (with transition) ────────────────────────────────────

    [Fact]
    public void TransitionTo_WithTransition_OldScreenNotDeactivatedYet()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        Assert.False(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_CompletesAfterDuration()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>(new DissolveTransition { Duration = 0.5f });
        game.Update(0.5f);
        Assert.True(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_DrawDoesNotThrow()
    {
        var (game, _) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        using var bmp = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bmp);
        game.Update(0.25f);
        var ex = Record.Exception(() => game.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }

    // ── PushOverlay / PopOverlay ───────────────────────────────────────────

    [Fact]
    public void PushOverlay_PausesBaseScreen()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().PushOverlay<OverlayScreen>();
        Assert.True(tracker.APaused);
        Assert.True(tracker.AIsPausedWhenPaused);
    }

    [Fact]
    public void PushOverlay_BaseScreenNotUpdated()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().PushOverlay<OverlayScreen>();
        tracker.AUpdateCalled = false; // reset after initial activation
        game.Update(0.016f);
        Assert.False(tracker.AUpdateCalled);
    }

    [Fact]
    public void PopOverlay_ResumesBaseScreen()
    {
        var (game, tracker) = TestGameFactory.Create();
        var coordinator = game.Services.GetRequiredService<IScreenCoordinator>();
        coordinator.PushOverlay<OverlayScreen>();
        coordinator.PopOverlay();
        Assert.True(tracker.AResumed);
    }

    [Fact]
    public void PopOverlay_WhenEmpty_DoesNothing()
    {
        var (game, _) = TestGameFactory.Create();
        var ex = Record.Exception(() => game.Services.GetRequiredService<IScreenCoordinator>().PopOverlay());
        Assert.Null(ex);
    }

    // ── TransitionTo clears overlay stack ─────────────────────────────────

    [Fact]
    public void TransitionTo_ClearsOverlayStack_DeactivatesBase()
    {
        var (game, tracker) = TestGameFactory.Create();
        var coordinator = game.Services.GetRequiredService<IScreenCoordinator>();
        coordinator.PushOverlay<OverlayScreen>();
        coordinator.TransitionTo<ScreenB>();
        Assert.True(tracker.ADeactivated);
    }
}
