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
    public void SliderValueFromX_ClampsToRange()
    {
        var rect = SKRect.Create(10f, 10f, 100f, 20f);

        Assert.Equal(0f, UiControls.SliderValueFromX(rect, -100f));
        Assert.Equal(0.5f, UiControls.SliderValueFromX(rect, 60f), 3);
        Assert.Equal(1f, UiControls.SliderValueFromX(rect, 1000f));
    }

    [Fact]
    public void ClampJoystick_ClampsToRadius()
    {
        var clamped = UiControls.ClampJoystick(new SKPoint(30f, 40f), 10f);
        float length = MathF.Sqrt(clamped.X * clamped.X + clamped.Y * clamped.Y);

        Assert.Equal(10f, length, 3);
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
