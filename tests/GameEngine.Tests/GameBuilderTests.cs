using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

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
        // The initial screen is activated immediately by Build()
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
        builder.SetInitialScreen<ScreenB>(); // explicitly choose a non-first screen
        builder.Build();
        Assert.False(tracker.AActivated);
        Assert.True(tracker.BActivated);
    }

    [Fact]
    public void Build_ExposesServicesOnGame()
    {
        var builder = GameBuilder.CreateDefault();
        builder.Screens.Add<OverlayScreen>();
        builder.SetInitialScreen<OverlayScreen>();
        var game = builder.Build();
        Assert.NotNull(game.Services);
    }

    [Fact]
    public void Build_ServicesContainsIScreenCoordinator()
    {
        var builder = GameBuilder.CreateDefault();
        builder.Screens.Add<OverlayScreen>();
        builder.SetInitialScreen<OverlayScreen>();
        var game = builder.Build();
        var coordinator = game.Services.GetService<IScreenCoordinator>();
        Assert.NotNull(coordinator);
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
