using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class UiThemeTests
{
    [Fact]
    public void Build_RegistersDefaultTheme()
    {
        var stage = BuildBareGame();
        var theme = stage.Services.GetRequiredService<UiTheme>();

        Assert.IsType<UiButtonAppearance>(theme.Button);
    }

    [Fact]
    public void SetUiTheme_OverridesDefaultTheme()
    {
        var retro = new UiTheme();
        retro.ApplyFrom(UiThemes.Retro);

        var builder = StageBuilder.Create();
        builder.Scenes.Add<BlankScreen>();
        builder.SetOpeningScene<BlankScreen>();
        builder.SetUiTheme(retro);

        var stage = builder.Open();
        var theme = stage.Services.GetRequiredService<UiTheme>();

        Assert.Same(retro, theme);
    }

    [Fact]
    public void ClampJoystick_ClampsToRadius()
    {
        var clamped = UiJoystick.ClampJoystick(new SKPoint(30f, 40f), 10f);
        float length = MathF.Sqrt(clamped.X * clamped.X + clamped.Y * clamped.Y);

        Assert.Equal(10f, length, 3);
    }

    [Fact]
    public void Theme_Button_IsUiButtonAppearance()
    {
        Assert.IsType<UiButtonAppearance>(UiThemes.Simple.Button);
        Assert.IsType<UiButtonAppearance>(UiThemes.BoldCute.Button);
        Assert.IsType<UiButtonAppearance>(UiThemes.Retro.Button);
    }

    [Fact]
    public void Theme_HasPointerAppearance()
    {
        Assert.IsType<UiCrosshairAppearance>(UiThemes.Simple.Spotlight);
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
        public override void Draw(SKCanvas canvas, int width, int height) { }
    }
}
