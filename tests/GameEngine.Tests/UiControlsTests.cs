using SkiaSharp;
using SkiaSharpGames.GameEngine.UI;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class UiControlsTests
{
    [Fact]
    public void HitTest_ReturnsExpected()
    {
        var rect = SKRect.Create(10f, 10f, 20f, 20f);
        Assert.True(UiControls.HitTest(rect, 15f, 15f));
        Assert.False(UiControls.HitTest(rect, 1f, 1f));
    }

    [Fact]
    public void DrawButton_CoversDefaultAndCustomPaths()
    {
        using var bitmap = new SKBitmap(200, 120);
        using var canvas = new SKCanvas(bitmap);
        var rect = SKRect.Create(20f, 20f, 140f, 60f);

        UiControls.DrawButton(canvas, rect, "PLAY", UiThemes.Simple.Button, pressed: false, enabled: true);
        UiControls.DrawButton(canvas, rect, "PLAY", UiThemes.Simple.Button with { BevelSize = 0f }, pressed: true, enabled: false);

        bool called = false;
        UiControls.DrawButton(canvas, rect, "PLAY", UiThemes.Simple.Button,
            customDraw: (_, _, _, _, _) => called = true);

        Assert.True(called);
    }

    [Fact]
    public void DrawCheckbox_CoversCheckedUncheckedAndCustomPaths()
    {
        using var bitmap = new SKBitmap(120, 120);
        using var canvas = new SKCanvas(bitmap);
        var rect = SKRect.Create(20f, 20f, 36f, 36f);

        UiControls.DrawCheckbox(canvas, rect, isChecked: false, UiThemes.Simple.Checkbox);
        UiControls.DrawCheckbox(canvas, rect, isChecked: true, UiThemes.Simple.Checkbox);

        bool called = false;
        UiControls.DrawCheckbox(canvas, rect, isChecked: true, UiThemes.Simple.Checkbox,
            customDraw: (_, _, _, _) => called = true);

        Assert.True(called);
    }

    [Fact]
    public void DrawSwitch_CoversSlidingToggleAndCustomPaths()
    {
        using var bitmap = new SKBitmap(220, 120);
        using var canvas = new SKCanvas(bitmap);
        var rect = SKRect.Create(20f, 30f, 120f, 44f);

        UiControls.DrawSwitch(canvas, rect, isOn: false, UiThemes.Simple.Switch, UiSwitchVariant.Sliding);
        UiControls.DrawSwitch(canvas, rect, isOn: true, UiThemes.Simple.Switch, UiSwitchVariant.Sliding);
        UiControls.DrawSwitch(canvas, rect, isOn: true, UiThemes.Simple.Switch, UiSwitchVariant.ToggleButton);

        bool called = false;
        UiControls.DrawSwitch(canvas, rect, isOn: true, UiThemes.Simple.Switch, UiSwitchVariant.Sliding,
            customDraw: (_, _, _, _, _) => called = true);

        Assert.True(called);
    }

    [Fact]
    public void DrawSlider_CoversDefaultAndCustomPaths()
    {
        using var bitmap = new SKBitmap(260, 100);
        using var canvas = new SKCanvas(bitmap);
        var rect = SKRect.Create(20f, 30f, 200f, 20f);

        UiControls.DrawSlider(canvas, rect, -2f, UiThemes.Simple.Slider);
        UiControls.DrawSlider(canvas, rect, 2f, UiThemes.Simple.Slider);

        bool called = false;
        UiControls.DrawSlider(canvas, rect, 0.5f, UiThemes.Simple.Slider,
            customDraw: (_, _, _, _) => called = true);

        Assert.True(called);
    }

    [Fact]
    public void DrawJoystick_CoversDefaultAndCustomPaths()
    {
        using var bitmap = new SKBitmap(260, 260);
        using var canvas = new SKCanvas(bitmap);

        UiControls.DrawJoystick(canvas, new SKPoint(120f, 120f), 60f, SKPoint.Empty, UiThemes.Simple.Joystick);

        bool called = false;
        UiControls.DrawJoystick(canvas, new SKPoint(120f, 120f), 60f, new SKPoint(5f, -5f), UiThemes.Simple.Joystick,
            customDraw: (_, _, _, _, _) => called = true);

        Assert.True(called);
    }

    [Fact]
    public void ClampJoystick_WhenInsideRadius_ReturnsOriginalDelta()
    {
        var delta = new SKPoint(2f, 3f);
        var clamped = UiControls.ClampJoystick(delta, 10f);

        Assert.Equal(delta, clamped);
    }
}
