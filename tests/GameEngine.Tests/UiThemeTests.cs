using SkiaSharp;
using SkiaSharp.Theatre;
using SkiaSharp.Theatre.Themes.Default;

using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class HudThemeTests
{
    [Fact]
    public void DefaultTheme_HasButtonAppearance()
    {
        var theme = DefaultTheme.Create();
        Assert.IsType<DefaultButtonAppearance>(theme.Button);
    }

    [Fact]
    public void Stage_HudTheme_CanBeSet()
    {
        var stage = BuildBareGame();
        stage.HudTheme = DefaultTheme.Create();
        Assert.NotNull(stage.HudTheme);
    }

    [Fact]
    public void ClampJoystick_ClampsToRadius()
    {
        var clamped = HudJoystick.ClampJoystick(new SKPoint(30f, 40f), 10f);
        float length = MathF.Sqrt(clamped.X * clamped.X + clamped.Y * clamped.Y);

        Assert.Equal(10f, length, 3);
    }

    [Fact]
    public void Theme_Button_IsDefaultButtonAppearance()
    {
        Assert.IsType<DefaultButtonAppearance>(DefaultTheme.Create().Button);
        Assert.IsType<DefaultButtonAppearance>(BoldCuteTheme.Create().Button);
        Assert.IsType<DefaultButtonAppearance>(RetroTheme.Create().Button);
    }

    [Fact]
    public void Theme_HasPointerAppearance()
    {
        Assert.IsType<DefaultCrosshairAppearance>(DefaultTheme.Create().Pointer);
    }

    private static Stage BuildBareGame()
    {
        var builder = StageBuilder.Create();
        builder.Scenes.Add<BlankScreen>();
        builder.SetOpeningScene<BlankScreen>();
        return builder.Open();
    }

    private sealed class BlankScreen : Scene
    {
        protected override void OnDraw(SKCanvas canvas) { }
    }
}
