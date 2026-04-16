using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.Testing;
using Xunit;

namespace SkiaSharpGames.Games.Tests;

/// <summary>
/// Smoke tests for every game: run the start screen for 3 seconds,
/// transition to the play screen, then run gameplay for 3 seconds.
/// Asserts that frames render without crashing and produce visible output.
/// </summary>
public class GameSmokeTests
{
    private const float MenuDuration = 3f;
    private const float PlayDuration = 3f;
    private const float DeltaTime = 1f / 60f;

    private static void RunMenuThenPlay(Game game)
    {
        using var harness = GameTestHarness.FromGame(game);

        // Run the start/menu screen for 3 seconds
        harness.RunFor(MenuDuration, DeltaTime);

        // Capture menu frame and verify something was drawn
        using var menuFrame = harness.CaptureFrame();
        Assert.True(
            menuFrame.HasNonBackgroundPixel(
                new SKRectI(0, 0, menuFrame.Width, menuFrame.Height),
                SKColors.Transparent),
            "Menu screen should render visible content");

        // Transition to play screen via click (all games support this)
        harness.Click(
            game.GameDimensions.Width / 2f,
            game.GameDimensions.Height / 2f);

        // Let transition complete
        harness.RunFor(1f, DeltaTime);

        // Run gameplay for 3 seconds
        harness.RunFor(PlayDuration, DeltaTime);

        // Capture gameplay frame and verify something was drawn
        using var playFrame = harness.CaptureFrame();
        Assert.True(
            playFrame.HasNonBackgroundPixel(
                new SKRectI(0, 0, playFrame.Width, playFrame.Height),
                SKColors.Transparent),
            "Play screen should render visible content");

        // Menu and play should look different
        float diff = menuFrame.DiffRatio(playFrame);
        Assert.True(diff > 0.001f,
            $"Menu and play frames should differ (diff={diff:P2})");
    }

    [Fact]
    public void Asteroids_MenuThenPlay()
        => RunMenuThenPlay(Asteroids.AsteroidsGame.Create());

    [Fact]
    public void Breakout_MenuThenPlay()
        => RunMenuThenPlay(Breakout.BreakoutGame.Create());

    [Fact]
    public void CastleAttack_MenuThenPlay()
        => RunMenuThenPlay(CastleAttack.CastleAttackGame.Create());

    [Fact]
    public void Catch_MenuThenPlay()
        => RunMenuThenPlay(Catch.CatchGame.Create());

    [Fact]
    public void LunarLander_MenuThenPlay()
        => RunMenuThenPlay(LunarLander.LunarLanderGame.Create());

    [Fact]
    public void Pong_MenuThenPlay()
        => RunMenuThenPlay(Pong.PongGame.Create());

    [Fact]
    public void SinkSub_MenuThenPlay()
        => RunMenuThenPlay(SinkSub.SinkSubGame.Create());

    [Fact]
    public void Snake_MenuThenPlay()
        => RunMenuThenPlay(Snake.SnakeGame.Create());

    [Fact]
    public void SpaceInvaders_MenuThenPlay()
        => RunMenuThenPlay(SpaceInvaders.SpaceInvadersGame.Create());

    [Fact]
    public void TwoZeroFourEight_MenuThenPlay()
        => RunMenuThenPlay(TwoZeroFourEight.TwoZeroFourEightGame.Create());

    [Fact]
    public void UIGallery_PlayScreen()
    {
        // UIGallery has no start screen — it goes directly to PlayScreen
        var game = UIGallery.UIGalleryGame.Create();
        using var harness = GameTestHarness.FromGame(game);

        harness.RunFor(3f, DeltaTime);

        using var frame = harness.CaptureFrame();
        Assert.True(
            frame.HasNonBackgroundPixel(
                new SKRectI(0, 0, frame.Width, frame.Height),
                SKColors.Transparent),
            "UIGallery should render visible content");
    }
}
