using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class StageBuilderTests
{
    [Fact]
    public void GameDimensions_DefaultIs800x600()
    {
        var builder = StageBuilder.Create();
        builder.Scenes.Add<OverlayScreen>();
        builder.SetOpeningScene<OverlayScreen>();
        var stage = builder.Open();
        Assert.Equal(new SKSize(800, 600), stage.StageSize);
    }

    [Fact]
    public void GameDimensions_CustomValue_PropagatestoGame()
    {
        var builder = StageBuilder.Create();
        builder.SetStageSize(1200, 600);
        builder.Scenes.Add<OverlayScreen>();
        builder.SetOpeningScene<OverlayScreen>();
        var stage = builder.Open();
        Assert.Equal(new SKSize(1200, 600), stage.StageSize);
    }

    [Fact]
    public void Build_NoInitialScreen_Throws()
    {
        var builder = StageBuilder.Create();
        var ex = Record.Exception(() => builder.Open());
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void Build_WithOneScreen_ReturnsGame()
    {
        var builder = StageBuilder.Create();
        builder.Scenes.Add<OverlayScreen>();
        builder.SetOpeningScene<OverlayScreen>();
        var stage = builder.Open();
        Assert.NotNull(stage);
    }

    [Fact]
    public void CreateDefault_ExposesScreensCollection()
    {
        var builder = StageBuilder.Create();
        Assert.NotNull(builder.Scenes);
    }

    [Fact]
    public void CreateDefault_ExposesAssetsCollection()
    {
        var builder = StageBuilder.Create();
        Assert.NotNull(builder.Props);
    }

    [Fact]
    public void CreateDefault_ExposesConfiguration()
    {
        var builder = StageBuilder.Create();
        Assert.NotNull(builder.Configuration);
    }

    [Fact]
    public void CreateDefault_ExposesServices()
    {
        var builder = StageBuilder.Create();
        Assert.NotNull(builder.Services);
    }

    [Fact]
    public void Build_FirstScreen_BecomesInitialScreen()
    {
        // The initial scene is activated immediately by Build()
        var tracker = new ScreenTracker();
        var builder = StageBuilder.Create();
        builder.Services.AddSingleton(tracker);
        builder.Scenes
               .Add<ScreenA>()
               .Add<ScreenB>();
        builder.SetOpeningScene<ScreenA>();
        builder.Open();
        Assert.True(tracker.AActivated);
        Assert.False(tracker.BActivated);
    }

    [Fact]
    public void Services_RegisteredSingleton_IsAvailableToScreens()
    {
        var tracker = new ScreenTracker();
        var builder = StageBuilder.Create();
        builder.Services.AddSingleton(tracker);
        builder.Scenes.Add<ScreenA>();
        builder.SetOpeningScene<ScreenA>();
        var stage = builder.Open();
        // ScreenA's constructor took the tracker singleton — it was able to call OnActivated
        Assert.True(tracker.AActivated);
    }

    [Fact]
    public void SetInitialScreen_CanChooseLaterScreen()
    {
        var tracker = new ScreenTracker();
        var builder = StageBuilder.Create();
        builder.Services.AddSingleton(tracker);
        builder.Scenes
               .Add<ScreenA>()
               .Add<ScreenB>();
        builder.SetOpeningScene<ScreenB>(); // explicitly choose a non-first scene
        builder.Open();
        Assert.False(tracker.AActivated);
        Assert.True(tracker.BActivated);
    }

    [Fact]
    public void Build_ExposesServicesOnGame()
    {
        var builder = StageBuilder.Create();
        builder.Scenes.Add<OverlayScreen>();
        builder.SetOpeningScene<OverlayScreen>();
        var stage = builder.Open();
        Assert.NotNull(stage.Services);
    }

    [Fact]
    public void Build_ServicesContainsIDirector()
    {
        var builder = StageBuilder.Create();
        builder.Scenes.Add<OverlayScreen>();
        builder.SetOpeningScene<OverlayScreen>();
        var stage = builder.Open();
        var director = stage.Services.GetService<IDirector>();
        Assert.NotNull(director);
    }

    private sealed class OverlayScreen : Scene
    {
        public override void Update(float dt) { }
        public override void Draw(SKCanvas c, int w, int h) { }
    }

    private sealed class ScreenA(ScreenTracker t) : Scene
    {
        public override void Update(float dt) { }
        public override void Draw(SKCanvas c, int w, int h) { }
        public override void OnActivated() => t.AActivated = true;
    }

    private sealed class ScreenB(ScreenTracker t) : Scene
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
