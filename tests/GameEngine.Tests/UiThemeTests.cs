using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class UiThemeTests
{
    [Fact]
    public void Build_RegistersSimpleThemeByDefault()
    {
        var game = BuildBareGame();
        var provider = game.Services.GetRequiredService<IUiThemeProvider>();

        Assert.Equal(UiThemes.Simple.Name, provider.Theme.Name);
    }

    [Fact]
    public void SetUiTheme_OverridesDefaultTheme()
    {
        var builder = GameBuilder.CreateDefault();
        builder.Screens.Add<BlankScreen>();
        builder.SetInitialScreen<BlankScreen>();
        builder.SetUiTheme(UiThemes.Retro);

        var game = builder.Build();
        var provider = game.Services.GetRequiredService<IUiThemeProvider>();

        Assert.Equal(UiThemes.Retro.Name, provider.Theme.Name);
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
        Assert.IsType<UiPointerAppearance>(UiThemes.Simple.Pointer);
    }

    private static Game BuildBareGame()
    {
        var builder = GameBuilder.CreateDefault();
        builder.Screens.Add<BlankScreen>();
        builder.SetInitialScreen<BlankScreen>();
        return builder.Build();
    }

    private sealed class BlankScreen : GameScreen
    {
        public override void Draw(SKCanvas canvas, int width, int height) { }
    }
}
