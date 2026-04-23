using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

// ── Observable state shared between scenes via DI ────────────────────────

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

// ── Minimal concrete scene implementations ───────────────────────────────

file sealed class ScreenA(ScreenTracker t) : Scene
{
    protected override void OnUpdate(float dt) => t.AUpdateCalled = true;
    protected override void OnDraw(SKCanvas c) { }
    public override void OnActivating() => t.AActivating = true;
    public override void OnActivated() => t.AActivated = true;
    public override void OnDeactivating() => t.ADeactivating = true;
    public override void OnDeactivated() => t.ADeactivated = true;
    public override void OnPaused() { t.APaused = true; t.AIsPausedWhenPaused = IsPaused; }
    public override void OnResumed() => t.AResumed = true;
}

file sealed class ScreenB(ScreenTracker t) : Scene
{
    protected override void OnUpdate(float dt) { }
    protected override void OnDraw(SKCanvas c) { }
    public override void OnActivating() => t.BActivating = true;
    public override void OnActivated() => t.BActivated = true;
    public override void OnDeactivating() => t.BDeactivating = true;
    public override void OnDeactivated() => t.BDeactivated = true;
}

file sealed class OverlayScreen : Scene
{
    protected override void OnUpdate(float dt) { }
    protected override void OnDraw(SKCanvas c) { }
}

// ── Builder helpers ───────────────────────────────────────────────────────

file static class TestGameFactory
{
    /// <summary>Builds a game with ScreenA (initial), ScreenB, and OverlayScreen.</summary>
    public static (Stage stage, ScreenTracker tracker) Create()
    {
        var tracker = new ScreenTracker();

        var builder = StageBuilder.Create();
        builder.Services.AddSingleton(tracker);
        builder.Scenes
               .Add<ScreenA>()
               .Add<ScreenB>()
               .Add<OverlayScreen>();
        builder.SetOpeningScene<ScreenA>();

        return (builder.Open(), tracker);
    }
}

// ── Stage tests ────────────────────────────────────────────────────────────

public class StageTests
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
        var (stage, tracker) = TestGameFactory.Create();
        stage.Update(0.016f);
        Assert.True(tracker.AUpdateCalled);
    }

    [Fact]
    public void InitialScreen_Draw_DoesNotThrow()
    {
        var (stage, _) = TestGameFactory.Create();
        using var bmp = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => stage.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void GameDimensions_ReturnsBuilderValue()
    {
        var (stage, _) = TestGameFactory.Create();
        Assert.Equal(new SKSize(800, 600), stage.StageSize);
    }

    [Fact]
    public void Services_ExposesIDirector()
    {
        var (stage, _) = TestGameFactory.Create();
        var director = stage.Services.GetRequiredService<IDirector>();
        Assert.NotNull(director);
    }

    // ── TransitionTo (immediate) ───────────────────────────────────────────

    [Fact]
    public void TransitionTo_NoTransition_DeactivatesOldScreen()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>();
        Assert.True(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_ActivatesNewScreen()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>();
        Assert.True(tracker.BActivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_NewScreenReceivesUpdate()
    {
        var (stage, _) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>();
        var ex = Record.Exception(() => stage.Update(0.016f));
        Assert.Null(ex);
    }

    // ── TransitionTo (with transition) ────────────────────────────────────

    [Fact]
    public void TransitionTo_WithTransition_OldScreenNotDeactivatedYet()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>(new DissolveCurtain { Duration = 1f });
        Assert.False(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_CompletesAfterDuration()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>(new DissolveCurtain { Duration = 0.5f });
        stage.Update(0.5f);
        Assert.True(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_DrawDoesNotThrow()
    {
        var (stage, _) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>(new DissolveCurtain { Duration = 1f });
        using var bmp = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bmp);
        stage.Update(0.25f);
        var ex = Record.Exception(() => stage.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void TransitionTo_InterruptedTransition_DeactivatesAbandonedIncoming()
    {
        var (stage, tracker) = TestGameFactory.Create();
        var director = stage.Services.GetRequiredService<IDirector>();
        director.TransitionTo<ScreenB>(new DissolveCurtain { Duration = 1f });
        director.TransitionTo<ScreenA>();
        Assert.True(tracker.BDeactivated);
    }

    // ── PushScene / PopScene ───────────────────────────────────────────

    [Fact]
    public void PushScene_PausesBaseScreen()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().PushScene<OverlayScreen>();
        Assert.True(tracker.APaused);
        Assert.True(tracker.AIsPausedWhenPaused);
    }

    [Fact]
    public void PushScene_BaseScreenNotUpdated()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().PushScene<OverlayScreen>();
        tracker.AUpdateCalled = false; // reset after initial activation
        stage.Update(0.016f);
        Assert.False(tracker.AUpdateCalled);
    }

    [Fact]
    public void PopScene_ResumesBaseScreen()
    {
        var (stage, tracker) = TestGameFactory.Create();
        var director = stage.Services.GetRequiredService<IDirector>();
        director.PushScene<OverlayScreen>();
        director.PopScene();
        Assert.True(tracker.AResumed);
    }

    [Fact]
    public void PopScene_WhenEmpty_DoesNothing()
    {
        var (stage, _) = TestGameFactory.Create();
        var ex = Record.Exception(() => stage.Services.GetRequiredService<IDirector>().PopScene());
        Assert.Null(ex);
    }

    // ── TransitionTo clears scene stack ─────────────────────────────────

    [Fact]
    public void TransitionTo_ClearsOverlayStack_DeactivatesBase()
    {
        var (stage, tracker) = TestGameFactory.Create();
        var director = stage.Services.GetRequiredService<IDirector>();
        director.PushScene<OverlayScreen>();
        director.TransitionTo<ScreenB>();
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
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>();
        Assert.True(tracker.ADeactivating);
    }

    [Fact]
    public void TransitionTo_NoTransition_OnActivatingCalledOnNewScreen()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>();
        Assert.True(tracker.BActivating);
    }

    [Fact]
    public void TransitionTo_WithTransition_OnActivatingCalledImmediately()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>(new DissolveCurtain { Duration = 1f });
        // OnActivating is called at transition START — before any Update
        Assert.True(tracker.BActivating);
        Assert.False(tracker.BActivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_OnDeactivatingCalledImmediately()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>(new DissolveCurtain { Duration = 1f });
        // OnDeactivating is called at transition START — OnDeactivated is not called yet
        Assert.True(tracker.ADeactivating);
        Assert.False(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_OnActivatedCalledAfterCompletion()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>(new DissolveCurtain { Duration = 0.5f });
        Assert.False(tracker.BActivated);  // not yet
        stage.Update(0.5f);                  // completes the transition
        Assert.True(tracker.BActivated);   // now called
    }

    [Fact]
    public void TransitionTo_WithTransition_OnDeactivatedCalledAfterCompletion()
    {
        var (stage, tracker) = TestGameFactory.Create();
        stage.Services.GetRequiredService<IDirector>().TransitionTo<ScreenB>(new DissolveCurtain { Duration = 0.5f });
        Assert.False(tracker.ADeactivated);
        stage.Update(0.5f);
        Assert.True(tracker.ADeactivated);
    }

    // ── Stage input forwarding ─────────────────────────────────────────────

    [Fact]
    public void OnPointerMove_ForwardedToActiveScreen()
    {
        var (stage, tracker) = InputTestFactory.Create();
        stage.OnPointerMove(100f, 200f);
        Assert.Equal((100f, 200f), tracker.LastPointerMove);
    }

    [Fact]
    public void OnPointerDown_ForwardedToActiveScreen()
    {
        var (stage, tracker) = InputTestFactory.Create();
        stage.OnPointerDown(50f, 60f);
        Assert.Equal((50f, 60f), tracker.LastPointerDown);
    }

    [Fact]
    public void OnPointerUp_ForwardedToActiveScreen()
    {
        var (stage, tracker) = InputTestFactory.Create();
        stage.OnPointerUp(70f, 80f);
        Assert.Equal((70f, 80f), tracker.LastPointerUp);
    }

    [Fact]
    public void OnKeyDown_ForwardedToActiveScreen()
    {
        var (stage, tracker) = InputTestFactory.Create();
        stage.OnKeyDown("Space");
        Assert.Equal("Space", tracker.LastKeyDown);
    }

    [Fact]
    public void OnKeyUp_ForwardedToActiveScreen()
    {
        var (stage, tracker) = InputTestFactory.Create();
        stage.OnKeyUp("Enter");
        Assert.Equal("Enter", tracker.LastKeyUp);
    }

    [Fact]
    public void ActiveInputScene_DuringTransition_DoesNotThrow()
    {
        var (stage, _) = InputTestFactory.Create();
        var director = stage.Services.GetRequiredService<IDirector>();
        director.TransitionTo<InputDummyScreen>(new DissolveCurtain { Duration = 1f });

        var ex = Record.Exception(() => stage.OnPointerMove(0f, 0f));
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

file sealed class InputCapturingScreen(InputTracker tracker) : Scene
{
    protected override void OnDraw(SKCanvas c) { }
    public override void OnPointerMove(float x, float y) => tracker.LastPointerMove = (x, y);
    public override void OnPointerDown(float x, float y) => tracker.LastPointerDown = (x, y);
    public override void OnPointerUp(float x, float y) => tracker.LastPointerUp = (x, y);
    public override void OnKeyDown(string key) => tracker.LastKeyDown = key;
    public override void OnKeyUp(string key) => tracker.LastKeyUp = key;
}

file sealed class InputDummyScreen : Scene
{
    protected override void OnDraw(SKCanvas c) { }
}

file static class InputTestFactory
{
    public static (Stage stage, InputTracker tracker) Create()
    {
        var tracker = new InputTracker();
        var builder = StageBuilder.Create();
        builder.Services.AddSingleton(tracker);
        builder.Scenes
               .Add<InputCapturingScreen>()
               .Add<InputDummyScreen>();
        builder.SetOpeningScene<InputCapturingScreen>();
        return (builder.Open(), tracker);
    }
}
