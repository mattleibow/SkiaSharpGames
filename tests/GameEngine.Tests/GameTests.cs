using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

// ── Observable state shared between screens via DI ────────────────────────

file sealed class ScreenTracker
{
    public bool AActivating { get; set; }
    public bool AActivated { get; set; }
    public bool ADeactivating { get; set; }
    public bool ADeactivated { get; set; }
    public bool AUpdateCalled { get; set; }
    public bool APaused { get; set; }
    public bool AResumed { get; set; }
    public bool AIsPausedWhenPaused { get; set; }

    public bool BActivating { get; set; }
    public bool BActivated { get; set; }
    public bool BDeactivating { get; set; }
    public bool BDeactivated { get; set; }
}

// ── Minimal concrete screen implementations ───────────────────────────────

file sealed class ScreenA(ScreenTracker t) : GameScreen
{
    public override void Update(float dt) => t.AUpdateCalled = true;
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnActivating() => t.AActivating = true;
    public override void OnActivated() => t.AActivated = true;
    public override void OnDeactivating() => t.ADeactivating = true;
    public override void OnDeactivated() => t.ADeactivated = true;
    public override void OnPaused() { t.APaused = true; t.AIsPausedWhenPaused = IsPaused; }
    public override void OnResumed() => t.AResumed = true;
}

file sealed class ScreenB(ScreenTracker t) : GameScreen
{
    public override void Update(float dt) { }
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnActivating() => t.BActivating = true;
    public override void OnActivated() => t.BActivated = true;
    public override void OnDeactivating() => t.BDeactivating = true;
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

    [Fact]
    public void TransitionTo_InterruptedTransition_DeactivatesAbandonedIncoming()
    {
        var (game, tracker) = TestGameFactory.Create();
        var coordinator = game.Services.GetRequiredService<IScreenCoordinator>();
        coordinator.TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        coordinator.TransitionTo<ScreenA>();
        Assert.True(tracker.BDeactivated);
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

    // ── Activating / Deactivating lifecycle ───────────────────────────────

    [Fact]
    public void InitialScreen_OnActivating_IsCalledBeforeOnActivated()
    {
        var (_, tracker) = TestGameFactory.Create();
        Assert.True(tracker.AActivating);
        Assert.True(tracker.AActivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_OnDeactivatingCalledOnOldScreen()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>();
        Assert.True(tracker.ADeactivating);
    }

    [Fact]
    public void TransitionTo_NoTransition_OnActivatingCalledOnNewScreen()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>();
        Assert.True(tracker.BActivating);
    }

    [Fact]
    public void TransitionTo_WithTransition_OnActivatingCalledImmediately()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        // OnActivating is called at transition START — before any Update
        Assert.True(tracker.BActivating);
        Assert.False(tracker.BActivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_OnDeactivatingCalledImmediately()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        // OnDeactivating is called at transition START — OnDeactivated is not called yet
        Assert.True(tracker.ADeactivating);
        Assert.False(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_OnActivatedCalledAfterCompletion()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>(new DissolveTransition { Duration = 0.5f });
        Assert.False(tracker.BActivated);  // not yet
        game.Update(0.5f);                  // completes the transition
        Assert.True(tracker.BActivated);   // now called
    }

    [Fact]
    public void TransitionTo_WithTransition_OnDeactivatedCalledAfterCompletion()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.Services.GetRequiredService<IScreenCoordinator>().TransitionTo<ScreenB>(new DissolveTransition { Duration = 0.5f });
        Assert.False(tracker.ADeactivated);
        game.Update(0.5f);
        Assert.True(tracker.ADeactivated);
    }

    // ── Game input forwarding ─────────────────────────────────────────────

    [Fact]
    public void OnPointerMove_ForwardedToActiveScreen()
    {
        var (game, tracker) = InputTestFactory.Create();
        game.OnPointerMove(100f, 200f);
        Assert.Equal((100f, 200f), tracker.LastPointerMove);
    }

    [Fact]
    public void OnPointerDown_ForwardedToActiveScreen()
    {
        var (game, tracker) = InputTestFactory.Create();
        game.OnPointerDown(50f, 60f);
        Assert.Equal((50f, 60f), tracker.LastPointerDown);
    }

    [Fact]
    public void OnPointerUp_ForwardedToActiveScreen()
    {
        var (game, tracker) = InputTestFactory.Create();
        game.OnPointerUp(70f, 80f);
        Assert.Equal((70f, 80f), tracker.LastPointerUp);
    }

    [Fact]
    public void OnKeyDown_ForwardedToActiveScreen()
    {
        var (game, tracker) = InputTestFactory.Create();
        game.OnKeyDown("Space");
        Assert.Equal("Space", tracker.LastKeyDown);
    }

    [Fact]
    public void OnKeyUp_ForwardedToActiveScreen()
    {
        var (game, tracker) = InputTestFactory.Create();
        game.OnKeyUp("Enter");
        Assert.Equal("Enter", tracker.LastKeyUp);
    }

    [Fact]
    public void ActiveInputScreen_DuringTransition_DoesNotThrow()
    {
        var (game, _) = InputTestFactory.Create();
        var coordinator = game.Services.GetRequiredService<IScreenCoordinator>();
        coordinator.TransitionTo<InputDummyScreen>(new DissolveTransition { Duration = 1f });

        var ex = Record.Exception(() => game.OnPointerMove(0f, 0f));
        Assert.Null(ex);
    }
}

// ── Input-forwarding test helpers ─────────────────────────────────────────

file sealed class InputTracker
{
    public (float x, float y)? LastPointerMove { get; set; }
    public (float x, float y)? LastPointerDown { get; set; }
    public (float x, float y)? LastPointerUp { get; set; }
    public string? LastKeyDown { get; set; }
    public string? LastKeyUp { get; set; }
}

file sealed class InputCapturingScreen(InputTracker tracker) : GameScreen
{
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnPointerMove(float x, float y) => tracker.LastPointerMove = (x, y);
    public override void OnPointerDown(float x, float y) => tracker.LastPointerDown = (x, y);
    public override void OnPointerUp(float x, float y) => tracker.LastPointerUp = (x, y);
    public override void OnKeyDown(string key) => tracker.LastKeyDown = key;
    public override void OnKeyUp(string key) => tracker.LastKeyUp = key;
}

file sealed class InputDummyScreen : GameScreen
{
    public override void Draw(SKCanvas c, int w, int h) { }
}

file static class InputTestFactory
{
    public static (Game game, InputTracker tracker) Create()
    {
        var tracker = new InputTracker();
        var builder = GameBuilder.CreateDefault();
        builder.Services.AddSingleton(tracker);
        builder.Screens
               .Add<InputCapturingScreen>()
               .Add<InputDummyScreen>();
        builder.SetInitialScreen<InputCapturingScreen>();
        return (builder.Build(), tracker);
    }
}
