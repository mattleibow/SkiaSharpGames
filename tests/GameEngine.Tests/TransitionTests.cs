using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

// ── Minimal concrete screen implementations ───────────────────────────────

file sealed class ScreenA : GameScreenBase
{
    public bool UpdateCalled { get; private set; }
    public bool Activated    { get; private set; }
    public bool Deactivated  { get; private set; }
    public bool Paused       { get; private set; }
    public bool Resumed      { get; private set; }

    public override void Update(float deltaTime) => UpdateCalled = true;
    public override void Draw(SKCanvas canvas, int w, int h) { }
    public override void OnActivated()   => Activated   = true;
    public override void OnDeactivated() => Deactivated = true;
    public override void OnPaused()      => Paused      = true;
    public override void OnResumed()     => Resumed     = true;
}

file sealed class ScreenB : GameScreenBase
{
    public bool Activated   { get; private set; }
    public bool Deactivated { get; private set; }

    public override void Update(float deltaTime) { }
    public override void Draw(SKCanvas canvas, int w, int h) { }
    public override void OnActivated()   => Activated   = true;
    public override void OnDeactivated() => Deactivated = true;
}

file sealed class OverlayScreen : GameScreenBase
{
    public override void Update(float deltaTime) { }
    public override void Draw(SKCanvas canvas, int w, int h) { }
}

// Builds a coordinator with ScreenA as initial, ScreenB and OverlayScreen available.
file static class CoordFactory
{
    public static (ScreenCoordinator coord, ScreenA initial) Create()
    {
        var services = new ServiceCollection();
        services.AddTransient<ScreenA>();
        services.AddTransient<ScreenB>();
        services.AddTransient<OverlayScreen>();
        var provider = services.BuildServiceProvider();
        var initial  = provider.GetRequiredService<ScreenA>();
        var coord    = new ScreenCoordinator(provider, initial);
        return (coord, initial);
    }
}

// ── ScreenCoordinator tests ────────────────────────────────────────────────

public class ScreenCoordinatorTests
{
    [Fact]
    public void InitialScreen_IsActivated()
    {
        var (_, initial) = CoordFactory.Create();
        Assert.True(initial.Activated);
    }

    [Fact]
    public void InitialScreen_Update_IsCalled()
    {
        var (coord, initial) = CoordFactory.Create();
        coord.Update(0.016f);
        Assert.True(initial.UpdateCalled);
    }

    [Fact]
    public void InitialScreen_Draw_DoesNotThrow()
    {
        var (coord, _) = CoordFactory.Create();
        using var bmp    = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bmp);
        var ex = Record.Exception(() => coord.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void GameDimensions_DelegatesToCurrentScreen()
    {
        var (coord, _) = CoordFactory.Create();
        Assert.Equal((800, 600), coord.GameDimensions);
    }

    [Fact]
    public void TransitionTo_NoTransition_DeactivatesOldScreen()
    {
        var (coord, initial) = CoordFactory.Create();
        coord.TransitionTo<ScreenB>();
        Assert.True(initial.Deactivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_NewScreenReceivesUpdate()
    {
        var (coord, _) = CoordFactory.Create();
        coord.TransitionTo<ScreenB>();
        var ex = Record.Exception(() => coord.Update(0.016f));
        Assert.Null(ex);
    }

    [Fact]
    public void TransitionTo_WithTransition_OldScreenNotDeactivatedYet()
    {
        var (coord, initial) = CoordFactory.Create();
        coord.TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        Assert.False(initial.Deactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_CompletesAfterDuration()
    {
        var (coord, initial) = CoordFactory.Create();
        coord.TransitionTo<ScreenB>(new DissolveTransition { Duration = 0.5f });
        coord.Update(0.5f);
        Assert.True(initial.Deactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_DrawDoesNotThrow()
    {
        var (coord, _) = CoordFactory.Create();
        coord.TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        using var bmp    = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bmp);
        coord.Update(0.25f);
        var ex = Record.Exception(() => coord.Draw(canvas, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void PushOverlay_PausesBaseScreen()
    {
        var (coord, initial) = CoordFactory.Create();
        coord.PushOverlay<OverlayScreen>();
        Assert.True(initial.IsPaused);
        Assert.True(initial.Paused);
    }

    [Fact]
    public void PushOverlay_BaseScreenNotUpdated()
    {
        var (coord, initial) = CoordFactory.Create();
        coord.PushOverlay<OverlayScreen>();
        coord.Update(0.016f);
        Assert.False(initial.UpdateCalled);
    }

    [Fact]
    public void PopOverlay_ResumesBaseScreen()
    {
        var (coord, initial) = CoordFactory.Create();
        coord.PushOverlay<OverlayScreen>();
        coord.PopOverlay();
        Assert.False(initial.IsPaused);
        Assert.True(initial.Resumed);
    }

    [Fact]
    public void PopOverlay_WhenEmpty_DoesNothing()
    {
        var (coord, _) = CoordFactory.Create();
        var ex = Record.Exception(() => coord.PopOverlay());
        Assert.Null(ex);
    }

    [Fact]
    public void TransitionTo_ClearsOverlayStack_DeactivatesBase()
    {
        var (coord, initial) = CoordFactory.Create();
        coord.PushOverlay<OverlayScreen>();
        coord.TransitionTo<ScreenB>();
        Assert.True(initial.Deactivated);
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
