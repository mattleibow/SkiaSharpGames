using SkiaSharp;
using SkiaSharp.Theatre;
using SkiaSharp.Theatre.Rehearsals;
using SkiaSharpGames.Asteroids;
using SkiaSharpGames.Breakout;
using SkiaSharpGames.CastleAttack;
using SkiaSharpGames.Catch;
using SkiaSharpGames.LunarLander;
using SkiaSharpGames.Pong;
using SkiaSharpGames.SinkSub;
using SkiaSharpGames.Snake;
using SkiaSharpGames.SpaceInvaders;
using SkiaSharpGames.TwoZeroFourEight;
using SkiaSharpGames.UIGallery;
using Xunit;

namespace SkiaSharp.Theatre.Games.Tests;

/// <summary>
/// Smoke tests for every game: run the start scene for 3 seconds,
/// transition to the play scene, then run gameplay for 3 seconds.
/// Asserts that frames render without crashing and produce visible output.
/// </summary>
public class GameSmokeTests
{
    private const float MenuDuration = 3f;
    private const float PlayDuration = 3f;
    private const float DeltaTime = 1f / 60f;

    private static void RunMenuThenPlay(Stage stage)
    {
        using var harness = Rehearsal.FromStage(stage);

        // Run the start/menu scene for 3 seconds
        harness.RunFor(MenuDuration, DeltaTime);

        // Capture menu frame and verify something was drawn
        using var menuFrame = harness.CaptureFrame();
        Assert.True(
            menuFrame.HasNonBackgroundPixel(
                new SKRectI(0, 0, menuFrame.Width, menuFrame.Height),
                SKColors.Transparent
            ),
            "Menu scene should render visible content"
        );

        // Transition to play scene via click (all games support this)
        harness.Click(stage.StageSize.Width / 2f, stage.StageSize.Height / 2f);

        // Let transition complete
        harness.RunFor(1f, DeltaTime);

        // Run gameplay for 3 seconds
        harness.RunFor(PlayDuration, DeltaTime);

        // Capture gameplay frame and verify something was drawn
        using var playFrame = harness.CaptureFrame();
        Assert.True(
            playFrame.HasNonBackgroundPixel(
                new SKRectI(0, 0, playFrame.Width, playFrame.Height),
                SKColors.Transparent
            ),
            "Play scene should render visible content"
        );

        // Menu and play should look different
        float diff = menuFrame.DiffRatio(playFrame);
        Assert.True(diff > 0.001f, $"Menu and play frames should differ (diff={diff:P2})");
    }

    [Fact]
    public void Asteroids_MenuThenPlay() => RunMenuThenPlay(AsteroidsGame.Create());

    [Fact]
    public void Breakout_MenuThenPlay() => RunMenuThenPlay(BreakoutGame.Create());

    [Fact]
    public void CastleAttack_MenuThenPlay() => RunMenuThenPlay(CastleAttackGame.Create());

    [Fact]
    public void Catch_MenuThenPlay() => RunMenuThenPlay(CatchGame.Create());

    [Fact]
    public void LunarLander_MenuThenPlay() => RunMenuThenPlay(LunarLanderGame.Create());

    [Fact]
    public void Pong_MenuThenPlay() => RunMenuThenPlay(PongGame.Create());

    [Fact]
    public void SinkSub_MenuThenPlay() => RunMenuThenPlay(SinkSubGame.Create());

    [Fact]
    public void Snake_MenuThenPlay() => RunMenuThenPlay(SnakeGame.Create());

    [Fact]
    public void SpaceInvaders_MenuThenPlay() => RunMenuThenPlay(SpaceInvadersGame.Create());

    [Fact]
    public void TwoZeroFourEight_MenuThenPlay() => RunMenuThenPlay(TwoZeroFourEightGame.Create());

    [Fact]
    public void UIGallery_PlayScreen()
    {
        // UIGallery has no start scene — it goes directly to PlayScreen
        var stage = UIGalleryGame.Create();
        using var harness = Rehearsal.FromStage(stage);

        harness.RunFor(3f, DeltaTime);

        using var frame = harness.CaptureFrame();
        Assert.True(
            frame.HasNonBackgroundPixel(
                new SKRectI(0, 0, frame.Width, frame.Height),
                SKColors.Transparent
            ),
            "UIGallery should render visible content"
        );
    }
}
