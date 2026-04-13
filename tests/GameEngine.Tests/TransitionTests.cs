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

file sealed class ScreenA(ScreenTracker t) : GameScreen
{
    public override void Update(float dt) => t.AUpdateCalled = true;
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnActivated()   => t.AActivated   = true;
    public override void OnDeactivated() => t.ADeactivated = true;
    public override void OnPaused()      { t.APaused = true; t.AIsPausedWhenPaused = IsPaused; }
    public override void OnResumed()     => t.AResumed = true;
}

file sealed class ScreenB(ScreenTracker t) : GameScreen
{
    public override void Update(float dt) { }
    public override void Draw(SKCanvas c, int w, int h) { }
    public override void OnActivated()   => t.BActivated   = true;
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
        using var bmp    = new SKBitmap(800, 600);
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

    // ── TransitionTo (immediate) ───────────────────────────────────────────

    [Fact]
    public void TransitionTo_NoTransition_DeactivatesOldScreen()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.TransitionTo<ScreenB>();
        Assert.True(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_ActivatesNewScreen()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.TransitionTo<ScreenB>();
        Assert.True(tracker.BActivated);
    }

    [Fact]
    public void TransitionTo_NoTransition_NewScreenReceivesUpdate()
    {
        var (game, _) = TestGameFactory.Create();
        game.TransitionTo<ScreenB>();
        var ex = Record.Exception(() => game.Update(0.016f));
        Assert.Null(ex);
    }

    // ── TransitionTo (with transition) ────────────────────────────────────

    [Fact]
    public void TransitionTo_WithTransition_OldScreenNotDeactivatedYet()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.TransitionTo<ScreenB>(new DissolveTransition { Duration = 1f });
        Assert.False(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_CompletesAfterDuration()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.TransitionTo<ScreenB>(new DissolveTransition { Duration = 0.5f });
        game.Update(0.5f);
        Assert.True(tracker.ADeactivated);
    }

    [Fact]
    public void TransitionTo_WithTransition_DrawDoesNotThrow()
    {
        var (game, _) = TestGameFactory.Create();
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
        var (game, tracker) = TestGameFactory.Create();
        game.PushOverlay<OverlayScreen>();
        Assert.True(tracker.APaused);
        Assert.True(tracker.AIsPausedWhenPaused);
    }

    [Fact]
    public void PushOverlay_BaseScreenNotUpdated()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.PushOverlay<OverlayScreen>();
        tracker.AUpdateCalled = false; // reset after initial activation
        game.Update(0.016f);
        Assert.False(tracker.AUpdateCalled);
    }

    [Fact]
    public void PopOverlay_ResumesBaseScreen()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.PushOverlay<OverlayScreen>();
        game.PopOverlay();
        Assert.True(tracker.AResumed);
    }

    [Fact]
    public void PopOverlay_WhenEmpty_DoesNothing()
    {
        var (game, _) = TestGameFactory.Create();
        var ex = Record.Exception(() => game.PopOverlay());
        Assert.Null(ex);
    }

    // ── TransitionTo clears overlay stack ─────────────────────────────────

    [Fact]
    public void TransitionTo_ClearsOverlayStack_DeactivatesBase()
    {
        var (game, tracker) = TestGameFactory.Create();
        game.PushOverlay<OverlayScreen>();
        game.TransitionTo<ScreenB>();
        Assert.True(tracker.ADeactivated);
    }
}

// ── GameBuilder tests ─────────────────────────────────────────────────────

public class GameBuilderTests
{
    [Fact]
    public void GameDimensions_DefaultIs800x600()
    {
        var builder = GameBuilder.CreateDefault();
        builder.Screens.Add<OverlayScreen>();
        builder.SetInitialScreen<OverlayScreen>();
        var game = builder.Build();
        Assert.Equal(new SKSize(800, 600), game.GameDimensions);
    }

    [Fact]
    public void GameDimensions_CustomValue_PropagatestoGame()
    {
        var builder = GameBuilder.CreateDefault();
        builder.SetGameDimensions(1200, 600);
        builder.Screens.Add<OverlayScreen>();
        builder.SetInitialScreen<OverlayScreen>();
        var game = builder.Build();
        Assert.Equal(new SKSize(1200, 600), game.GameDimensions);
    }

    [Fact]
    public void Build_NoInitialScreen_Throws()
    {
        var builder = GameBuilder.CreateDefault();
        var ex = Record.Exception(() => builder.Build());
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void Build_WithOneScreen_ReturnsGame()
    {
        var builder = GameBuilder.CreateDefault();
        builder.Screens.Add<OverlayScreen>();
        builder.SetInitialScreen<OverlayScreen>();
        var game = builder.Build();
        Assert.NotNull(game);
    }

    [Fact]
    public void CreateDefault_ExposesScreensCollection()
    {
        var builder = GameBuilder.CreateDefault();
        Assert.NotNull(builder.Screens);
    }

    [Fact]
    public void CreateDefault_ExposesAssetsCollection()
    {
        var builder = GameBuilder.CreateDefault();
        Assert.NotNull(builder.Assets);
    }

    [Fact]
    public void CreateDefault_ExposesConfiguration()
    {
        var builder = GameBuilder.CreateDefault();
        Assert.NotNull(builder.Configuration);
    }

    [Fact]
    public void CreateDefault_ExposesServices()
    {
        var builder = GameBuilder.CreateDefault();
        Assert.NotNull(builder.Services);
    }

    [Fact]
    public void Build_FirstScreen_BecomesInitialScreen()
    {
        // The initial screen is activated immediately by Game's constructor
        var tracker = new ScreenTracker();
        var builder = GameBuilder.CreateDefault();
        builder.Services.AddSingleton(tracker);
        builder.Screens
               .Add<ScreenA>()
               .Add<ScreenB>();
        builder.SetInitialScreen<ScreenA>();
        builder.Build();
        Assert.True(tracker.AActivated);
        Assert.False(tracker.BActivated);
    }

    [Fact]
    public void Services_RegisteredSingleton_IsAvailableToScreens()
    {
        var tracker = new ScreenTracker();
        var builder = GameBuilder.CreateDefault();
        builder.Services.AddSingleton(tracker);
        builder.Screens.Add<ScreenA>();
        builder.SetInitialScreen<ScreenA>();
        var game = builder.Build();
        // ScreenA's constructor took the tracker singleton — it was able to call OnActivated
        Assert.True(tracker.AActivated);
    }

    [Fact]
    public void SetInitialScreen_CanChooseLaterScreen()
    {
        var tracker = new ScreenTracker();
        var builder = GameBuilder.CreateDefault();
        builder.Services.AddSingleton(tracker);
        builder.Screens
               .Add<ScreenA>()
               .Add<ScreenB>();
        builder.SetInitialScreen<ScreenB>();   // explicitly choose a non-first screen
        builder.Build();
        Assert.False(tracker.AActivated);
        Assert.True(tracker.BActivated);
    }

    private sealed class OverlayScreen : GameScreen
    {
        public override void Update(float dt) { }
        public override void Draw(SKCanvas c, int w, int h) { }
    }

    private sealed class ScreenA(ScreenTracker t) : GameScreen
    {
        public override void Update(float dt) { }
        public override void Draw(SKCanvas c, int w, int h) { }
        public override void OnActivated() => t.AActivated = true;
    }

    private sealed class ScreenB(ScreenTracker t) : GameScreen
    {
        public override void Update(float dt) { }
        public override void Draw(SKCanvas c, int w, int h) { }
        public override void OnActivated() => t.BActivated = true;
    }

    private sealed class ScreenTracker
    {
        public bool AActivated { get; set; }
        public bool BActivated { get; set; }
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

// ── FadeToColorTransition tests ────────────────────────────────────────────

public class FadeToColorTransitionTests
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
        var t  = new FadeToColorTransition();
        var ex = Record.Exception(() => t.Draw(MakeCanvas(), progress, _ => { }, _ => { }, 800, 600));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_FirstHalf_CallsOutgoingCallback()
    {
        bool outCalled = false;
        var t = new FadeToColorTransition();
        t.Draw(MakeCanvas(), 0.2f, _ => outCalled = true, _ => { }, 800, 600);
        Assert.True(outCalled);
    }

    [Fact]
    public void Draw_SecondHalf_CallsIncomingCallback()
    {
        bool inCalled = false;
        var t = new FadeToColorTransition();
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
